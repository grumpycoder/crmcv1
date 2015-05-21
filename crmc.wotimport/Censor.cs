namespace WotImport
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Censor
    {
        public int Id { get; set; }

        public string Word { get; set; }
    }
}
