using CSMHomePage.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMHomePage.Models
{
    public class LinksVM
    {
        public List<csmLink> AllLinks { get; set; }
        public List<csmLink> UserLinks { get; set; }// these custom links appear on left side of the landing [age
    }
}