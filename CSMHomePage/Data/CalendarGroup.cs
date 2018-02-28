namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("intra.CalendarGroups")]
    public partial class CalendarGroup
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(25)]
        public string UserID { get; set; }

        [Key]
        [Column("CalendarGroup", Order = 1)]
        [StringLength(20)]
        public string CalendarGroup1 { get; set; }

        [Key]
        [Column(Order = 2)]
        public bool IsPrimary { get; set; }
    }
}
