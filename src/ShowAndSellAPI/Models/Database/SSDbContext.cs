﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using ShowAndSellAPI.Models.Http;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ShowAndSellAPI.Models.Database
{
    public class SSDbContext : DbContext
    {
        // Properties
        public DbSet<SSGroup> Groups { get; set; }
        public DbSet<SSUser> Users { get; set; }
        public DbSet<SSItem> Items { get; set; }

        // Constructor
        public SSDbContext(DbContextOptions<SSDbContext> options) : base(options) { }


        /*
         * CRUD METHODS FOR GROUP
         */
        // get a list of all groups in the database
        public IEnumerable<SSGroup> GetGroups()
        {
            return Groups.ToArray();
        }

        // get a group by its guid
        public SSGroup GetGroup(string id)
        {
            SSGroup group = Groups.Where(e => e.SSGroupId == id).FirstOrDefault();

            return group;
        }
        // get a group by its name
        public SSGroup GetGroupByName(string name)
        {
            return Groups.Where(e => e.Name == name).FirstOrDefault();
        }

        // add a group to the database
        public IActionResult AddGroup(AddGroupRequest groupRequest)
        {
            if (groupRequest == null) return new BadRequestResult();

            // if no admin was specified.
            // bool is if there is insufficient data entered.
            bool invalidRequest = groupRequest.Group.Admin == null || groupRequest.Group.Admin == "" || groupRequest.Group.Name == null || groupRequest.Group.Name == "" || groupRequest.Password == null;
            if (invalidRequest) return new StatusCodeResult(449);

            // check if user exists
            SSUser admin = Users.Where(e => e.SSUserId == groupRequest.Group.Admin).FirstOrDefault();
            if (admin == null) return new NotFoundResult();

            // check password
            string realPassword = admin.Password;
            if (groupRequest.Password != realPassword) return new StatusCodeResult(403);

            // check if group name is taken, or if admin already has a group.
            foreach (SSGroup group in Groups.ToArray())
            {
                if (group.Admin == admin.SSUserId || group.Name == groupRequest.Group.Name) return new BadRequestResult();
            }

            // add the group to the database.
            groupRequest.Group.SSGroupId = Guid.NewGuid().ToString();
            groupRequest.Group.DateCreated = DateTime.Now;
            Groups.Add(groupRequest.Group);
            SaveChanges();

            return new ObjectResult(groupRequest.Group);
        }

        // update a group in the database
        public IActionResult UpdateGroup(string id, UpdateGroupRequest groupRequest)
        {
            SSGroup groupToUpdate = Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            SSUser admin = Users.Where(e => e.SSUserId == groupToUpdate.Admin).FirstOrDefault();
            if (groupToUpdate == null || admin == null) return new NotFoundResult();

            // authenticate/authorize
            if (admin.Password != groupRequest.Password) return new StatusCodeResult(403);

            // check if group name is taken, or if admin already has a group.
            foreach (SSGroup group in Groups.ToArray())
            {
                if (group.Admin == admin.SSUserId || group.Name == groupRequest.NewName) return new BadRequestResult();
            }

            groupToUpdate.Name = groupRequest.NewName;
            Update(groupToUpdate);
            SaveChanges();
            return new ObjectResult(groupToUpdate);
        }

        // delete a group in the database
        public IActionResult DeleteGroup(string id, DeleteGroupRequest groupRequest)
        {
            SSGroup groupToDelete = Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            SSUser admin = Users.Where(e => e.SSUserId == groupToDelete.Admin).FirstOrDefault();

            if (groupToDelete == null || admin == null) return new NotFoundResult();
            if (admin.Password == null) return new StatusCodeResult(500);

            // if not authorized, return 403
            if (admin.Password != groupRequest.Password) return new StatusCodeResult(403);

            // check for items to delete.
            var items = Items.Where(e => e.GroupId == groupToDelete.SSGroupId);
            foreach (SSItem item in items)
            {
                // TODO: delete the item.
            }

            // remove the group
            Remove(groupToDelete);
            SaveChanges();
            return new StatusCodeResult(200);
        }


        /*
         * CRUD methods for UserController
         */
        // get a list of all users
        public IEnumerable<SSUser> GetUsers()
        {
            return Users.ToArray();
        }

        // get a user by its id and password
        public IActionResult GetUser(string id, string password)
        {
            SSUser user = Users.Where(e => e.SSUserId == id).FirstOrDefault();
            if (user == null) return new NotFoundResult();
            if (password != user.Password) return new StatusCodeResult(403);

            return new ObjectResult(user);
        }

        // get user by name
        public IEnumerable<SSUser> GetUsersByName(string name)
        {
            // get list of users who's first and last name contain the query name, then remove the password from the data before returning it.
            IEnumerable<SSUser> users = Users.Where(e => (e.FirstName + " " + e.LastName).Contains(name));
            foreach (var user in users)
            {
                user.Password = "";
            }
            return users;
        }

        // get user by username and password
        public IEnumerable<SSUser> GetUserByUsername(string username, string password)
        {
            SSUser user = Users.Where(e => e.Username == username).FirstOrDefault();
            if (user == null) return null;
            if (user.Password == password) return new SSUser[] { user };
            else return null;
        }

        // add a user to the database
        public IActionResult AddUser(SSUser user)
        {
            bool fieldsFilled = (user.Username != null && user.Password != null && user.FirstName != null && user.LastName != null && user.Email != null);
            if (!fieldsFilled) return new StatusCodeResult(449);

            // check if username or email already exists
            foreach (var _user in Users.ToArray())
            {
                if (_user.Username == user.Username || _user.Email == user.Email) return new StatusCodeResult(449);
            }

            // check if string has special characters
            var regex = new Regex("[a-zA-Z0-9]");
            if (!regex.IsMatch(user.Password)) return new StatusCodeResult(449);

                user.SSUserId = Guid.NewGuid().ToString();
            Users.Add(user);
            SaveChanges();

            return new CreatedAtRouteResult("GetUser", new { id = user.SSUserId }, user);
        }

        // update a user
        public IActionResult UpdateUser(string id, UpdateUserRequest updateRequest)
        {
            SSUser user = Users.Where(e => e.SSUserId == id).FirstOrDefault();
            if (user == null) return new NotFoundResult();

            bool fieldsFilled = updateRequest.NewUsername != null
                && updateRequest.NewPassword != null
                && updateRequest.OldPassword != null
                && updateRequest.NewFirstName != null
                && updateRequest.NewLastName != null
                && updateRequest.NewEmail != null;

            // if any of the fields aren't filled
            if (!fieldsFilled)
            {
                string errorMessage = "";
                using (StreamReader reader = new StreamReader(System.IO.File.OpenRead("Models/Http/Messages/AddGroup449.txt"))) { errorMessage += reader.ReadLine(); }

                return new StatusCodeResult(449);
            }
            // check if username or email already exists
            foreach (var _user in Users.ToArray())
            {
                if (_user.Username == user.Username || _user.Email == user.Email)
                {
                    string errorMessage = "";
                    using (StreamReader reader = new StreamReader(System.IO.File.OpenRead("Models/Http/Messages/AddGroup449.txt"))) { errorMessage += reader.ReadLine(); }
                    return new StatusCodeResult(449);
                }
            }

            // check password
            if (updateRequest.OldPassword != user.Password) return new StatusCodeResult(403);

            // update the userdata
            user.Username = updateRequest.NewUsername;
            user.Password = updateRequest.NewPassword;
            user.FirstName = updateRequest.NewFirstName;
            user.LastName = updateRequest.NewLastName;
            user.Email = updateRequest.NewEmail;

            // save changes
            Update(user);
            SaveChanges();

            return new ObjectResult(user);
        }

        // delete a user
        public IActionResult DeleteUser(string id, DeleteUserRequest deleteRequest)
        {
            SSUser userToDelete = Users.Where(e => e.SSUserId == id).FirstOrDefault();
            if (userToDelete == null) return new NotFoundResult();
            if (userToDelete.Password != deleteRequest.Password) return new StatusCodeResult(403);

            // check and delete a group that the user is admin of.
            SSGroup groupToDelete = Groups.Where(e => e.Admin == userToDelete.SSUserId).FirstOrDefault();
            if (groupToDelete != null)
            {
                // request group delete.
                DeleteGroup(groupToDelete.SSGroupId, new DeleteGroupRequest { Password = userToDelete.Password });
            }

            Remove(userToDelete);
            SaveChanges();
            return new StatusCodeResult(200);
        }

        /*
         * CRUD methods for ItemController
         */
        // GET items.
        public IEnumerable<SSItem> GetItems()
        {
            return Items.ToArray();
        }
        public IEnumerable<SSItem> GetItems(string groupId)
        {
            return Items.Where(e => e.SSItemId == groupId).ToArray();
        }
        public IEnumerable<SSItem> GetItems(string query, string groupId)
        {
            var groupItems = GetItems(groupId);
            var results = groupItems.Where(e => e.Name.Contains(query)).ToArray();

            return results;
        }
        public IEnumerable<SSItem> SearchItems(string query)
        {
            return Items.Where(e => e.Name.Contains(query)).ToArray();
        }
        public IActionResult GetItem(string itemId)
        {
            SSItem item = Items.Where(e => e.SSItemId == itemId).FirstOrDefault();
            if (item != null) return new ObjectResult(item);
            else return new NotFoundResult();
        }

        // Create an item
        public IActionResult CreateItem(SSItem item)
        {
            // check that the item is valid.
            if (item == null) return new NotFoundResult();
            if (item.Name == null || item.Name == "") return new StatusCodeResult(449);
            // check group id
            SSGroup group = Groups.Where(e => e.SSGroupId == item.GroupId).FirstOrDefault();
            if (group == null) return new StatusCodeResult(449);

            Items.Add(item);
            SaveChanges();

            return new ObjectResult(item);

        }

        // update an item
        /*
        public IActionResult UpdateItem(string id)
        {

        }
        */
    }
}
