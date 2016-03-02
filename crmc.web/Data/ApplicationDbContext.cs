using System;
using System.Data.Entity;
using System.Linq;
using crmc.domain;
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

        public override int SaveChanges()
        {
            
            var newEntityList = ChangeTracker.Entries()
                                    .Where(x => x.Entity is Person &&
                                    (x.State == EntityState.Added));

            foreach (var entity in newEntityList)
            {
                ((Person)entity.Entity).SortOrder = Guid.NewGuid();
            }


            return base.SaveChanges();
        }
    }

}