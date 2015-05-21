namespace WotImport
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Persons")]
    public partial class Person
    {
        public int Id { get; set; }

        public int? AccountId { get; set; }

        [StringLength(50)]
        public string Firstname { get; set; }

        [StringLength(50)]
        public string Lastname { get; set; }

        [StringLength(75)]
        public string EmailAddress { get; set; }

        [StringLength(10)]
        public string Zipcode { get; set; }

        public bool? IsDonor { get; set; }

        public bool? IsPriority { get; set; }

        public decimal? FuzzyMatchValue { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; }
    }
}
