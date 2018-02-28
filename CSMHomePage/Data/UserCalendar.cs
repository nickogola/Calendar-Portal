namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("intra.UserCalendar")]
    public partial class UserCalendar
    {
        public int UserCalendarID { get; set; }

        [Required]
        [StringLength(25)]
        public string UserID { get; set; }

        [Column(TypeName = "date")]
        public DateTime EventStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime EventEndDate { get; set; }

        [Required]
        [StringLength(100)]
        public string EventDescription { get; set; }
    }
}
