﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using ShowAndSellAPI.Models.Database;
using ShowAndSellAPI.Models;
using ShowAndSellAPI.Models.Http;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using System.Diagnostics;

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
        // /api/items/itemsinrange?groupId={group id}&start={start (int)}&end={end (int)}
        // GET an array of Items in a given Group, from index start (inclusive) to end (exclusive)
        [HttpGet]
        public IActionResult ItemsInRange([FromQuery]string groupId, [FromQuery]int start, [FromQuery]int end)
        {
            int finish = (end >= 0 && end <= _context.Items.Count()) ? end : _context.Items.Count();    // end > 0 and less than equal to the array size, default to 0.
            int begin = (start >= 0 && start <= finish) ? start : 0;                                    // make sure start is greater than 0 and lessequal to end. Default to 0

            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(groupId)).FirstOrDefault();
            if (group == null) return NotFound("Group not found.");
            else
            {
                List<SSItem> items = new List<SSItem>();
                for(int i = begin, j = 0; i < finish; i++)
                {
                    items.Insert(j, _context.Items.ToArray().GetValue(i) as SSItem);
                    j += 1;
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
        // /api/items/approvedinrange?groupId={group id}&start={start (int)}&end={end (int)}
        // GET an array of approved Items in a given Group, from index start (inclusive) to end (exclusive)
        [HttpGet]
        public IActionResult ApprovedInRange([FromQuery]string groupId, [FromQuery]int start, [FromQuery]int end)
        {
            int finish = (end >= 0 && end <= _context.Items.Count()) ? end : _context.Items.Count();    // end > 0 and less than equal to the array size, default to 0.
            int begin = (start >= 0 && start <= finish) ? start : 0;                                    // make sure start is greater than 0 and lessequal to end. Default to 0

            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(groupId)).FirstOrDefault();
            if (group == null) return NotFound("Group not found.");
            else
            {
                IEnumerable<SSItem> approvedItems = _context.Items.Where(e => e.Approved);
                List<SSItem> items = new List<SSItem>();
                for (int i = begin, j = 0; i < finish; i++)
                {
                    items.Insert(j, approvedItems.ToArray().GetValue(i) as SSItem);
                    j += 1;
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

        // /api/items/buy?id={item id}&userId={id of customer}&password={password of customer}
        // POST buy an item.
        [HttpPost]
        public IActionResult BuyItem([FromQuery]string id, [FromQuery]string userId, [FromQuery]string password)
        {
            // check if item or user is null
            SSItem itemToBuy = _context.Items.Where(e => e.SSItemId.Equals(id)).FirstOrDefault();
            if (itemToBuy == null) return NotFound("Item with ID " + id + " not found.");

            if (!itemToBuy.Approved) return BadRequest("Item not available for purchase.");

            // get the Group Admin
            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(itemToBuy.GroupId)).FirstOrDefault();
            if (group == null) return NotFound("Could not load the group this item is a member of.");
            SSUser admin = _context.Users.Where(e => e.SSUserId.Equals(group.AdminId)).FirstOrDefault();
            if (admin == null) return NotFound("The Admin of the group this item is a member of could not be found.");

            SSUser customer = _context.Users.Where(e => e.SSUserId.Equals(userId)).FirstOrDefault();
            if (customer == null) return NotFound("User with ID " + userId + " not found.");

            // authenticate
            if (!customer.Password.Equals(password)) return Unauthorized();

            // send confirmation email.
            var task = SendMail(customer.Email, "Purchase Confirmation", "Confirming your purchase of " + itemToBuy.Name + " with ID " + itemToBuy.SSItemId + ".");

            // remove the Item
            Delete(itemToBuy.SSItemId, admin.Password);

            return new ObjectResult(itemToBuy);
        }

        // /api/items/update?id={item id}&adminPassword={group admin password}
        // PUT update an Item
        [HttpPut]
        public IActionResult Update([FromQuery]string id, [FromBody]UpdateItemRequest itemRequest, [FromQuery]string adminPassword)
        {
            SSItem itemToUpdate = _context.Items.Where(e => e.SSItemId.Equals(id)).FirstOrDefault();
            if (itemToUpdate == null) return NotFound("Item not found.");

            // check if password is correct
            SSGroup group = _context.Groups.Where(e => e.SSGroupId.Equals(itemToUpdate.GroupId)).FirstOrDefault();
            if (group == null) return NotFound("Group not found.");

            SSUser admin = _context.Users.Where(e => e.SSUserId.Equals(group.AdminId)).FirstOrDefault();
            if (admin == null) return NotFound("Admin of Item's Group not found.");
            
            if (!adminPassword.Equals(admin.Password)) return Unauthorized();

            // check if fields are filled out.
            bool valid = itemRequest.NewName.Count() > 0 && itemRequest.NewPrice.Count() > 0 && itemRequest.NewCondition.Count() > 0 && itemRequest.NewDescription.Count() > 0 && itemRequest.NewThumbnail.Count() > 0;
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
            SSUser groupAdmin = _context.Users.Where(e => e.SSUserId == itemGroup.AdminId).FirstOrDefault();
            // check authentication (owner password or poster's password.
            if (owner.Password != password && groupAdmin.Password != password) return Unauthorized();
        
            // remove bookmarks.
            foreach(var bookmark in _context.Bookmarks.Where(e => e.ItemId.Equals(itemToDelete.SSItemId)).ToArray()) 
            {
                _context.Remove(bookmark);
            }

            // remove chat
            foreach(var message in _context.Messages.Where(e => e.ItemId.Equals(itemToDelete.SSItemId)).ToArray())
            {
                _context.Remove(message);
            }

            // delete the item and return the object that was deleted.
            _context.Remove(itemToDelete);
            _context.SaveChanges();
            return new ObjectResult(itemToDelete);
        }


        /*
         * Helper methods
         */
         static async Task SendMail(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            
            emailMessage.From.Add(new MailboxAddress("ShowAndSell", "showandsellmail@gmail.com"));
            Debug.WriteLine("EMAIL ADDRESS: " + email);
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using(var client = new SmtpClient())
            {
                /*
                // Get Google access token
                var certificate = new X509Certificate2(@"C:\Users\brayd\Documents\showandsell\ShowAndSell-d3364b669a22.p12", "Brayden14", X509KeyStorageFlags.Exportable);
                var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer("115075227415-compute@developer.gserviceaccount.com")
                {
                    Scopes = new[] { "https://mail.google.com"},
                    User = "showandsellmail@gmail.com"
                }.FromCertificate(certificate));

                bool result = await credential.RequestAccessTokenAsync(new CancellationToken());

                //client.LocalDomain = "api.sendgrid.com";
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                Debug.WriteLine("removed OAuth2");
                await client.ConnectAsync("smtp.google.com", 587, MailKit.Security.SecureSocketOptions.StartTls).ConfigureAwait(false);
                Debug.WriteLine("connected");
                await client.AuthenticateAsync("showandsellmail@gmail.com", "Brayden14").ConfigureAwait(false);
                Debug.WriteLine("authenticated");
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                Debug.WriteLine("send mail");
                await client.DisconnectAsync(true).ConfigureAwait(false);
                Debug.WriteLine("Disconnected");
                */

                await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect).ConfigureAwait(false);
                await client.AuthenticateAsync("showandsellmail@gmail.com", "Brayden14").ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}