using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShowAndSellAPI.Models
{
    public class SSGroup
    {
        // main properties
        public string SSGroupId { get; }
        public SSUser Admin { get; set; }
        public List<SSUser> Moderators { get; set; }
        public List<SSItem> Items { get; set; }

        // extra properties
        public DateTime DateCreated { get; set; }
        public int ItemsSold { get; set; }

        // Constructor - must have an admin to create a group, so it goes in the constructor.
        public SSGroup(SSUser admin)
        {
            this.SSGroupId = Guid.NewGuid().ToString();                                            // Create a new Guid to uniquely identify this object.
            this.Admin = admin;
            DateCreated = DateTime.Now;                                                     // DateCreated is whenever the constructor is called.
        }
    }
}
