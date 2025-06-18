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

          //


        }
    }
    }
