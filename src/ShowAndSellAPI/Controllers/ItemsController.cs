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
    public class ItemsController : Controller
    {
        private readonly SSDbContext _context;

        public ItemsController(SSDbContext context)
        {
            _context = context;
        }

        // GET: api/items
        [HttpGet]
        public IEnumerable<SSItem> Query([FromQuery]string query, [FromQuery]string groupId)
        {
            // if group id is specified
            if(groupId != null)
            {
                // group id and query
                if(query != null)
                {
                    return _context.GetItems(query, groupId);
                }
                // just groupid
                else
                {
                    return _context.GetItems(groupId);
                }
            }
            // just query
            else if(query != null)
            {
                return _context.SearchItems(query);
            }
            
            // all of them are null.
            return null;
        }

        // GET api/items/id
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return _context.GetItem(id);
        }

        // POST api/values create a new item
        [HttpPost]
        public void Post([FromBody]SSItem item)
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
