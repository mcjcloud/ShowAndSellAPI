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
    [Route("api/[controller]/[action]")]
    public class GroupsController : Controller
    {
        // Properties
        private readonly SSDbContext _context;

        // CONSTRUCTOR
        public GroupsController(SSDbContext context)
        {
            _context = context;
        }

        // /api/groups/allgroups
        // GET all Groups on the server.
        [HttpGet]
        public IActionResult AllGroups()
        {
            IEnumerable<SSGroup> groups = _context.Groups.ToArray();
            if (groups != null && groups.Count() > 0) return new ObjectResult(groups);
            else return NotFound("No Groups found.");
        }
        // /api/groups/group?id={group id}
        // GET a group with the given Group ID.
        [HttpGet]
        public IActionResult Group([FromQuery]string id)
        {
            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(id)).FirstOrDefault();
            if(group != null)
            {
                return new ObjectResult(group);
            }
            else    // group not found
            {
                return NotFound("Group with id " + id + " not found.");
            }
        }
        // /api/groups/groupwithadmin?adminId={id of group admin}
        // GET the group with the given user as Admin
        [HttpGet]
        public IActionResult GroupWithAdmin([FromQuery]string adminId)
        {
            SSGroup group = _context.Groups.Where(e => e.AdminId.Equals(adminId)).FirstOrDefault();
            if (group == null) return NotFound("Group not found with Admin ID " + adminId);
            else return new ObjectResult(group);
        }
        // /api/groups/search
        // GET an array of Groups whose names contain the given string.
        [HttpGet]
        public IActionResult Search([FromQuery]string name)
        {
            IEnumerable<SSGroup> groups = _context.Groups.Where(e => e.Name.Contains(name)).ToArray();
            if (groups != null && groups.Count() > 0) return new ObjectResult(groups);
            else return NotFound("No groups containing the name " + name + " found.");
        }

        // /api/groups/create
        // POST a new Group to the server.
        [HttpPost]
        public IActionResult Create([FromBody]AddGroupRequest groupRequest)
        {
            if (groupRequest == null) return BadRequest("Missing or invalid body");

            // if no admin was specified.
            // bool is if there is insufficient data entered.
            bool invalidRequest = groupRequest.Group.AdminId == null || groupRequest.Group.AdminId == "" || groupRequest.Group.Name == null || groupRequest.Group.Name == "" || groupRequest.Password == null || groupRequest.Group.Location == "" || groupRequest.Group.Location == null || groupRequest.Group.LocationDetail == "" || groupRequest.Group.LocationDetail == null;
            if (invalidRequest) return StatusCode(449, "Some fields missing or invalid.");

            // check if user exists
            SSUser admin = _context.Users.Where(e => e.SSUserId == groupRequest.Group.AdminId).FirstOrDefault();
            if (admin == null) return NotFound("Error creating group. Admin with ID " + groupRequest.Group.AdminId + " not found.");

            // check password
            string realPassword = admin.Password;
            if (groupRequest.Password != realPassword) return Unauthorized();

            // check if group name is taken, or if admin already has a group.
            foreach (SSGroup group in _context.Groups.ToArray())
            {
                if (group.AdminId == admin.SSUserId) return BadRequest("Group under admin " + admin.Username + " already exists.");
                if (group.Name == groupRequest.Group.Name) return BadRequest("Group with name " + groupRequest.Group.Name + " already exists.");
            }

            // add the group to the database.
            groupRequest.Group.SSGroupId = Guid.NewGuid().ToString();
            groupRequest.Group.DateCreated = DateTime.Now;
            _context.Groups.Add(groupRequest.Group);
            _context.SaveChanges();

            return new ObjectResult(groupRequest.Group);
        }

        // /api/groups/update?id={group id}
        // PUT a Group in the server with the new data.
        [HttpPut]
        public IActionResult Update([FromQuery]string id, [FromBody]UpdateGroupRequest groupRequest)
        {
            SSGroup groupToUpdate = _context.Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            SSUser admin = _context.Users.Where(e => e.SSUserId == groupToUpdate.AdminId).FirstOrDefault();
            if (groupToUpdate == null) return NotFound("Group not found.");
            if (admin == null) return NotFound("Admin not found.");

            // authenticate/authorize
            if (admin.Password != groupRequest.Password) return Unauthorized();

            // check if group name is taken, or if admin already has a group.
            foreach (SSGroup group in _context.Groups.ToArray())
            {
                if (group.AdminId == admin.SSUserId) return BadRequest("Group with admin " + admin.Username + " already exists.");
                if (group.Name == groupRequest.NewName) return BadRequest("Group with name " + groupRequest.NewName + " already exists.");
            }

            // check if fields filled
            bool valid = groupRequest.NewLocation.Count() > 0 && groupRequest.NewLocationDetail.Count() > 0 && groupRequest.NewName.Count() > 0;
            if (!valid) return StatusCode(449, "Some fields missing or invalid.");

            groupToUpdate.Name = groupRequest.NewName;
            groupToUpdate.LocationDetail = groupRequest.NewLocationDetail;
            groupToUpdate.Location = groupRequest.NewLocation;
            _context.Update(groupToUpdate);
            _context.SaveChanges();
            return new ObjectResult(groupToUpdate);
        }

        // /api/groups/delete?id={group id}&password={admin password}
        // DELETE a Group from the server.
        [HttpDelete]
        public IActionResult Delete([FromQuery]string id, [FromQuery]string password)
        {
            SSGroup groupToDelete = _context.Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            if (groupToDelete == null) return NotFound("Group not found.");

            SSUser admin = _context.Users.Where(e => e.SSUserId == groupToDelete.AdminId).FirstOrDefault();
            if (admin == null) return NotFound("Admin not found.");

            // if not authorized, return 401
            if (admin.Password != password) return Unauthorized();

            // check for items to delete.
            IList<SSItem> items = _context.Items.Where(e => e.GroupId == groupToDelete.SSGroupId).ToList();
            foreach (SSItem item in items)
            {
                // remove bookmarks.
                foreach (var bookmark in _context.Bookmarks.Where(e => e.ItemId.Equals(item.SSItemId)).ToArray())
                {
                    _context.Remove(bookmark);
                }
                // remove chat
                foreach (var message in _context.Messages.Where(e => e.ItemId.Equals(item.SSItemId)).ToArray())
                {
                    _context.Remove(message);
                }

                _context.Remove(item);
            }

            // remove the group
            _context.Remove(groupToDelete);
            _context.SaveChanges();
            // returned the removed group
            return new ObjectResult(groupToDelete);
        }
    }
}
