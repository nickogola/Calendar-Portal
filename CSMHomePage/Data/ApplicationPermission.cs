namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("sec.ApplicationPermissions")]
    public partial class ApplicationPermission
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ApplicationPermission()
        {
            UserPermissions = new HashSet<UserPermission>();
        }

        [Key]
        public short PermissionID { get; set; }

        [Required]
        [StringLength(40)]
        public string PermissionName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
    }
}
