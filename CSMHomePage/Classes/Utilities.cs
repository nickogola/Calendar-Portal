using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMHomePage.Classes
{
    public static class Utilities
    {
        public static int AsInteger(this string val)
        {
            return Convert.ToInt32(val);
        }

        public static decimal AsDecimal(this object val)
        {
            return Convert.ToDecimal(val);
        }

        public static DateTime AsDateTime(this object val)
        {
            return Convert.ToDateTime(val);
        }
        public static string GetUser()
        {
            return HttpContext.Current.User.Identity.Name.Replace("WRBTS\\", "");
        }

        public static string GetEventColor(string evt, string defaultColor)
        {
            switch (evt.ToUpper().Trim())
            {
                case "PTO":
                    return "#00c0ef";
                case "TIME OFF":
                    return "#00c0ef";
                case "1/2 PTO":
                    return "#605ca8";
                case "SICK":
                    return "#dd4b39";
                case "REMOTE":
                    return "#00a65a";
                case "1/2 SICK":
                    return "#f012be";
                case "SUMMER HOURS":
                    return "#ff851b";
                default:
                    return defaultColor;
            }
        }
        public static bool DisplayClearCacheLink()
        {
            switch (GetUser())
            {
                case "koloughlin":
                    return true;
                case "nogola":
                    return true;
                case "MBueno":
                    return true;
                default:
                    return false;
            }
        }
    }
}