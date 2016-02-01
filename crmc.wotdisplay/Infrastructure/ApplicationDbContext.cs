using System;
using System.Data.Entity;
using crmc.domain;

namespace crmc.wotdisplay.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public DbSet<Person> Persons { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<Configuration>().ToTable("Configuration");

            base.OnModelCreating(modelBuilder);
        }
    }
}
