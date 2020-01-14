using System;
using Digitalist_Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Digitalist_Data
{
    public class ListContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ShoppingList> ShoppingList { get; set; }
        public DbSet<ListItem> ListItem { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("server=localhost;Database=digitalist;Uid=root;Pwd=root;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        }
    }
}
