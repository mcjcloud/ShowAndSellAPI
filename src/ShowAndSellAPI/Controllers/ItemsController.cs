using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ShowAndSellAPI.Models.Database;
using ShowAndSellAPI.Models;
using ShowAndSellAPI.Models.Http;
using Microsoft.AspNetCore.Hosting;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ShowAndSellAPI.Controllers
{
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private readonly SSDbContext _context;
        private readonly IHostingEnvironment _environment;

        public ItemsController(SSDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _environment = env;
        }

        // GET: api/items
        [HttpGet]
        public IEnumerable<SSItem> Query([FromQuery]string name, [FromQuery]string groupId)
        {
            // if group id is specified
            if(groupId != null && groupId != "")
            {
                // group id and query
                if(name != null && name != "")
                {
                    return _context.GetItems(name, groupId);
                }
                // just groupid
                else
                {
                    return _context.GetItems(groupId);
                }
            }
            // just query
            else if(name != null && name != "")
            {
                return _context.SearchItems(name);
            }
            
            // all of them are null, return all items.
            return _context.Items.ToArray();
        }

        // GET api/items/id
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            return _context.GetItem(id);
        }

        // POST api/values create a new item
        [HttpPost]
        public IActionResult CreateItem([FromBody]SSItem item)
        {
            return _context.AddItem(item);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult UpdateItem(string id, [FromBody]UpdateItemRequest request, [FromQuery]string groupOwnerPassword)
        {
            return _context.UpdateItem(id, request, groupOwnerPassword);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id, [FromQuery]string ownerPassword)
        {
            return _context.DeleteItem(id, ownerPassword);
        }
    }
}
