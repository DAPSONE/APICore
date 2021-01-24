using System.Collections.Generic;
using JWTCore.Authentication.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTCore.Authentication.Helpers
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Modules> Modules { get; set; }

        public DbSet<UsersModules> UsersModules { get; set; }

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsersModules>()
                .HasKey(x => new { x.IdUser, x.IdModule });
            modelBuilder.Entity<UsersModules>()
                .HasOne(x => x.Module)
                .WithMany(x => x.UsersModules)
                .HasForeignKey(x => x.IdModule);
            modelBuilder.Entity<UsersModules>()
                .HasOne(x => x.User)
                .WithMany(x => x.UsersModules)
                .HasForeignKey(x => x.IdUser);
        }
    }
}