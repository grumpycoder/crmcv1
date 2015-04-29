using System;
using System.Data.Entity;
using crmc.web.Models;

namespace crmc.web.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Censor> Censors { get; set; }
        public DbSet<AppConfig> AppSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
            modelBuilder.Entity<Person>().ToTable("Persons");
            base.OnModelCreating(modelBuilder);
        }
    }

}