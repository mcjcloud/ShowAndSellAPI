using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShowAndSellAPI.Models
{
    public class SSGroup
    {
        // main properties
        [Key]
        public string SSGroupId { get; set; }

        public string Name { get; set; }

        // data to be accessed by GUID
        public string AdminId { get; set; }
        public string Location { get; set; }
        public string LocationDetail { get; set; }

        // extra properties
        public DateTime DateCreated { get; set; }
        public int ItemsSold { get; set; }
    }
}
