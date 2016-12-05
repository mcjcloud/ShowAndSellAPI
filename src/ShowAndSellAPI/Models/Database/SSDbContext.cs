using System;
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
        public DbSet<SSBookmark> Bookmarks { get; set; }

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
        public IActionResult DeleteGroup(string id, string password)
        {
            SSGroup groupToDelete = Groups.Where(e => e.SSGroupId == id).FirstOrDefault();
            SSUser admin = Users.Where(e => e.SSUserId == groupToDelete.Admin).FirstOrDefault();

            if (groupToDelete == null || admin == null) return new NotFoundResult();
            if (admin.Password == null) return new StatusCodeResult(500);

            // if not authorized, return 403
            if (admin.Password != password) return new StatusCodeResult(403);

            // check for items to delete.
            IList<SSItem> items = Items.Where(e => e.GroupId == groupToDelete.SSGroupId).ToList();
            foreach (SSItem item in items)
            {
                // TODO: delete the item.
                string passw = Users.Where(e => e.SSUserId == groupToDelete.Admin).FirstOrDefault().Password;
                DeleteItem(item.SSItemId, passw);   // the long complicated part gets the owner of the item, and then gets the password on the user.
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
                return new StatusCodeResult(449);
            }
            // check if username or email already exists
            foreach (var _user in Users.ToArray())
            {
                if (_user.Username == user.Username || _user.Email == user.Email)
                {
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
        public IActionResult DeleteUser(string id, string password)
        {
            SSUser userToDelete = Users.Where(e => e.SSUserId == id).FirstOrDefault();
            if (userToDelete == null) return new NotFoundResult();
            if (userToDelete.Password != password) return new StatusCodeResult(403);

            // check and delete a group that the user is admin of.
            SSGroup groupToDelete = Groups.Where(e => e.Admin == userToDelete.SSUserId).FirstOrDefault();
            if (groupToDelete != null)
            {
                // request group delete.
                DeleteGroup(groupToDelete.SSGroupId, userToDelete.Password);
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
            return Items.Where(e => e.GroupId == groupId).ToArray();
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
        public IActionResult AddItem(SSItem item)
        {
            // check that the item is valid.
            if (item == null) return new BadRequestResult();
            if (item.Name == null || item.Name == "") return new StatusCodeResult(449);
            // check group id
            SSGroup group = Groups.Where(e => e.SSGroupId == item.GroupId).FirstOrDefault();
            if (group == null) return new StatusCodeResult(449);
            // check owner id
            SSUser owner = Users.Where(e => e.SSUserId == item.OwnerId).FirstOrDefault();
            if (owner == null) return new StatusCodeResult(449);

            // check if other data is null
            var valid = item.Name != null && item.Condition != null && item.Description != null && item.Thumbnail != null && item.GroupId != null && item.OwnerId != null;
            if (!valid) return new StatusCodeResult(449);

            // finalize the item and add it to the database.
            item.SSItemId = Guid.NewGuid().ToString();
            Items.Add(item);
            SaveChanges();

            // return the item as a JSON
            return new ObjectResult(item);

        }

        // update an item
        public IActionResult UpdateItem(string id, UpdateItemRequest itemRequest)
        {
            SSItem itemToUpdate = Items.Where(e => e.SSItemId == id).FirstOrDefault();

            // check if fields are filled out.
            bool valid = itemRequest.NewName != null && itemRequest.NewPrice != null && itemRequest.NewCondition != null && itemRequest.NewDescription != null && itemRequest.NewThumbnail != null;
            if (!valid) return new StatusCodeResult(449);

            // check if password is correct
            SSGroup group = Groups.Where(e => e.SSGroupId == itemToUpdate.GroupId).FirstOrDefault();
            SSUser admin = Users.Where(e => e.SSUserId == group.Admin).FirstOrDefault();
            SSUser owner = Users.Where(e => e.SSUserId == itemToUpdate.OwnerId).FirstOrDefault();
            if (owner.Password != itemRequest.OwnerPassword && admin.Password != itemRequest.OwnerPassword) return new UnauthorizedResult();

            // set item properties
            itemToUpdate.Name = itemRequest.NewName;
            itemToUpdate.Price = itemRequest.NewPrice;
            itemToUpdate.Condition = itemRequest.NewCondition;
            itemToUpdate.Description = itemRequest.NewDescription;
            itemToUpdate.Thumbnail = itemRequest.NewThumbnail;

            // update and save changes
            Update(itemToUpdate);
            SaveChanges();
            // return the updated object.
            return new ObjectResult(itemToUpdate);
        }

        // delete an item.
        public IActionResult DeleteItem(string id, string password)
        {
            SSItem itemToDelete = Items.Where(e => e.SSItemId == id).FirstOrDefault();
            SSGroup itemGroup = Groups.Where(e => e.SSGroupId == itemToDelete.GroupId).FirstOrDefault();

            SSUser owner = Users.Where(e => e.SSUserId == itemToDelete.OwnerId).FirstOrDefault();
            SSUser groupAdmin = Users.Where(e => e.SSUserId == itemGroup.Admin).FirstOrDefault();
            // check authentication (owner password or poster's password.
            if (owner.Password != password && groupAdmin.Password != password) return new UnauthorizedResult();

            // delete the item and return the object that was deleted.
            Remove(itemToDelete);
            SaveChanges();
            return new ObjectResult(itemToDelete);
        }

        /*
         *  CRUD METHODS FOR BOOKMARKS
         */
        
        // get all bookmarks for a user with password
        public IActionResult GetBookmarks(string userId, string password)
        {
            // get the requested user
            SSUser user = Users.Where(e => e.SSUserId == userId && e.Password == password).FirstOrDefault();
            if(user == null)
            {
                // return unauthenticated
                return new StatusCodeResult(401);
            }

            // get the items for the given bookmarks
            SSBookmark[] bookmarks = Bookmarks.Where(e => e.userId == user.SSUserId).ToArray();

            // return the bookmarks if no error
            return new ObjectResult(bookmarks);
        }
        public IActionResult GetBookmarkedItems(string userId, string password)
        {
            // get a list of all bookmarks
            IEnumerable<SSBookmark> bookmarks = GetBookmarks(userId, password) as IEnumerable<SSBookmark>;

            // get a list of item ids
            string[] itemIds = new string[bookmarks.Count()];
            foreach(var bookmark in bookmarks)
            {
                itemIds.Append(bookmark.itemId);
            }

            // get a list of items based on ItemIds
            SSItem[] items = new SSItem[bookmarks.Count()];
            foreach (var id in itemIds)
            {
                items.Append(Items.Where(e => e.SSItemId == id).FirstOrDefault());
            }

            // create a list of GetBookmarkResponse with data
            GetBookmarkResponse[] responses = new GetBookmarkResponse[items.Count()];
            foreach(var item in items)
            {
                responses.Append(new GetBookmarkResponse { BookmarkId = bookmarks.Where(e => e.itemId == item.SSItemId).FirstOrDefault().SSBookmarkId, Item = item });
            }

            return new ObjectResult(responses);
        }

        // create a bookmark
        public IActionResult CreateBookmark(string userId, SSItem bookmarkedItem)
        {
            SSBookmark bookmarkToAdd = new SSBookmark
            {
                SSBookmarkId = Guid.NewGuid().ToString(),
                itemId = bookmarkedItem.SSItemId,
                userId = userId
            };

            Bookmarks.Add(bookmarkToAdd);
            SaveChanges();

            // return the bookmarked item.
            return new ObjectResult(bookmarkToAdd);
        }

        // delete bookmark
        public IActionResult DeleteBookmark(string bookmarkId)
        {
            SSBookmark bookmarkToDelete = Bookmarks.Where(e => e.SSBookmarkId == bookmarkId).FirstOrDefault();
            if(bookmarkToDelete == null)
            {
                return new NotFoundResult();
            }

            // remove the bookmark.
            Bookmarks.Remove(bookmarkToDelete);
            SaveChanges();

            // return the deleted bookmark.
            return new ObjectResult(bookmarkToDelete);
        }
    }
}
