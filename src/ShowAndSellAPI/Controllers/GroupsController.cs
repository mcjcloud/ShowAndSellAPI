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
        public IEnumerable<SSGroup> Query(string name)
        {
            // check if name is specified
            if (name != null)
            {
                return new SSGroup[] { _context.GetGroupByName(name) };
            }

            return _context.GetGroups();
        }

        // GET api/groups/id
        [HttpGet("{id}", Name = "GetGroup")]
        public IActionResult GetGroup(string id)
        {
            return new ObjectResult(_context.GetGroup(id));
        }

        // POST api/groups
        // TODO: make sure this works with authentication
        [HttpPost]
        public IActionResult CreateGroup([FromBody]AddGroupRequest groupRequest)
        {
            return _context.AddGroup(groupRequest);
        }

        // PUT api/groups/id update a group in the database
        // TODO: make sure this method works (have to add users first).
        [HttpPut("{id}")]
        public IActionResult UpdateGroup(string id, [FromBody]UpdateGroupRequest groupRequest)
        {
            return _context.UpdateGroup(id, groupRequest);
        }

        // DELETE api/group/id
        [HttpDelete("{id}")]
        public IActionResult DeleteGroup(string id, [FromBody]DeleteGroupRequest groupRequest)
        {
            return _context.DeleteGroup(id, groupRequest);
        }
    }
}
