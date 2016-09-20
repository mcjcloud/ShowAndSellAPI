using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ShowAndSellAPI.Models.Database;
using ShowAndSellAPI.Models;
using ShowAndSellAPI.Models.Http;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Microsoft.Win32.SafeHandles;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowAndSellAPI.Controllers
{
    [Route("api/[controller]")]
    public class GroupsController : Controller
    {
        // Properties
        private readonly SSDbContext _context;

        // CONSTRUCTOR
        public GroupsController(SSDbContext context)
        {
            _context = context;
        }


        // GET: api/groups
        [HttpGet]
        public IEnumerable<SSGroup> GetAll()
        {
            return _context.Groups.ToArray<SSGroup>();
        }

        // GET api/groups/id
        [HttpGet("{id}", Name = "GetGroup")]
        public IActionResult GetGroup(string id)
        {
            SSGroup group = _context.Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            if(group == null) return NotFound();

            return new ObjectResult(group);
        }

        // POST api/groups
        // TODO: make sure this works with authentication
        [HttpPost]
        public IActionResult Add([FromBody]AddGroupRequest groupRequest)
        {
            if (groupRequest == null) return BadRequest();

            // if no admin was specified.
            // bool is if there is insufficient data entered.
            bool invalidRequest = groupRequest.Group.Admin == null || groupRequest.Group.Admin == "" || groupRequest.Group.Name == null || groupRequest.Group.Name == "" || groupRequest.Password == null;
            if(invalidRequest)
            {
                string errorMessage = "";
                using(StreamReader reader = new StreamReader(System.IO.File.OpenRead("Models/Http/Messages/AddGroup449.txt")))
                {
                    errorMessage += reader.ReadLine();
                }

                return StatusCode(449, errorMessage);
            }

            // check if user exists
            SSUser admin = _context.Users.Where(e => e.SSUserId == groupRequest.Group.Admin).FirstOrDefault();
            if(admin == null) return NotFound();

            // check password
            string realPassword = admin.Password;
            if(groupRequest.Password != realPassword) return StatusCode(403, "Admin ID or password is incorrect.");

            // add the group to the database.
            groupRequest.Group.SSGroupId = Guid.NewGuid().ToString();
            groupRequest.Group.DateCreated = DateTime.Now;
            _context.Groups.Add(groupRequest.Group);
            _context.SaveChanges();

            return CreatedAtRoute("GetGroup", new { id = groupRequest.Group.SSGroupId }, groupRequest.Group);
        }

        // PUT api/groups/id update a group in the database
        // TODO: make sure this method works (have to add users first).
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody]UpdateGroupRequest groupRequest)
        {
            SSGroup groupToUpdate = _context.Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            SSUser admin = _context.Users.Where(e => e.SSUserId == groupToUpdate.Admin).FirstOrDefault();
            if(groupToUpdate == null || admin == null) return NotFound();

            // authenticate/authorize
            if(admin.Password != groupRequest.Password) return StatusCode(403, "Invalid id or password.");

            groupToUpdate.Name = groupRequest.NewName;
            _context.SaveChanges();
            return StatusCode(200, "Group with GUID " + id + " has been updated.");
        }

        // DELETE api/group/id
        [HttpDelete("{id}")]
        public IActionResult Delete(string id, [FromBody]DeleteGroupRequest groupRequest)
        {
            SSGroup groupToDelete = _context.Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            SSUser admin = _context.Users.Where(e => e.SSUserId == groupToDelete.Admin).FirstOrDefault();

            if (groupToDelete == null || admin == null) return NotFound();
            if (admin.Password == null) return StatusCode(500, "Internal Server Error :(");

            // if not authorized, return 403
            if(admin.Password != groupRequest.Password) return StatusCode(403, "Invalid group or password");

            // remove the group
            _context.Remove(groupToDelete);
            _context.SaveChanges();
            return StatusCode(200, "Group removed");
        }
    }
}
