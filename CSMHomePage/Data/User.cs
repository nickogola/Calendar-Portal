namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("sec.Users")]
    public partial class User
    {
        [StringLength(25)]
        public string UserID { get; set; }

        [Required]
        [StringLength(20)]
        public string UserFirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string UserLastName { get; set; }

        [Required]
        [StringLength(25)]
        public string UserRole { get; set; }

        public bool UserIsActive { get; set; }

        [Required]
        [StringLength(75)]
        public string UserTitle { get; set; }

        [Required]
        [StringLength(100)]
        public string UserEmailAddress { get; set; }

        public DateTime dtInserted { get; set; }

        [Required]
        [StringLength(25)]
        public string InsertedBy { get; set; }

        public DateTime? dtUpdated { get; set; }

        [StringLength(25)]
        public string UpdatedBy { get; set; }

        [StringLength(10)]
        public string UserColor { get; set; }
    }
}
