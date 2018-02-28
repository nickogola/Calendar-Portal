namespace CSMHomePage.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("intra.Links")]
    public partial class Link
    {
        public short LinkID { get; set; }

        [Required]
        [StringLength(15)]
        public string LinkType { get; set; }

        public byte LinkSortOrder { get; set; }

        [Required]
        [StringLength(75)]
        public string LinkDescription { get; set; }

        [Required]
        [StringLength(255)]
        public string LinkURL { get; set; }
    }
}
