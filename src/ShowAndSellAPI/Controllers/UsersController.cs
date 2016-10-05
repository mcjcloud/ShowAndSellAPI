using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ShowAndSellAPI.Models.Database;
using ShowAndSellAPI.Models;
using System.IO;
using ShowAndSellAPI.Models.Http;
using Microsoft.AspNetCore.WebUtilities;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowAndSellAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private const string ADMIN_PASSWORD = "donut";
        private readonly SSDbContext _context;

        public UsersController(SSDbContext context)
        {
            _context = context;
        }

        //  Likely won't need a method to get all users (security)
        // GET: api/values
        [HttpGet]
        public IEnumerable<SSUser> Query([FromQuery]string adminPass, string name, string username, string password)
        {
            // they put the admin pass, then return 
            if(adminPass != null)
            {
                if (adminPass == ADMIN_PASSWORD)
                    return _context.GetUsers();
            }
            //
            if(name != null)
            {
                return _context.GetUsersByName(name);
            }

            if(username != null && password != null)
            {
                return _context.GetUserByUsername(username, password);
            }
             
            return null;
        }

        // GET api/users/id?password=pass
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(string id, [FromQuery]string password)
        {
            return _context.GetUser(id, password);
        }

        // POST api/users Create a user object in the database.
        [HttpPost]
        public IActionResult CreateUser([FromBody]SSUser user)
        {
            return _context.AddUser(user);
        }

        // PUT api/users/id
        [HttpPut("{id}")]
        public IActionResult UpdateUser(string id, [FromBody]UpdateUserRequest updateRequest)
        {
            return _context.UpdateUser(id, updateRequest);
        }

        // DELETE api/users/id
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(string id, [FromBody]DeleteUserRequest deleteRequest)
        {
            return _context.DeleteUser(id, deleteRequest);
        }
    }
}
