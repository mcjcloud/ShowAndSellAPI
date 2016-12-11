using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowAndSellAPI.Models.Http
{
    public class AddBookmarkRequest
    {
        public string UserId { get; set; }
        public string ItemId { get; set; }
    }
}
