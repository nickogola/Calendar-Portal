using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMHomePage.Data
{
    public class cmUser
    {
        public string UserID { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public string UserRole { get; set; }

        public bool UserIsActive { get; set; }

        public string UserTitle { get; set; }

        public string UserEmailAddress { get; set; }

        public string UserColor { get; set; }
        public string DefaultCalendar { get; set; }

        public string CalendarName { get; set; }
    }
}