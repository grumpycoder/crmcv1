namespace WotImport
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("name=ApplicationDbContext")
        {
        }

        public virtual DbSet<Censor> Censors { get; set; }
        public virtual DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Censor>()
                .Property(e => e.Word)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.Firstname)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.Lastname)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.EmailAddress)
                .IsUnicode(false);

            modelBuilder.Entity<Person>()
                .Property(e => e.Zipcode)
                .IsUnicode(false);
        }
    }
}
