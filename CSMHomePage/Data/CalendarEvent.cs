using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMHomePage.Data
{
    public class CalendarEvent
    {
        public int userCalendarId { get; set; }

        public string UserID { get; set; }

        public DateTime EventStartDate { get; set; }

        public DateTime EventEndDate { get; set; }

        public string EventDescription { get; set; }
        public string title { get; set; }
        public string tooltip { get; set; }
      
        public string color { get; set; }

        public bool allDay { get; set; }

        public DateTime start { get; set; }

        public DateTime end { get; set; }
    }
}