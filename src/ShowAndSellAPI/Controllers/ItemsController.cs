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
    [Route("api/[controller]/[action]")]
    public class ItemsController : Controller
    {
        private readonly SSDbContext _context;
        private readonly IHostingEnvironment _environment;

        public ItemsController(SSDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _environment = env;
        }

        // /api/items/allitems
        // GET all Items on the server
        [HttpGet]
        public IActionResult AllItems()
        {
            IEnumerable<SSItem> items = _context.Items.ToArray();
            if (items != null) return new ObjectResult(items);
            else return NotFound("No Items found."); 

        }
        // api/items/searchgroup?name={name}&groupId={group id}
        // GET an array of Items with a given name, in a given Group.
        [HttpGet]
        public IActionResult SearchGroup([FromQuery]string name, [FromQuery]string groupId)
        {
            IEnumerable<SSItem> items = _context.Items.Where(e => e.Name.Contains(name) && e.GroupId.Equals(groupId)).ToArray();
            if (items.Count() <= 0) return NotFound("No Items matching name in the specified Group.");
            else return new ObjectResult(items);
        }
        // api/items/search?name={name}
        // GET an array of Items from any Group with the given name.
        [HttpGet]
        public IActionResult Search([FromQuery]string name)
        {
            IEnumerable<SSItem> items = _context.Items.Where(e => e.Name.Contains(name)).ToArray();
            if (items.Count() <= 0) return NotFound("No Items matching name.");
            else return new ObjectResult(items);
        }
        // /api/items/items?groupId={group id}
        // GET an array of Items in a given Group
        [HttpGet]
        public IActionResult Items([FromQuery]string groupId)
        {
            // check group
            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(groupId)).FirstOrDefault();
            if (group == null) return NotFound("No group withs specified ID.");

            // check items count.
            IEnumerable<SSItem> items = _context.Items.Where(e => e.GroupId.Equals(groupId)).ToArray();
            if (items.Count() <= 0) return NotFound("No Items in group.");
            else return new ObjectResult(items);
        }
        // /api/items/items?groupId={group id}&start={start (int)}&end={end (int)}
        // GET an array of Items in a given Group, from index start (inclusive) to end (exclusive)
        [HttpGet]
        public IActionResult Items([FromQuery]string groupId, [FromQuery]int start, [FromQuery]int end)
        {
            int finish = (end >= 0 && end <= _context.Items.Count()) ? end : _context.Items.Count();    // end > 0 and less than equal to the array size, default to 0.
            int begin = (start >= 0 && start <= finish) ? start : 0;                                    // make sure start is greater than 0 and lessequal to end. Default to 0

            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(groupId)).FirstOrDefault();
            if (group == null) return NotFound("Group not found.");
            else
            {
                IEnumerable<SSItem> items = new List<SSItem>();
                for(int i = begin; i < finish; i++)
                {
                    items.Append(_context.Items.ToArray().GetValue(i));
                }

                // check size
                if(items.Count() <= 0)
                {
                    return NotFound("No Items found in the specified Group.");
                }

                return new ObjectResult(items);
            }
        }
        // /api/items/approved?groupId={group id}
        // GET approved Items in the given Group
        [HttpGet]
        public IActionResult Approved([FromQuery]string groupId)
        {
            IEnumerable<SSItem> items = _context.Items.Where(e => e.GroupId.Equals(groupId) && e.Approved).ToArray();
            if (items.Count() <= 0) return NotFound("No approved Items.");
            else return new ObjectResult(items);
        }
        // /api/items/item?id={item id}
        // GET an Item with the given id
        [HttpGet]
        public IActionResult Item([FromQuery]string id)
        {
            SSItem itemToReturn = _context.Items.Where(e => e.SSItemId.Equals(id)).FirstOrDefault();
            if(itemToReturn != null)
            {
                return new ObjectResult(itemToReturn);
            }
            else
            {
                return NotFound("Item with id " + id + " not found");
            }
        }
        // /api/items/approved?groupId={group id}&start={start (int)}&end={end (int)}
        // GET an array of approved Items in a given Group, from index start (inclusive) to end (exclusive)
        [HttpGet]
        public IActionResult Approved([FromQuery]string groupId, [FromQuery]int start, [FromQuery]int end)
        {
            int finish = (end >= 0 && end <= _context.Items.Count()) ? end : _context.Items.Count();    // end > 0 and less than equal to the array size, default to 0.
            int begin = (start >= 0 && start <= finish) ? start : 0;                                    // make sure start is greater than 0 and lessequal to end. Default to 0

            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(groupId)).FirstOrDefault();
            if (group == null) return NotFound("Group not found.");
            else
            {
                IEnumerable<SSItem> approvedItems = _context.Items.Where(e => e.Approved);
                IEnumerable<SSItem> items = new List<SSItem>();
                for (int i = begin; i < finish; i++)
                {
                    items.Append(approvedItems.ToArray().GetValue(i));
                }

                // check size
                if (items.Count() <= 0)
                {
                    return NotFound("No approved Items found in the specified Group.");
                }

                return new ObjectResult(items);
            }
        }

        // /api/items/create
        // POST an Item
        [HttpPost]
        public IActionResult Create([FromBody]SSItem item)
        {
            // check that the item is valid.
            if (item == null) return BadRequest("Error with item.");
            if (item.Name == null || item.Name == "") return StatusCode(449, "Item name missing or invalid.");
            // check group id
            SSGroup group = _context.Groups.Where(e => e.SSGroupId == item.GroupId).FirstOrDefault();
            if (group == null) return StatusCode(449, "Group missing or invalid.");
            // check owner id
            SSUser owner = _context.Users.Where(e => e.SSUserId == item.OwnerId).FirstOrDefault();
            if (owner == null) return StatusCode(449, "Owner missing or invalid.");

            // check if other data is null
            var valid = item.Name != null && item.Condition != null && item.Description != null && item.Thumbnail != null && item.GroupId != null && item.OwnerId != null;
            if (!valid) return StatusCode(449, "Some fields missing or invalid.");

            // finalize the item and add it to the database.
            item.SSItemId = Guid.NewGuid().ToString();
            item.Approved = false;
            _context.Items.Add(item);
            _context.SaveChanges();

            // return the item as a JSON
            return new ObjectResult(item);

        }

        // /api/items/update?id={item id}&adminPassword={group admin password}
        // PUT update an Item
        [HttpPut]
        public IActionResult Update([FromQuery]string id, [FromBody]UpdateItemRequest itemRequest, [FromQuery]string password)
        {
            SSItem itemToUpdate = _context.Items.Where(e => e.SSItemId == id).FirstOrDefault();

            // check if password is correct
            SSGroup group = _context.Groups.Where(e => e.SSGroupId == itemToUpdate.GroupId).FirstOrDefault();
            SSUser admin = _context.Users.Where(e => e.SSUserId == group.Admin).FirstOrDefault();
            string pass = admin.Password;
            if (pass != password) return Unauthorized();

            // check if fields are filled out.
            bool valid = itemRequest.NewName != null && itemRequest.NewPrice != null && itemRequest.NewCondition != null && itemRequest.NewDescription != null && itemRequest.NewThumbnail != null;
            if (!valid) return StatusCode(449, "Some fields are missing or invalid.");

            // set item properties
            itemToUpdate.Name = itemRequest.NewName;
            itemToUpdate.Price = itemRequest.NewPrice;
            itemToUpdate.Condition = itemRequest.NewCondition;
            itemToUpdate.Description = itemRequest.NewDescription;
            itemToUpdate.Thumbnail = itemRequest.NewThumbnail;
            itemToUpdate.Approved = itemRequest.Approved;

            // update and save changes
            _context.Update(itemToUpdate);
            _context.SaveChanges();
            // return the updated object.
            return new ObjectResult(itemToUpdate);
        }

        // /api/items/delete?id={item id}&password={item owner or admin password}
        // DELETE an Item with the given id, and the password of the Owner or Group administrator.
        [HttpDelete]
        public IActionResult Delete([FromQuery]string id, [FromQuery]string password)
        {
            SSItem itemToDelete = _context.Items.Where(e => e.SSItemId == id).FirstOrDefault();
            if (itemToDelete == null) return NotFound("Item with specified ID not found.");

            SSGroup itemGroup = _context.Groups.Where(e => e.SSGroupId == itemToDelete.GroupId).FirstOrDefault();

            SSUser owner = _context.Users.Where(e => e.SSUserId == itemToDelete.OwnerId).FirstOrDefault();
            SSUser groupAdmin = _context.Users.Where(e => e.SSUserId == itemGroup.Admin).FirstOrDefault();
            // check authentication (owner password or poster's password.
            if (owner.Password != password && groupAdmin.Password != password) return Unauthorized();

            // remove bookmarks.
            foreach(var bookmark in _context.Bookmarks.Where(e => e.ItemId.Equals(itemToDelete.SSItemId)).ToArray()) 
            {
                _context.Remove(bookmark);
            }

            // delete the item and return the object that was deleted.
            _context.Remove(itemToDelete);
            _context.SaveChanges();
            return new ObjectResult(itemToDelete);
        }
    }
}
