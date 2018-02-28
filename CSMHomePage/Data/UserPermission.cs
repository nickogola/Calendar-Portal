namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("sec.UserPermissions")]
    public partial class UserPermission
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(25)]
        public string UserID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PermissionID { get; set; }

        public virtual ApplicationPermission ApplicationPermission { get; set; }
    }
}
