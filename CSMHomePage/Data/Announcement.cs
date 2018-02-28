namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("intra.Announcements")]
    public partial class Announcement
    {
        public short AnnouncementID { get; set; }

        [Column(TypeName = "date")]
        public DateTime AnnouncementStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime AnnouncementEndDate { get; set; }

        [Required]
        [StringLength(2000)]
        public string AnnouncementText { get; set; }
    }
}
