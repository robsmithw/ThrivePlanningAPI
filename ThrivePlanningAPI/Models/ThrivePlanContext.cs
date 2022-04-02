using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using ThrivePlanningAPI.Models;

namespace ThrivePlanningAPI.Models
{
    public partial class ThrivePlanContext : DbContext
    {
        public ThrivePlanContext(DbContextOptions<ThrivePlanContext> options)
            : base(options) { }

        public virtual DbSet<Company> Company { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #if DEBUG
                optionsBuilder.UseMySql("server=thriveplanningdb;port=3306;database=ThrivePlan;user=root;password=Password1;Connection Timeout=120;CharSet=utf8");
                #elif RELEASE
                optionsBuilder.UseMySql("");
                #else
                optionsBuilder.UseMySql("server=thriveplanningdb;port=3306;database=ThrivePlan;user=root;password=Password1;Connection Timeout=120;CharSet=utf8");
                #endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasColumnType("varchar(256)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}