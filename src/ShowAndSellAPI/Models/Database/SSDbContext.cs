using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace ShowAndSellAPI.Models.Database
{
    public class SSDbContext : DbContext
    {
        // Properties


        // Constructor
        public SSDbContext(DbContextOptions<SSDbContext> options) : base(options) { }

    }
}
