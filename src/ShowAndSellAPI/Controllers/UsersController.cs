using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ShowAndSellAPI.Models.Database;
using ShowAndSellAPI.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowAndSellAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly SSDbContext _context;

        public UsersController(SSDbContext context)
        {
            _context = context;
        }

        //  Likely won't need a method to get all users (security)
        // GET: api/values
        [HttpGet]
        public IEnumerable<SSGroup> GetAll()
        {
            return _context.Groups.ToArray();
        }

        // GET api/users/id
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(string id)
        {
            SSUser user = _context.Users.Where(e => e.SSUserId == id).FirstOrDefault();
            if (user == null) return NotFound();

            return new ObjectResult(user);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
