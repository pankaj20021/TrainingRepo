using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolProject.DTOs;


using SchoolProject.Model;
using System.Collections.Generic;

namespace SchoolProject.Connection
{
    public class AppDbContext : IdentityDbContext<UserApp>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StudentModel> Students { get; set; }
        public DbSet<StudentAddress> StudentAddresss { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().HasData(
                new Country
                {
                    Id = 1,
                    Name = "Afghanistan",
                    Code = "AF"
                },
                new Country
                {
                    Id = 2,
                    Name = "Åland Islands",
                    Code = "AX"
                },
                new Country
                {
                    Id = 3,
                    Name = "Albania",
                    Code = "AL"
                },
                new Country
                {
                    Id = 4,
                    Name = "Algeria",
                    Code = "DZ"
                },
                new Country { Id = 5, Name = "American Samoa", Code = "AS" },
                new Country { Id = 6, Name = "AndorrA", Code = "AD" },
                new Country { Id = 7, Name = "Angola", Code = "AO" },
                new Country { Id = 8, Name = "Anguilla", Code = "AI" },
                new Country { Id = 9, Name = "Antarctica", Code = "AQ" }
            );


        }
    }
    }
