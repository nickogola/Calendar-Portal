using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMHomePage.Data
{
    public class csmLink
    {
        public short LinkID { get; set; }

        public string LinkType { get; set; }

        public byte LinkSortOrder { get; set; }
       
        public string LinkDescription { get; set; }

        public string LinkURL { get; set; }
        public string Target { get; set; }
    }
}