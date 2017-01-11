using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowAndSellAPI.Models.Http
{
    public class UpdateGroupRequest
    {
        public string NewName { get; set; }         // the new name for the group
        public string Password { get; set; }        // the password of the admin (to authenticate the request).
        public string NewLocation { get; set; }
        public string NewLocationDetail { get; set; }
    }
}
