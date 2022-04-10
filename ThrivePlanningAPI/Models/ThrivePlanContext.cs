using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using ThrivePlanningAPI.Models;
using ThrivePlanningAPI.Models.Entities;

namespace ThrivePlanningAPI.Models
{
    public partial class ThrivePlanContext : DbContext
    {
        public ThrivePlanContext(DbContextOptions<ThrivePlanContext> options)
            : base(options) { }

        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<User> Users { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        #if DEBUG
        //        optionsBuilder.UseMySql("server=thriveplanningdb;port=3306;database=ThrivePlan;user=root;password=Password1;Connection Timeout=120;CharSet=utf8");
        //        #elif RELEASE
        //        optionsBuilder.UseMySql("");
        //        #else
        //        optionsBuilder.UseMySql("server=thriveplanningdb;port=3306;database=ThrivePlan;user=root;password=Password1;Connection Timeout=120;CharSet=utf8");
        //        #endif
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.CompanyAdminFirstName)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CompanyAdminLastName)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.CompanyName)
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnType("varchar(15)");

                entity.Property(e => e.TaxId)
                    .HasColumnType("varchar(9)");

                entity.Property(e => e.Industry)
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.IsConfirmed)
                    .HasColumnType("bit");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.LastName)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Phone)
                    .HasColumnType("varchar(15)");

                entity.Property(e => e.Username)
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.Type)
                    .HasColumnType("int");

                entity.Property(e => e.CompanyId)
                    .HasColumnType("varchar(36)");

                // entity.HasOne(e => e.Company)
                //     .WithMany()
                //     .HasForeignKey(e => e.CompanyId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}