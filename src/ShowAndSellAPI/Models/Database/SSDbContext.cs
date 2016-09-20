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
        public DbSet<SSGroup> Groups { get; set; }
        public DbSet<SSUser> Users { get; set; }
        public DbSet<SSItem> Items { get; set; }

        // Constructor
        public SSDbContext(DbContextOptions<SSDbContext> options) : base(options) { }


        /*
         * CRUD METHODS
         */
        public void AddGroup(SSGroup group)
        {
            Groups.Add(group);
            SaveChanges();
        }

        public SSGroup GetGroupWithName(string name)
        {
            return Groups.Where(e => e.Name == name).FirstOrDefault();
        }
    }
}
