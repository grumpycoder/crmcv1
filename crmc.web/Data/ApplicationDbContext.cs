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
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Censor> Censors { get; set; }
        public DbSet<AppConfig> AppSettings { get; set; }
        //public DbSet<WallConfiguration> WallConfigurations { get; set; }

        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<ConfigurationColor> ConfigurationColors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<Configuration>().ToTable("Configuration");
            modelBuilder.Entity<ConfigurationColor>().ToTable("ConfigurationColors");

            base.OnModelCreating(modelBuilder);
        }
    }

}