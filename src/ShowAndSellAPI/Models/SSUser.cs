using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShowAndSellAPI.Models
{
    public class SSUser
    {
        // Properties
        public string SSUserId { get; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        // Constructor - with username and password.
        public SSUser(string username, string password)
        {
            this.SSUserId = Guid.NewGuid().ToString();                              // create a Guid for the object.

            this.Username = username;
            this.Password = password;
        }
    }
}
