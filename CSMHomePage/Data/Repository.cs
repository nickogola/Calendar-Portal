using CSMHomePage.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CSMHomePage.Data
{
    public class Repository : IDisposable
    {
        public string connectionString { get; set; }
        List<cmUser> users = new List<cmUser>();
        public Repository()
        {
           
        }
        public IList<CalendarEvent> GetUserTeamCalendar(string userID)
        {
        
            List<CalendarEvent> userTeamCalendar = new List<CalendarEvent>();
            cmUser user = GetUser(userID);
            userTeamCalendar = GetTeamCalendar(user.DefaultCalendar).ToList();
           
            return userTeamCalendar;

        }

        public IList<CalendarEvent> SaveEvent(DateTime? startDate, DateTime? endDate, string eventDescription, string userID)
        {
            using(var db = new csmContext())
            {
                db.UserCalendars.Add(new UserCalendar
                {
                    UserID = userID,
                    EventStartDate = startDate.Value,
                    EventEndDate = endDate.Value,
                    EventDescription = eventDescription
                });
                db.SaveChanges();

                return GetUserCalendar(Utilities.GetUser());
            }
        }

        public IList<CalendarEvent> UpdateEvent(int userCalendarID, DateTime? startDate, DateTime? endDate, string eventDescription, string userID)
        {
            using (var db = new csmContext())
            {
                UserCalendar uc = db.UserCalendars.FirstOrDefault(u => u.UserCalendarID == userCalendarID);
                uc.UserID = userID;
                uc.EventStartDate = startDate.Value;
                uc.EventEndDate = endDate.Value;
                uc.EventDescription = eventDescription;
                db.SaveChanges();

                return GetUserCalendar(Utilities.GetUser());
            }
        }
        public IList<CalendarEvent> DeleteEvent(int userCalendarID)
        {
            using (var db = new csmContext())
            {
                UserCalendar uc = db.UserCalendars.FirstOrDefault(u => u.UserCalendarID == userCalendarID);
                db.UserCalendars.Remove(uc);
                db.SaveChanges();

                return GetUserCalendar(Utilities.GetUser());
            }
        }

        public List<string> GetTeams()
        {
            return new List<string>
            {
                "ACCOUNTING",
                "BA",
                "CLAIMS",
                "EXECUTIVE",
                "HR",
                "LEGAL",
                "IT",
                "SHARED",
                "UW",
                "UWOPS",
            };
        }
        public void GetUsers()
        {
            using (var db = new csmContext())
            {
                if (HttpContext.Current.Cache["Users"] != null)
                {
                    users = (List<cmUser>)HttpContext.Current.Cache["Users"];
                }
                else
                {
                    // DAL dal = new DAL(connectionString);
                    // DataSet ds = dal.GetDataNoParams("intra.usp_Get_Users");
                    var json = GetUsersJson();
                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
                    if (dt.Rows.Count != 0)
                    {

                        foreach (DataRow dr in dt.Rows)
                        {
                            users.Add(new cmUser {
                                UserID = dr.Field<string>("UserID"),
                                UserFirstName = dr.Field<string>("UserFirstName"),
                                UserLastName = dr.Field<string>("UserLastName"),
                                UserRole = dr.Field<string>("UserRole"),
                                UserEmailAddress = dr.Field<string>("UserEmailAddress"),
                                UserColor = dr.Field<string>("UserColor"),
                                DefaultCalendar = dr.Field<string>("PrimaryCalendar"),
                                CalendarName = dr.Field<string>("CalendarName")
                            }
                                );
                        }

                    }
                    //users = db.Users.ToList();
                    HttpContext.Current.Cache["Users"] = users;
                }
              //  return users;
            }
        }
        public bool EventExists(DateTime? startDate, DateTime? endDate, string eventDescription)
        {
            using (var db = new csmContext())
            {
               var events = db.UserCalendars.Where(u => u.EventStartDate == startDate && u.EventEndDate == endDate && u.EventDescription.Equals(eventDescription));


                return events.Any() ? true : false;
            }
        }
        public IList<CalendarEvent> GetTeamCalendar(string team)
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["csmContext"].ConnectionString;
            DAL dal = new DAL(connectionString);
            List<CalendarEvent> teamCalendars = new List<CalendarEvent>();


            SqlParameter[] par ={
                                new SqlParameter("Team", team)
                            };

            // DataSet ds = dal.SelectPolicyList("intra.usp_Get_Team_Calendar", par);
            //string JSONresult;
            //JSONresult = JsonConvert.SerializeObject(ds.Tables[0]);

            var json = TeamCalendarJson();
            DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
            var Teams = GetTeams();
            //  GetUsers();
           
            //Response.Write(JSONresult);
            if (dt.Rows.Count != 0)
            {
              
                foreach (DataRow dr in dt.Rows)
                {
                    if (Teams.Contains(dr[1].ToString().Trim().ToUpper()))
                    {
                        teamCalendars.Add(new CalendarEvent
                        {
                            userCalendarId = Convert.ToInt32(dr[0]),
                            UserID = dr[1].ToString(),
                            EventStartDate = (DateTime)dr[2],
                            EventEndDate = (DateTime)dr[3],
                            start = (DateTime)dr[2],
                            end = ((DateTime)dr[3]).AddHours(23).AddMinutes(59).AddSeconds(59),
                            allDay = false,
                            color = "#a1915e", //"#f39c12",
                            title = dr[4].ToString().Replace("CSM Holiday - ", ""),
                            tooltip = dr[1].ToString(),
                            EventDescription = dr[4].ToString()
                        });
                    }
                    else
                    {
                        cmUser user = GetUser(dr.Field<string>("UserID"));

                        teamCalendars.Add(new CalendarEvent
                        {
                            userCalendarId = Convert.ToInt32(dr[0]),
                            UserID = dr[1].ToString(),
                            EventStartDate = (DateTime)dr[2],
                            EventEndDate = (DateTime)dr[3],
                            start = (DateTime)dr[2],
                            end = ((DateTime)dr[3]).AddHours(23).AddMinutes(59).AddSeconds(59),
                            allDay = false,
                            color = user.UserColor,
                            title = user.CalendarName + "-" + dr[4].ToString(),
                            tooltip = user.UserFirstName + " " + user.UserLastName + "-" + dr[4].ToString(),
                            EventDescription = dr[4].ToString()
                        });
                    }
                }

            }
            return teamCalendars;

        }

        public string TeamCalendarJson()
        {
            var json = "[{\"UserCalendarID\":2,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-02-20T00:00:00\",\"EventEndDate\":\"2017-02-20T00:00:00\",\"EventDescription\":\"CSM Holiday - President's Day\"},{\"UserCalendarID\":53,\"UserID\":\"IT\",\"EventStartDate\":\"2017-03-28T00:00:00\",\"EventEndDate\":\"2017-03-28T00:00:00\",\"EventDescription\":\"Michele's Birthday\"},{\"UserCalendarID\":55,\"UserID\":\"IT\",\"EventStartDate\":\"2017-04-25T00:00:00\",\"EventEndDate\":\"2017-04-25T00:00:00\",\"EventDescription\":\"Caitlin's Birthday\"},{\"UserCalendarID\":3,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-05-29T00:00:00\",\"EventEndDate\":\"2017-05-29T00:00:00\",\"EventDescription\":\"CSM Holiday - Memorial Day\"},{\"UserCalendarID\":4,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-07-04T00:00:00\",\"EventEndDate\":\"2017-07-04T00:00:00\",\"EventDescription\":\"CSM Holiday - Independence Day\"},{\"UserCalendarID\":56,\"UserID\":\"IT\",\"EventStartDate\":\"2017-07-17T00:00:00\",\"EventEndDate\":\"2017-07-17T00:00:00\",\"EventDescription\":\"Keith's Birthday\"},{\"UserCalendarID\":14,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-07-27T00:00:00\",\"EventEndDate\":\"2017-07-27T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":40,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-07-28T00:00:00\",\"EventEndDate\":\"2017-07-28T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":10,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-01T00:00:00\",\"EventEndDate\":\"2017-08-02T00:00:00\",\"EventDescription\":\"CIO Conference in Greenwich\"},{\"UserCalendarID\":42,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-08-02T00:00:00\",\"EventEndDate\":\"2017-08-02T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":13,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-03T00:00:00\",\"EventEndDate\":\"2017-08-03T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":11,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-07T00:00:00\",\"EventEndDate\":\"2017-08-11T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":35,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-08-07T00:00:00\",\"EventEndDate\":\"2017-08-07T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":43,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-08-10T00:00:00\",\"EventEndDate\":\"2017-08-10T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":44,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-08-11T00:00:00\",\"EventEndDate\":\"2017-08-11T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":49,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-08-11T00:00:00\",\"EventEndDate\":\"2017-08-11T00:00:00\",\"EventDescription\":\"Sick Day\"},{\"UserCalendarID\":52,\"UserID\":\"IT\",\"EventStartDate\":\"2017-08-12T00:00:00\",\"EventEndDate\":\"2017-08-12T00:00:00\",\"EventDescription\":\"Michael's Birthday\"},{\"UserCalendarID\":41,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-16T00:00:00\",\"EventEndDate\":\"2017-08-16T00:00:00\",\"EventDescription\":\"1/2 Time Off\"},{\"UserCalendarID\":15,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-18T00:00:00\",\"EventEndDate\":\"2017-08-18T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":51,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-08-21T00:00:00\",\"EventEndDate\":\"2017-08-25T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":16,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-24T00:00:00\",\"EventEndDate\":\"2017-08-24T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":58,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-28T00:00:00\",\"EventEndDate\":\"2017-08-28T00:00:00\",\"EventDescription\":\"1/2 Time Off\"},{\"UserCalendarID\":60,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-30T00:00:00\",\"EventEndDate\":\"2017-08-30T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":17,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-08-31T00:00:00\",\"EventEndDate\":\"2017-08-31T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":5,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-09-04T00:00:00\",\"EventEndDate\":\"2017-09-04T00:00:00\",\"EventDescription\":\"CSM Holiday - Labor Day\"},{\"UserCalendarID\":18,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-09-07T00:00:00\",\"EventEndDate\":\"2017-09-07T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":45,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-09-08T00:00:00\",\"EventEndDate\":\"2017-09-08T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":19,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-09-14T00:00:00\",\"EventEndDate\":\"2017-09-14T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":74,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-09-15T00:00:00\",\"EventEndDate\":\"2017-09-15T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":174,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-09-21T00:00:00\",\"EventEndDate\":\"2017-09-21T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":20,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-09-21T00:00:00\",\"EventEndDate\":\"2017-09-21T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":47,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-09-22T00:00:00\",\"EventEndDate\":\"2017-09-22T00:00:00\",\"EventDescription\":\"1/2 Time Off\"},{\"UserCalendarID\":157,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-09-22T00:00:00\",\"EventEndDate\":\"2017-09-22T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":21,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-09-28T00:00:00\",\"EventEndDate\":\"2017-09-28T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":73,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-09-29T00:00:00\",\"EventEndDate\":\"2017-09-29T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":22,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-10-05T00:00:00\",\"EventEndDate\":\"2017-10-05T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":37,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-10-09T00:00:00\",\"EventEndDate\":\"2017-10-09T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":154,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-10-09T00:00:00\",\"EventEndDate\":\"2017-10-09T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":93,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-10-10T00:00:00\",\"EventEndDate\":\"2017-10-13T00:00:00\",\"EventDescription\":\"BTS - Iowa Trip\"},{\"UserCalendarID\":57,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-10-10T00:00:00\",\"EventEndDate\":\"2017-10-13T00:00:00\",\"EventDescription\":\"Iowa Trip\"},{\"UserCalendarID\":38,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-10-10T00:00:00\",\"EventEndDate\":\"2017-10-13T00:00:00\",\"EventDescription\":\"BTS Trip\"},{\"UserCalendarID\":411,\"UserID\":\"MBueno\",\"EventStartDate\":\"2017-10-17T00:00:00\",\"EventEndDate\":\"2017-10-17T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":393,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-10-19T00:00:00\",\"EventEndDate\":\"2017-10-19T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":24,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-10-19T00:00:00\",\"EventEndDate\":\"2017-10-19T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":565,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-10-24T00:00:00\",\"EventEndDate\":\"2017-10-24T00:00:00\",\"EventDescription\":\"Remote (morning)\"},{\"UserCalendarID\":25,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-10-26T00:00:00\",\"EventEndDate\":\"2017-10-26T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":54,\"UserID\":\"IT\",\"EventStartDate\":\"2017-11-02T00:00:00\",\"EventEndDate\":\"2017-11-02T00:00:00\",\"EventDescription\":\"Nick's Birthday\"},{\"UserCalendarID\":632,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-11-03T00:00:00\",\"EventEndDate\":\"2017-11-03T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":633,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-11-06T00:00:00\",\"EventEndDate\":\"2017-11-06T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":158,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-06T00:00:00\",\"EventEndDate\":\"2017-11-06T00:00:00\",\"EventDescription\":\"1/2 day Time Off\"},{\"UserCalendarID\":159,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-07T00:00:00\",\"EventEndDate\":\"2017-11-07T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":161,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-09T00:00:00\",\"EventEndDate\":\"2017-11-09T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":162,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-10T00:00:00\",\"EventEndDate\":\"2017-11-10T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":707,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-11-10T00:00:00\",\"EventEndDate\":\"2017-11-10T00:00:00\",\"EventDescription\":\"1/2 Day PM\"},{\"UserCalendarID\":629,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-15T00:00:00\",\"EventEndDate\":\"2017-11-15T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":28,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-16T00:00:00\",\"EventEndDate\":\"2017-11-16T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":6,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-11-23T00:00:00\",\"EventEndDate\":\"2017-11-24T00:00:00\",\"EventDescription\":\"CSM Holiday - Thanksgiving\"},{\"UserCalendarID\":737,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-11-27T00:00:00\",\"EventEndDate\":\"2017-11-27T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":29,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-11-30T00:00:00\",\"EventEndDate\":\"2017-11-30T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":909,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-04T00:00:00\",\"EventEndDate\":\"2017-12-04T00:00:00\",\"EventDescription\":\"1/2 Day (Remote)\"},{\"UserCalendarID\":921,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-12-05T00:00:00\",\"EventEndDate\":\"2017-12-05T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":928,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-12-06T00:00:00\",\"EventEndDate\":\"2017-12-06T00:00:00\",\"EventDescription\":\"Time Off 1/2 Day PM\"},{\"UserCalendarID\":30,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-07T00:00:00\",\"EventEndDate\":\"2017-12-07T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":585,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-12-08T00:00:00\",\"EventEndDate\":\"2017-12-08T00:00:00\",\"EventDescription\":\"CSM Holiday Party 12:30pm\"},{\"UserCalendarID\":903,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-11T00:00:00\",\"EventEndDate\":\"2017-12-11T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":31,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-14T00:00:00\",\"EventEndDate\":\"2017-12-14T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":904,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-18T00:00:00\",\"EventEndDate\":\"2017-12-18T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":912,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-12-18T00:00:00\",\"EventEndDate\":\"2017-12-18T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":32,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-21T00:00:00\",\"EventEndDate\":\"2017-12-21T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":832,\"UserID\":\"mimann\",\"EventStartDate\":\"2017-12-22T00:00:00\",\"EventEndDate\":\"2017-12-22T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":7,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-12-25T00:00:00\",\"EventEndDate\":\"2017-12-25T00:00:00\",\"EventDescription\":\"CSM Holiday - Christmas Day\"},{\"UserCalendarID\":905,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-26T00:00:00\",\"EventEndDate\":\"2017-12-26T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":913,\"UserID\":\"cslattery\",\"EventStartDate\":\"2017-12-26T00:00:00\",\"EventEndDate\":\"2017-12-26T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":906,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-27T00:00:00\",\"EventEndDate\":\"2017-12-27T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":907,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-28T00:00:00\",\"EventEndDate\":\"2017-12-28T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1054,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-12-28T00:00:00\",\"EventEndDate\":\"2017-12-28T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1053,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-12-29T00:00:00\",\"EventEndDate\":\"2017-12-29T00:00:00\",\"EventDescription\":\"Time Off - PM\"},{\"UserCalendarID\":908,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2017-12-29T00:00:00\",\"EventEndDate\":\"2017-12-29T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":8,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-01-01T00:00:00\",\"EventEndDate\":\"2018-01-01T00:00:00\",\"EventDescription\":\"CSM Holiday - New Year's Day\"},{\"UserCalendarID\":1490,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-01-04T00:00:00\",\"EventEndDate\":\"2018-01-04T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1217,\"UserID\":\"cslattery\",\"EventStartDate\":\"2018-01-09T00:00:00\",\"EventEndDate\":\"2018-01-09T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1491,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-01-11T00:00:00\",\"EventEndDate\":\"2018-01-11T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":178,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-01-15T00:00:00\",\"EventEndDate\":\"2018-01-15T00:00:00\",\"EventDescription\":\"CSM Holiday - Martin Luther King Day\"},{\"UserCalendarID\":1492,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-01-18T00:00:00\",\"EventEndDate\":\"2018-01-18T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":753,\"UserID\":\"mimann\",\"EventStartDate\":\"2018-01-24T00:00:00\",\"EventEndDate\":\"2018-01-24T00:00:00\",\"EventDescription\":\"1/2 Time Off\"},{\"UserCalendarID\":752,\"UserID\":\"mimann\",\"EventStartDate\":\"2018-01-25T00:00:00\",\"EventEndDate\":\"2018-01-25T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":728,\"UserID\":\"mimann\",\"EventStartDate\":\"2018-01-26T00:00:00\",\"EventEndDate\":\"2018-01-26T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1467,\"UserID\":\"nogola\",\"EventStartDate\":\"2018-01-26T00:00:00\",\"EventEndDate\":\"2018-01-26T00:00:00\",\"EventDescription\":\"1/2 Time Off\"},{\"UserCalendarID\":1473,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-01-26T00:00:00\",\"EventEndDate\":\"2018-01-26T00:00:00\",\"EventDescription\":\"Remote (1/2 day)\"},{\"UserCalendarID\":1483,\"UserID\":\"cslattery\",\"EventStartDate\":\"2018-01-30T00:00:00\",\"EventEndDate\":\"2018-01-30T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1482,\"UserID\":\"cslattery\",\"EventStartDate\":\"2018-01-31T00:00:00\",\"EventEndDate\":\"2018-01-31T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1493,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-02-01T00:00:00\",\"EventEndDate\":\"2018-02-01T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1542,\"UserID\":\"cslattery\",\"EventStartDate\":\"2018-02-01T00:00:00\",\"EventEndDate\":\"2018-02-01T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1494,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-02-08T00:00:00\",\"EventEndDate\":\"2018-02-08T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1582,\"UserID\":\"nogola\",\"EventStartDate\":\"2018-02-09T00:00:00\",\"EventEndDate\":\"2018-02-09T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1500,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-02-14T00:00:00\",\"EventEndDate\":\"2018-02-14T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1498,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-02-15T00:00:00\",\"EventEndDate\":\"2018-02-15T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1499,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-02-16T00:00:00\",\"EventEndDate\":\"2018-02-16T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1562,\"UserID\":\"jtobia\",\"EventStartDate\":\"2018-02-16T00:00:00\",\"EventEndDate\":\"2018-02-16T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":179,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-02-19T00:00:00\",\"EventEndDate\":\"2018-02-19T00:00:00\",\"EventDescription\":\"CSM Holiday - President's Day\"},{\"UserCalendarID\":1496,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-02-22T00:00:00\",\"EventEndDate\":\"2018-02-22T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1497,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-03-01T00:00:00\",\"EventEndDate\":\"2018-03-01T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1501,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-03-08T00:00:00\",\"EventEndDate\":\"2018-03-08T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1502,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-03-15T00:00:00\",\"EventEndDate\":\"2018-03-15T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1503,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-03-22T00:00:00\",\"EventEndDate\":\"2018-03-22T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":62,\"UserID\":\"IT\",\"EventStartDate\":\"2018-03-28T00:00:00\",\"EventEndDate\":\"2018-03-28T00:00:00\",\"EventDescription\":\"Michele's Birthday\"},{\"UserCalendarID\":1504,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-03-29T00:00:00\",\"EventEndDate\":\"2018-03-29T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1505,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-04-05T00:00:00\",\"EventEndDate\":\"2018-04-05T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1506,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-04-12T00:00:00\",\"EventEndDate\":\"2018-04-12T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1507,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-04-19T00:00:00\",\"EventEndDate\":\"2018-04-19T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1596,\"UserID\":\"jtobia\",\"EventStartDate\":\"2018-04-20T00:00:00\",\"EventEndDate\":\"2018-04-20T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":64,\"UserID\":\"IT\",\"EventStartDate\":\"2018-04-25T00:00:00\",\"EventEndDate\":\"2018-04-25T00:00:00\",\"EventDescription\":\"Caitlin's Birthday\"},{\"UserCalendarID\":1508,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-04-26T00:00:00\",\"EventEndDate\":\"2018-04-26T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1509,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-05-03T00:00:00\",\"EventEndDate\":\"2018-05-03T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1510,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-05-10T00:00:00\",\"EventEndDate\":\"2018-05-10T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1511,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-05-17T00:00:00\",\"EventEndDate\":\"2018-05-17T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1512,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-05-24T00:00:00\",\"EventEndDate\":\"2018-05-24T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":180,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-05-28T00:00:00\",\"EventEndDate\":\"2018-05-28T00:00:00\",\"EventDescription\":\"CSM Holiday - Memorial Day\"},{\"UserCalendarID\":1513,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-05-31T00:00:00\",\"EventEndDate\":\"2018-05-31T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1434,\"UserID\":\"mimann\",\"EventStartDate\":\"2018-06-04T00:00:00\",\"EventEndDate\":\"2018-06-05T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1514,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-06-07T00:00:00\",\"EventEndDate\":\"2018-06-07T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1517,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-06-14T00:00:00\",\"EventEndDate\":\"2018-06-14T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1516,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-06-21T00:00:00\",\"EventEndDate\":\"2018-06-21T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":1515,\"UserID\":\"koloughlin\",\"EventStartDate\":\"2018-06-28T00:00:00\",\"EventEndDate\":\"2018-06-28T00:00:00\",\"EventDescription\":\"Remote\"},{\"UserCalendarID\":181,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-07-04T00:00:00\",\"EventEndDate\":\"2018-07-04T00:00:00\",\"EventDescription\":\"CSM Holiday - Independence Day\"},{\"UserCalendarID\":65,\"UserID\":\"IT\",\"EventStartDate\":\"2018-07-17T00:00:00\",\"EventEndDate\":\"2018-07-17T00:00:00\",\"EventDescription\":\"Keith's Birthday\"},{\"UserCalendarID\":61,\"UserID\":\"IT\",\"EventStartDate\":\"2018-08-12T00:00:00\",\"EventEndDate\":\"2018-08-12T00:00:00\",\"EventDescription\":\"Michael's Birthday\"},{\"UserCalendarID\":988,\"UserID\":\"cslattery\",\"EventStartDate\":\"2018-08-27T00:00:00\",\"EventEndDate\":\"2018-08-31T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":182,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-09-03T00:00:00\",\"EventEndDate\":\"2018-09-03T00:00:00\",\"EventDescription\":\"CSM Holiday - Labor Day\"},{\"UserCalendarID\":63,\"UserID\":\"IT\",\"EventStartDate\":\"2018-11-02T00:00:00\",\"EventEndDate\":\"2018-11-02T00:00:00\",\"EventDescription\":\"Nick's Birthday\"},{\"UserCalendarID\":183,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-11-22T00:00:00\",\"EventEndDate\":\"2018-11-23T00:00:00\",\"EventDescription\":\"CSM Holiday - Thanksgiving\"},{\"UserCalendarID\":185,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-12-25T00:00:00\",\"EventEndDate\":\"2018-12-25T00:00:00\",\"EventDescription\":\"CSM Holiday - Christmas Day\"},{\"UserCalendarID\":67,\"UserID\":\"IT\",\"EventStartDate\":\"2019-03-28T00:00:00\",\"EventEndDate\":\"2019-03-28T00:00:00\",\"EventDescription\":\"Michele's Birthday\"},{\"UserCalendarID\":69,\"UserID\":\"IT\",\"EventStartDate\":\"2019-04-25T00:00:00\",\"EventEndDate\":\"2019-04-25T00:00:00\",\"EventDescription\":\"Caitlin's Birthday\"},{\"UserCalendarID\":70,\"UserID\":\"IT\",\"EventStartDate\":\"2019-07-17T00:00:00\",\"EventEndDate\":\"2019-07-17T00:00:00\",\"EventDescription\":\"Keith's Birthday\"},{\"UserCalendarID\":66,\"UserID\":\"IT\",\"EventStartDate\":\"2019-08-12T00:00:00\",\"EventEndDate\":\"2019-08-12T00:00:00\",\"EventDescription\":\"Michael's Birthday\"},{\"UserCalendarID\":68,\"UserID\":\"IT\",\"EventStartDate\":\"2019-11-02T00:00:00\",\"EventEndDate\":\"2019-11-02T00:00:00\",\"EventDescription\":\"Nick's Birthday\"}]";
            return json;
        }
        public string TeamLinksJson()
        {
            var json = "[{\"LinkID\":53,\"LinkType\":\"Accounting\",\"LinkSortOrder\":10,\"LinkDescription\":\"Berkley Billing\",\"LinkURL\":\"http://bil.clermonthid.com/ \"},{\"LinkID\":54,\"LinkType\":\"Accounting\",\"LinkSortOrder\":20,\"LinkDescription\":\"FAB\",\"LinkURL\":\"http://csm-prd/Billing/MultiFab.jnlp\"},{\"LinkID\":55,\"LinkType\":\"Accounting\",\"LinkSortOrder\":30,\"LinkDescription\":\"FRS\",\"LinkURL\":\"http://myapps.wrberkley.com/Citrix/XenAppInternal/auth/login.aspx\"},{\"LinkID\":56,\"LinkType\":\"Accounting\",\"LinkSortOrder\":40,\"LinkDescription\":\"Hyperion\",\"LinkURL\":\"http://biprod/workspace/\"},{\"LinkID\":57,\"LinkType\":\"Accounting\",\"LinkSortOrder\":50,\"LinkDescription\":\"PeopleSoft\",\"LinkURL\":\"http://psft92-prd.wrbts.ads.wrberkley.com/psp/fs92prd/EMPLOYEE/ERP/h/?tab=DEFAULT&cmd=login&errorCode=106&languageCd=ENG\"},{\"LinkID\":58,\"LinkType\":\"Accounting\",\"LinkSortOrder\":60,\"LinkDescription\":\"Perceptive\",\"LinkURL\":\"https://perceptiveprd.wrbts.ads.wrberkley.com/#login\"},{\"LinkID\":59,\"LinkType\":\"Accounting\",\"LinkSortOrder\":70,\"LinkDescription\":\"Perceptive Navigator \",\"LinkURL\":\"https://perceptiveprd.wrbts.ads.wrberkley.com/contentexplorer/#login\"},{\"LinkID\":60,\"LinkType\":\"Actuary\",\"LinkSortOrder\":10,\"LinkDescription\":\"CARE\",\"LinkURL\":\"http://care.berkleyluxurygroup.com/\"},{\"LinkID\":61,\"LinkType\":\"Actuary\",\"LinkSortOrder\":20,\"LinkDescription\":\"Rate Monitor Actuary\",\"LinkURL\":\"http://ratemonitor.berkleyluxurygroup.com/\"},{\"LinkID\":62,\"LinkType\":\"Actuary\",\"LinkSortOrder\":30,\"LinkDescription\":\"Rate Monitor UW\",\"LinkURL\":\"http://rmu.berkleyluxurygroup.com/\"},{\"LinkID\":69,\"LinkType\":\"BA\",\"LinkSortOrder\":10,\"LinkDescription\":\"TST-Admin Panel\",\"LinkURL\":\"http://adminpanel.berkleyluxurygroup.com/\"},{\"LinkID\":70,\"LinkType\":\"BA\",\"LinkSortOrder\":20,\"LinkDescription\":\"TST-APS\",\"LinkURL\":\"http://tst-aps.clermonthid.com/aps\"},{\"LinkID\":71,\"LinkType\":\"BA\",\"LinkSortOrder\":30,\"LinkDescription\":\"TST-BCS\",\"LinkURL\":\"https://clearance-test.wrbts.ads.wrberkley.com/csm/clients\"},{\"LinkID\":72,\"LinkType\":\"BA\",\"LinkSortOrder\":40,\"LinkDescription\":\"TST-CARE\",\"LinkURL\":\"http://tst-care.berkleyluxurygroup.com/\"},{\"LinkID\":73,\"LinkType\":\"BA\",\"LinkSortOrder\":50,\"LinkDescription\":\"TST-BPMi\",\"LinkURL\":\"https://bts1-bpmt.wrbts.ads.wrberkley.com/ProcessPortal/login.jsp\"},{\"LinkID\":74,\"LinkType\":\"BA\",\"LinkSortOrder\":60,\"LinkDescription\":\"TST-ClaimTrak\",\"LinkURL\":\"https://claimtrak-tst.wrberkley.com/\"},{\"LinkID\":75,\"LinkType\":\"BA\",\"LinkSortOrder\":70,\"LinkDescription\":\"TST-Content Navigator\",\"LinkURL\":\"https://contentnav-na-tst.wrberkley.com/navigator/?desktop=CSM982d4cb6\"},{\"LinkID\":76,\"LinkType\":\"BA\",\"LinkSortOrder\":80,\"LinkDescription\":\"TST-Exception Pages\",\"LinkURL\":\"http://tst-expages.berkleyluxurygroup.com/\"},{\"LinkID\":77,\"LinkType\":\"BA\",\"LinkSortOrder\":90,\"LinkDescription\":\"TST-Forms Library\",\"LinkURL\":\"http://tst-forms.berkleyluxurygroup.com/\"},{\"LinkID\":78,\"LinkType\":\"BA\",\"LinkSortOrder\":100,\"LinkDescription\":\"TST-Filings Library\",\"LinkURL\":\"http://tst-filings.berkleyluxurygroup.com/\"},{\"LinkID\":79,\"LinkType\":\"BA\",\"LinkSortOrder\":110,\"LinkDescription\":\"TST-IT Support Ticket\",\"LinkURL\":\"http://tst-ticket.berkleyluxurygroup.com/\"},{\"LinkID\":80,\"LinkType\":\"BA\",\"LinkSortOrder\":120,\"LinkDescription\":\"TST-Policy # Generator\",\"LinkURL\":\"http://btsdetstcsmweb1:2000/\"},{\"LinkID\":81,\"LinkType\":\"BA\",\"LinkSortOrder\":130,\"LinkDescription\":\"TST-RM Actuary\",\"LinkURL\":\"http://btsdetstcsmweb1:90/\"},{\"LinkID\":82,\"LinkType\":\"BA\",\"LinkSortOrder\":140,\"LinkDescription\":\"TST-RM UW\",\"LinkURL\":\"http://tst-rmu.berkleyluxurygroup.com/\"},{\"LinkID\":83,\"LinkType\":\"BA\",\"LinkSortOrder\":150,\"LinkDescription\":\"TST-SSRS\",\"LinkURL\":\"http://btsdetstsql05/Reports/Pages/Folder.aspx?ItemPath=%2fCSM&ViewMode=Detail\"},{\"LinkID\":35,\"LinkType\":\"Calendars\",\"LinkSortOrder\":10,\"LinkDescription\":\"Accounting\",\"LinkURL\":\"\"},{\"LinkID\":36,\"LinkType\":\"Calendars\",\"LinkSortOrder\":20,\"LinkDescription\":\"Claims\",\"LinkURL\":\"\"},{\"LinkID\":37,\"LinkType\":\"Calendars\",\"LinkSortOrder\":30,\"LinkDescription\":\"Executive\",\"LinkURL\":\"\"},{\"LinkID\":38,\"LinkType\":\"Calendars\",\"LinkSortOrder\":40,\"LinkDescription\":\"HR\",\"LinkURL\":\"\"},{\"LinkID\":39,\"LinkType\":\"Calendars\",\"LinkSortOrder\":50,\"LinkDescription\":\"IT\",\"LinkURL\":\"\"},{\"LinkID\":40,\"LinkType\":\"Calendars\",\"LinkSortOrder\":60,\"LinkDescription\":\"Legal\",\"LinkURL\":\"\"},{\"LinkID\":41,\"LinkType\":\"Calendars\",\"LinkSortOrder\":70,\"LinkDescription\":\"Underwriting\",\"LinkURL\":\"\"},{\"LinkID\":42,\"LinkType\":\"Calendars\",\"LinkSortOrder\":80,\"LinkDescription\":\"UW Operations\",\"LinkURL\":\"\"},{\"LinkID\":43,\"LinkType\":\"Claims\",\"LinkSortOrder\":10,\"LinkDescription\":\"Claims & Legal Resources\",\"LinkURL\":\"http://wiki.theclm.org/\"},{\"LinkID\":44,\"LinkType\":\"Claims\",\"LinkSortOrder\":20,\"LinkDescription\":\"ClaimTrak\",\"LinkURL\":\"https://claimtrak.wrberkley.com/\"},{\"LinkID\":45,\"LinkType\":\"Claims\",\"LinkSortOrder\":30,\"LinkDescription\":\"ClaimTrak Admin\",\"LinkURL\":\"https://claimtrak.wrberkley.com/Admin\"},{\"LinkID\":46,\"LinkType\":\"Claims\",\"LinkSortOrder\":40,\"LinkDescription\":\"Content Navigator\",\"LinkURL\":\"https://contentnav-na.wrberkley.com/navigator/?desktop=CSM982d4cb6\"},{\"LinkID\":47,\"LinkType\":\"Claims\",\"LinkSortOrder\":50,\"LinkDescription\":\"FRS\",\"LinkURL\":\"http://myapps.wrberkley.com/Citrix/XenAppInternal/auth/login.aspx\"},{\"LinkID\":48,\"LinkType\":\"Claims\",\"LinkSortOrder\":60,\"LinkDescription\":\"Genesys\",\"LinkURL\":\"http://csm-prd.wrbts.ads.wrberkley.com/policystarweb/genesys/search\"},{\"LinkID\":49,\"LinkType\":\"Claims\",\"LinkSortOrder\":70,\"LinkDescription\":\"Legal-X\",\"LinkURL\":\"https://bottomline.legal-x.com/cas/login\"},{\"LinkID\":50,\"LinkType\":\"Claims\",\"LinkSortOrder\":80,\"LinkDescription\":\"LLR Tracker\",\"LinkURL\":\"http://largeloss\"},{\"LinkID\":51,\"LinkType\":\"Claims\",\"LinkSortOrder\":90,\"LinkDescription\":\"NJ Business Records Service\",\"LinkURL\":\"https://www.njportal.com/DOR/businessrecords/EntityDocs/BusinessCopies.aspx\"},{\"LinkID\":52,\"LinkType\":\"Claims\",\"LinkSortOrder\":100,\"LinkDescription\":\"NY Corp & Business Entity Search\",\"LinkURL\":\"https://www.dos.ny.gov/corps/bus_entity_search.html\"},{\"LinkID\":130,\"LinkType\":\"Compliance\",\"LinkSortOrder\":10,\"LinkDescription\":\"Content Navigator\",\"LinkURL\":\"https://contentnav-na.wrberkley.com/navigator/?desktop=CSM982d4cb6\"},{\"LinkID\":131,\"LinkType\":\"Compliance\",\"LinkSortOrder\":20,\"LinkDescription\":\"Genesys\",\"LinkURL\":\"http://csm-prd.wrbts.ads.wrberkley.com/policystarweb/genesys/search\"},{\"LinkID\":133,\"LinkType\":\"Compliance\",\"LinkSortOrder\":40,\"LinkDescription\":\"UW Exception Pages\",\"LinkURL\":\"http://expages.berkleyluxurygroup.com/\"},{\"LinkID\":134,\"LinkType\":\"Compliance\",\"LinkSortOrder\":50,\"LinkDescription\":\"UW Forms\",\"LinkURL\":\"http://forms.berkleyluxurygroup.com/\"},{\"LinkID\":135,\"LinkType\":\"Compliance\",\"LinkSortOrder\":60,\"LinkDescription\":\"UW Filings\",\"LinkURL\":\"http://filings.berkleyluxurygroup.com/\"},{\"LinkID\":1,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":10,\"LinkDescription\":\"BLG Website\",\"LinkURL\":\"https://www.berkleyluxurygroup.com/\"},{\"LinkID\":6,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":15,\"LinkDescription\":\"Shared Documents (S: Drive)\",\"LinkURL\":\"file://WRBTS/companies/CSM\"},{\"LinkID\":2,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":20,\"LinkDescription\":\"WR Berkley Website\",\"LinkURL\":\"http://www.wrberkley.com/\"},{\"LinkID\":3,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":30,\"LinkDescription\":\"WR Berkley Directory\",\"LinkURL\":\"https://btsprod.service-now.com/bts?id=directory_v2\"},{\"LinkID\":4,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":40,\"LinkDescription\":\"IT Support Ticket\",\"LinkURL\":\"http://ticket.berkleyluxurygroup.com/\"},{\"LinkID\":5,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":50,\"LinkDescription\":\"ServiceNow\",\"LinkURL\":\"https://btsprod.service-now.com/bts?id=home\"},{\"LinkID\":7,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":70,\"LinkDescription\":\"SSRS Reports\",\"LinkURL\":\"http://btsdesql54/Reports/Pages/Folder.aspx?ItemPath=%2fCSM&ViewMode=Detail\"},{\"LinkID\":8,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":80,\"LinkDescription\":\"Outlook Web Mail\",\"LinkURL\":\"https://email.wrberkley.com/owa/auth/logon.aspx?replaceCurrent=1&url=https%3a%2f%2femail.wrberkley.com%2fowa%2f\"},{\"LinkID\":9,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":90,\"LinkDescription\":\"BTS SharePoint\",\"LinkURL\":\"https://sharepoint.wrberkley.com/Intranet/Pages/Home.aspx\"},{\"LinkID\":10,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":100,\"LinkDescription\":\"Wells Fargo CC\",\"LinkURL\":\"https://wellsoffice.wellsfargo.com/portal/signon/\"},{\"LinkID\":11,\"LinkType\":\"CSM Links\",\"LinkSortOrder\":110,\"LinkDescription\":\"Weather-Rutherford\",\"LinkURL\":\"https://www.wunderground.com/cgi-bin/findweather/getForecast?query=zmw:07070.1.99999&bannertypeclick=wu_travel_traveler1\"},{\"LinkID\":63,\"LinkType\":\"Executive\",\"LinkSortOrder\":10,\"LinkDescription\":\"BLG Website\",\"LinkURL\":\"https://www.berkleyluxurygroup.com/\"},{\"LinkID\":64,\"LinkType\":\"Executive\",\"LinkSortOrder\":20,\"LinkDescription\":\"WR Berkley Website\",\"LinkURL\":\"http://www.wrberkley.com/\"},{\"LinkID\":65,\"LinkType\":\"Executive\",\"LinkSortOrder\":30,\"LinkDescription\":\"WR Berkley Directory\",\"LinkURL\":\"https://btsprod.service-now.com/bts?id=directory_v2\"},{\"LinkID\":177,\"LinkType\":\"Financial\",\"LinkSortOrder\":10,\"LinkDescription\":\"2017 Q3 Results\",\"LinkURL\":\"file://WRBTS/companies/CSM/Management Reports/2017/CSM 2017-Q3 Results.pdf\"},{\"LinkID\":162,\"LinkType\":\"HR\",\"LinkSortOrder\":10,\"LinkDescription\":\"ADP Self Service Portal\",\"LinkURL\":\"https://online.adp.com/portal/login.html\"},{\"LinkID\":163,\"LinkType\":\"HR\",\"LinkSortOrder\":20,\"LinkDescription\":\"Computershare\",\"LinkURL\":\"https://www-us.computershare.com/Employee/Login/SelectCompany.aspx#\"},{\"LinkID\":164,\"LinkType\":\"HR\",\"LinkSortOrder\":30,\"LinkDescription\":\"CIGNA Vision\",\"LinkURL\":\"https://cigna.vsp.com/signon.html\"},{\"LinkID\":165,\"LinkType\":\"HR\",\"LinkSortOrder\":40,\"LinkDescription\":\"CVS/Caremark\",\"LinkURL\":\"https://www.caremark.com/wps/portal\"},{\"LinkID\":166,\"LinkType\":\"HR\",\"LinkSortOrder\":50,\"LinkDescription\":\"Delta Dental\",\"LinkURL\":\"http://www.deltadentalnj.com/\"},{\"LinkID\":167,\"LinkType\":\"HR\",\"LinkSortOrder\":60,\"LinkDescription\":\"Employee Benefits Corporation\",\"LinkURL\":\"http://www.ebcflex.com/\"},{\"LinkID\":168,\"LinkType\":\"HR\",\"LinkSortOrder\":70,\"LinkDescription\":\"Fidelity Investments\",\"LinkURL\":\"https://nb.fidelity.com/public/nb/401k/home\"},{\"LinkID\":169,\"LinkType\":\"HR\",\"LinkSortOrder\":80,\"LinkDescription\":\"FMLA Requests\",\"LinkURL\":\"https://www.fmlasource.com/FMLAWeb/login/login.xhtml\"},{\"LinkID\":170,\"LinkType\":\"HR\",\"LinkSortOrder\":90,\"LinkDescription\":\"HealthEquity\",\"LinkURL\":\"http://www.healthequity.com/\"},{\"LinkID\":171,\"LinkType\":\"HR\",\"LinkSortOrder\":100,\"LinkDescription\":\"MyCigna\",\"LinkURL\":\"https://my.cigna.com/web/public/guest\"},{\"LinkID\":172,\"LinkType\":\"HR\",\"LinkSortOrder\":110,\"LinkDescription\":\"Unum\",\"LinkURL\":\"http://www.unum.com/\"},{\"LinkID\":173,\"LinkType\":\"HR\",\"LinkSortOrder\":120,\"LinkDescription\":\"Viverae Wellness\",\"LinkURL\":\"https://www.myviverae.com/ehms/login.seam\"},{\"LinkID\":174,\"LinkType\":\"HR\",\"LinkSortOrder\":130,\"LinkDescription\":\"Workday\",\"LinkURL\":\"https://www.myworkday.com/wrberkley/login.htmld\"},{\"LinkID\":25,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":10,\"LinkDescription\":\"HR Documents\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared\"},{\"LinkID\":26,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":20,\"LinkDescription\":\"Benefits\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Benefits\"},{\"LinkID\":27,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":30,\"LinkDescription\":\"Corporate HR Policies\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Corporate HR Policies\"},{\"LinkID\":28,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":40,\"LinkDescription\":\"Employee Appreciation\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Employee Appreciation\"},{\"LinkID\":29,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":50,\"LinkDescription\":\"Employee Handbook\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Employee Handbook\"},{\"LinkID\":30,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":60,\"LinkDescription\":\"HR Forms\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/HR Forms\"},{\"LinkID\":31,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":70,\"LinkDescription\":\"Manager's Toolbox\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Managers Toolbox\"},{\"LinkID\":32,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":80,\"LinkDescription\":\"Open Enrollment\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Open Enrollment\"},{\"LinkID\":33,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":90,\"LinkDescription\":\"Payroll\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Payroll\"},{\"LinkID\":34,\"LinkType\":\"HR Documents\",\"LinkSortOrder\":100,\"LinkDescription\":\"Performance Management\",\"LinkURL\":\"file://wrbts/companies/CSM/HR Shared/Performance Management\"},{\"LinkID\":12,\"LinkType\":\"HR Links\",\"LinkSortOrder\":10,\"LinkDescription\":\"ADP Self Service Portal\",\"LinkURL\":\"https://online.adp.com/portal/login.html\"},{\"LinkID\":13,\"LinkType\":\"HR Links\",\"LinkSortOrder\":20,\"LinkDescription\":\"Computershare\",\"LinkURL\":\"https://www-us.computershare.com/Employee/Login/SelectCompany.aspx#\"},{\"LinkID\":14,\"LinkType\":\"HR Links\",\"LinkSortOrder\":30,\"LinkDescription\":\"CIGNA Vision\",\"LinkURL\":\"https://cigna.vsp.com/signon.html\"},{\"LinkID\":15,\"LinkType\":\"HR Links\",\"LinkSortOrder\":40,\"LinkDescription\":\"CVS/Caremark\",\"LinkURL\":\"https://www.caremark.com/wps/portal\"},{\"LinkID\":16,\"LinkType\":\"HR Links\",\"LinkSortOrder\":50,\"LinkDescription\":\"Delta Dental\",\"LinkURL\":\"http://www.deltadentalnj.com/\"},{\"LinkID\":17,\"LinkType\":\"HR Links\",\"LinkSortOrder\":60,\"LinkDescription\":\"Employee Benefits Corporation\",\"LinkURL\":\"http://www.ebcflex.com/\"},{\"LinkID\":18,\"LinkType\":\"HR Links\",\"LinkSortOrder\":70,\"LinkDescription\":\"Fidelity Investments\",\"LinkURL\":\"https://nb.fidelity.com/public/nb/401k/home\"},{\"LinkID\":19,\"LinkType\":\"HR Links\",\"LinkSortOrder\":80,\"LinkDescription\":\"FMLA Requests\",\"LinkURL\":\"https://www.fmlasource.com/FMLAWeb/login/login.xhtml\"},{\"LinkID\":20,\"LinkType\":\"HR Links\",\"LinkSortOrder\":90,\"LinkDescription\":\"HealthEquity\",\"LinkURL\":\"http://www.healthequity.com/\"},{\"LinkID\":21,\"LinkType\":\"HR Links\",\"LinkSortOrder\":100,\"LinkDescription\":\"MyCigna\",\"LinkURL\":\"https://my.cigna.com/web/public/guest\"},{\"LinkID\":22,\"LinkType\":\"HR Links\",\"LinkSortOrder\":110,\"LinkDescription\":\"Unum\",\"LinkURL\":\"http://www.unum.com/\"},{\"LinkID\":23,\"LinkType\":\"HR Links\",\"LinkSortOrder\":120,\"LinkDescription\":\"Viverae Wellness\",\"LinkURL\":\"https://www.myviverae.com/ehms/login.seam\"},{\"LinkID\":24,\"LinkType\":\"HR Links\",\"LinkSortOrder\":130,\"LinkDescription\":\"Workday\",\"LinkURL\":\"https://www.myworkday.com/wrberkley/login.htmld\"},{\"LinkID\":84,\"LinkType\":\"IT\",\"LinkSortOrder\":10,\"LinkDescription\":\"TST-Admin Panel\",\"LinkURL\":\"http://adminpanel.berkleyluxurygroup.com/\"},{\"LinkID\":85,\"LinkType\":\"IT\",\"LinkSortOrder\":20,\"LinkDescription\":\"TST-APS\",\"LinkURL\":\"http://tst-aps.clermonthid.com/aps\"},{\"LinkID\":86,\"LinkType\":\"IT\",\"LinkSortOrder\":30,\"LinkDescription\":\"TST-BCS\",\"LinkURL\":\"https://clearance-test.wrbts.ads.wrberkley.com/csm/clients\"},{\"LinkID\":87,\"LinkType\":\"IT\",\"LinkSortOrder\":40,\"LinkDescription\":\"TST-CARE\",\"LinkURL\":\"http://tst-care.berkleyluxurygroup.com/\"},{\"LinkID\":88,\"LinkType\":\"IT\",\"LinkSortOrder\":50,\"LinkDescription\":\"TST-BPMi\",\"LinkURL\":\"https://bts1-bpmt.wrbts.ads.wrberkley.com/ProcessPortal/login.jsp\"},{\"LinkID\":89,\"LinkType\":\"IT\",\"LinkSortOrder\":60,\"LinkDescription\":\"TST-ClaimTrak\",\"LinkURL\":\"https://claimtrak-tst.wrberkley.com/\"},{\"LinkID\":90,\"LinkType\":\"IT\",\"LinkSortOrder\":70,\"LinkDescription\":\"TST-Content Navigator\",\"LinkURL\":\"https://contentnav-na-tst.wrberkley.com/navigator/?desktop=CSM982d4cb6\"},{\"LinkID\":91,\"LinkType\":\"IT\",\"LinkSortOrder\":80,\"LinkDescription\":\"TST-Exception Pages\",\"LinkURL\":\"http://tst-expages.berkleyluxurygroup.com/\"},{\"LinkID\":92,\"LinkType\":\"IT\",\"LinkSortOrder\":90,\"LinkDescription\":\"TST-Forms Library\",\"LinkURL\":\"http://tst-forms.berkleyluxurygroup.com/\"},{\"LinkID\":93,\"LinkType\":\"IT\",\"LinkSortOrder\":100,\"LinkDescription\":\"TST-Filings Library\",\"LinkURL\":\"http://tst-filings.berkleyluxurygroup.com/\"},{\"LinkID\":94,\"LinkType\":\"IT\",\"LinkSortOrder\":110,\"LinkDescription\":\"TST-IT Support Ticket\",\"LinkURL\":\"http://tst-ticket.berkleyluxurygroup.com/\"},{\"LinkID\":95,\"LinkType\":\"IT\",\"LinkSortOrder\":120,\"LinkDescription\":\"TST-Policy # Generator\",\"LinkURL\":\"http://btsdetstcsmweb1:2000/\"},{\"LinkID\":96,\"LinkType\":\"IT\",\"LinkSortOrder\":130,\"LinkDescription\":\"TST-RM Actuary\",\"LinkURL\":\"http://btsdetstcsmweb1:90/\"},{\"LinkID\":97,\"LinkType\":\"IT\",\"LinkSortOrder\":140,\"LinkDescription\":\"TST-RM UW\",\"LinkURL\":\"http://tst-rmu.berkleyluxurygroup.com/\"},{\"LinkID\":98,\"LinkType\":\"IT\",\"LinkSortOrder\":150,\"LinkDescription\":\"TST-SSRS\",\"LinkURL\":\"http://btsdetstsql05/Reports/Pages/Folder.aspx?ItemPath=%2fCSM&ViewMode=Detail\"},{\"LinkID\":160,\"LinkType\":\"IT Policies\",\"LinkSortOrder\":10,\"LinkDescription\":\"Record Retention Policy\",\"LinkURL\":\"file://wrbts/companies/CSM/IT Policies/Record Retention Policy and Schedule 2016.pdf\"},{\"LinkID\":161,\"LinkType\":\"IT Policies\",\"LinkSortOrder\":20,\"LinkDescription\":\"UserID/Password Policy\",\"LinkURL\":\"file://wrbts/companies/CSM/IT Policies/WRBC- User ID and Password Policy 2016 Update.pdf\"},{\"LinkID\":66,\"LinkType\":\"Legal\",\"LinkSortOrder\":10,\"LinkDescription\":\"Claims & Legal Resources\",\"LinkURL\":\"http://wiki.theclm.org/\"},{\"LinkID\":67,\"LinkType\":\"Legal\",\"LinkSortOrder\":20,\"LinkDescription\":\"eCourts\",\"LinkURL\":\"https://iapps.courts.state.ny.us/webcivil/FCASMain\"},{\"LinkID\":68,\"LinkType\":\"Legal\",\"LinkSortOrder\":30,\"LinkDescription\":\"eLaw\",\"LinkURL\":\"https://www.elaw.com/eLaw21/Index.aspx\"},{\"LinkID\":155,\"LinkType\":\"Lunch\",\"LinkSortOrder\":10,\"LinkDescription\":\"Chris' Pizza\",\"LinkURL\":\"http://chrispizzeria.com/\"},{\"LinkID\":156,\"LinkType\":\"Lunch\",\"LinkSortOrder\":20,\"LinkDescription\":\"Mambo Tea House\",\"LinkURL\":\"http://www.mamboteahousenj.com/\"},{\"LinkID\":157,\"LinkType\":\"Lunch\",\"LinkSortOrder\":30,\"LinkDescription\":\"Mr. Bruno's Pizza\",\"LinkURL\":\"http://mrbrunoseastrutherford.com/\"},{\"LinkID\":158,\"LinkType\":\"Lunch\",\"LinkSortOrder\":40,\"LinkDescription\":\"Paisano's Restaurant\",\"LinkURL\":\"http://www.paisanos.com/\"},{\"LinkID\":159,\"LinkType\":\"Lunch\",\"LinkSortOrder\":50,\"LinkDescription\":\"Volare's Restaurant\",\"LinkURL\":\"http://www.volaresrestaurant.com/\"},{\"LinkID\":99,\"LinkType\":\"UW\",\"LinkSortOrder\":10,\"LinkDescription\":\"American Insurance Association\",\"LinkURL\":\"http://www.aiadc.org/\"},{\"LinkID\":100,\"LinkType\":\"UW\",\"LinkSortOrder\":20,\"LinkDescription\":\"APS\",\"LinkURL\":\"http://aps.clermonthid.com/aps\"},{\"LinkID\":101,\"LinkType\":\"UW\",\"LinkSortOrder\":30,\"LinkDescription\":\"BCS\",\"LinkURL\":\"https://clearance.wrbts.ads.wrberkley.com/csm/clients\"},{\"LinkID\":102,\"LinkType\":\"UW\",\"LinkSortOrder\":40,\"LinkDescription\":\"Berkley Concierge\",\"LinkURL\":\"https://concierge.wrberkley.com/\"},{\"LinkID\":103,\"LinkType\":\"UW\",\"LinkSortOrder\":50,\"LinkDescription\":\"BPMi\",\"LinkURL\":\"http://bts1-bpmp/ProcessPortal\"},{\"LinkID\":118,\"LinkType\":\"UW\",\"LinkSortOrder\":55,\"LinkDescription\":\"Broker Reports\",\"LinkURL\":\"file://wrbts/companies/CSM/Management Reports/Broker Reports\"},{\"LinkID\":104,\"LinkType\":\"UW\",\"LinkSortOrder\":60,\"LinkDescription\":\"CARE\",\"LinkURL\":\"http://care.berkleyluxurygroup.com/\"},{\"LinkID\":176,\"LinkType\":\"UW\",\"LinkSortOrder\":65,\"LinkDescription\":\"CBC\",\"LinkURL\":\"https://www.creditbureaureports.com/\"},{\"LinkID\":105,\"LinkType\":\"UW\",\"LinkSortOrder\":70,\"LinkDescription\":\"Commercial Express\",\"LinkURL\":\"https://wrberkley.msbcommercial.com/Administration/Account/LogOn?isExpired=False\"},{\"LinkID\":106,\"LinkType\":\"UW\",\"LinkSortOrder\":80,\"LinkDescription\":\"Content Navigator\",\"LinkURL\":\"https://contentnav-na.wrberkley.com/navigator/?desktop=CSM982d4cb6\"},{\"LinkID\":107,\"LinkType\":\"UW\",\"LinkSortOrder\":90,\"LinkDescription\":\"Experian\",\"LinkURL\":\"https://ss1.experian.com/BusinessIQ/login.html\"},{\"LinkID\":108,\"LinkType\":\"UW\",\"LinkSortOrder\":100,\"LinkDescription\":\"FC&S Online\",\"LinkURL\":\"http://www.nationalunderwriterpc.com/Pages/default.aspx\"},{\"LinkID\":109,\"LinkType\":\"UW\",\"LinkSortOrder\":110,\"LinkDescription\":\"FEMA\",\"LinkURL\":\"https://www.fema.gov/\"},{\"LinkID\":110,\"LinkType\":\"UW\",\"LinkSortOrder\":120,\"LinkDescription\":\"GC AdvantagePoint\",\"LinkURL\":\"https://advantagepoint.guycarp.com\"},{\"LinkID\":111,\"LinkType\":\"UW\",\"LinkSortOrder\":130,\"LinkDescription\":\"Genesys\",\"LinkURL\":\"http://csm-prd.wrbts.ads.wrberkley.com/policystarweb/genesys/search\"},{\"LinkID\":112,\"LinkType\":\"UW\",\"LinkSortOrder\":140,\"LinkDescription\":\"IRMI Online\",\"LinkURL\":\"https://www.irmi.com/online/default.aspx\"},{\"LinkID\":113,\"LinkType\":\"UW\",\"LinkSortOrder\":150,\"LinkDescription\":\"ISO NET\",\"LinkURL\":\"https://info.iso.com/content/start.action\"},{\"LinkID\":114,\"LinkType\":\"UW\",\"LinkSortOrder\":160,\"LinkDescription\":\"ODEN State Rules\",\"LinkURL\":\"http://www.odenonline.com/oden/www/index.phtml\"},{\"LinkID\":178,\"LinkType\":\"UW\",\"LinkSortOrder\":165,\"LinkDescription\":\"Policy # Generator\",\"LinkURL\":\"http://png.berkleyluxurygroup.com/\"},{\"LinkID\":115,\"LinkType\":\"UW\",\"LinkSortOrder\":170,\"LinkDescription\":\"Property Shark\",\"LinkURL\":\"https://www.propertyshark.com/mason/\"},{\"LinkID\":116,\"LinkType\":\"UW\",\"LinkSortOrder\":180,\"LinkDescription\":\"Rate Monitor\",\"LinkURL\":\"http://rmu.berkleyluxurygroup.com/\"},{\"LinkID\":117,\"LinkType\":\"UW\",\"LinkSortOrder\":190,\"LinkDescription\":\"UW Docs-Account Mgmt\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/Account Management\"},{\"LinkID\":119,\"LinkType\":\"UW\",\"LinkSortOrder\":200,\"LinkDescription\":\"UW Docs-Bus Dev\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/Business Development\"},{\"LinkID\":120,\"LinkType\":\"UW\",\"LinkSortOrder\":205,\"LinkDescription\":\"UW Docs-Pricing Guidelines\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/Pricing Guidelines\"},{\"LinkID\":121,\"LinkType\":\"UW\",\"LinkSortOrder\":210,\"LinkDescription\":\"UW Docs-Reinsurance Structure\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/Reinsurance Structure\"},{\"LinkID\":175,\"LinkType\":\"UW\",\"LinkSortOrder\":212,\"LinkDescription\":\"UW Docs-Restaurant Business Lost\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting/Restaurant Business Lost/Restaurant Business Lost.xlsx\"},{\"LinkID\":122,\"LinkType\":\"UW\",\"LinkSortOrder\":215,\"LinkDescription\":\"UW Docs-Training Materials\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/Training Materials\"},{\"LinkID\":123,\"LinkType\":\"UW\",\"LinkSortOrder\":220,\"LinkDescription\":\"UW Docs-UW Documents\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/UW Documents\"},{\"LinkID\":124,\"LinkType\":\"UW\",\"LinkSortOrder\":225,\"LinkDescription\":\"UW Docs-UW Guidelines & Bulletins\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/UW Guidelines and Bulletins\"},{\"LinkID\":125,\"LinkType\":\"UW\",\"LinkSortOrder\":230,\"LinkDescription\":\"UW Docs-UW Workflows\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/UW Workflows\"},{\"LinkID\":126,\"LinkType\":\"UW\",\"LinkSortOrder\":235,\"LinkDescription\":\"UW Docs-Worksheets\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared/Worksheets\"},{\"LinkID\":127,\"LinkType\":\"UW\",\"LinkSortOrder\":240,\"LinkDescription\":\"UW Exception Pages\",\"LinkURL\":\"http://expages.berkleyluxurygroup.com/\"},{\"LinkID\":128,\"LinkType\":\"UW\",\"LinkSortOrder\":245,\"LinkDescription\":\"UW Forms\",\"LinkURL\":\"http://forms.berkleyluxurygroup.com/\"},{\"LinkID\":129,\"LinkType\":\"UW\",\"LinkSortOrder\":250,\"LinkDescription\":\"UW Filings\",\"LinkURL\":\"http://filings.berkleyluxurygroup.com/\"},{\"LinkID\":136,\"LinkType\":\"UWOps\",\"LinkSortOrder\":10,\"LinkDescription\":\"BCS\",\"LinkURL\":\"https://clearance.wrbts.ads.wrberkley.com/csm/clients\"},{\"LinkID\":137,\"LinkType\":\"UWOps\",\"LinkSortOrder\":20,\"LinkDescription\":\"BPMi\",\"LinkURL\":\"http://bts1-bpmp/ProcessPortal\"},{\"LinkID\":138,\"LinkType\":\"UWOps\",\"LinkSortOrder\":30,\"LinkDescription\":\"CARE\",\"LinkURL\":\"http://care.berkleyluxurygroup.com/\"},{\"LinkID\":139,\"LinkType\":\"UWOps\",\"LinkSortOrder\":40,\"LinkDescription\":\"CBCInnovis\",\"LinkURL\":\"https://www.creditbureaureports.com/servlet/Presenter\"},{\"LinkID\":140,\"LinkType\":\"UWOps\",\"LinkSortOrder\":50,\"LinkDescription\":\"Content Navigator\",\"LinkURL\":\"https://contentnav-na.wrberkley.com/navigator/?desktop=CSM982d4cb6\"},{\"LinkID\":141,\"LinkType\":\"UWOps\",\"LinkSortOrder\":60,\"LinkDescription\":\"Date and Days Calculator\",\"LinkURL\":\"http://www.calculatorsoup.com/calculators/time/date-day.php\"},{\"LinkID\":142,\"LinkType\":\"UWOps\",\"LinkSortOrder\":70,\"LinkDescription\":\"Experian\",\"LinkURL\":\"https://ss1.experian.com/BusinessIQ/login.html\"},{\"LinkID\":143,\"LinkType\":\"UWOps\",\"LinkSortOrder\":80,\"LinkDescription\":\"GC AdvantagePoint\",\"LinkURL\":\"https://advantagepoint.guycarp.com\"},{\"LinkID\":144,\"LinkType\":\"UWOps\",\"LinkSortOrder\":90,\"LinkDescription\":\"Genesys\",\"LinkURL\":\"http://csm-prd.wrbts.ads.wrberkley.com/policystarweb/genesys/search\"},{\"LinkID\":145,\"LinkType\":\"UWOps\",\"LinkSortOrder\":100,\"LinkDescription\":\"Information Providers\",\"LinkURL\":\"http://www.informationproviders.com\"},{\"LinkID\":146,\"LinkType\":\"UWOps\",\"LinkSortOrder\":110,\"LinkDescription\":\"ISO NET\",\"LinkURL\":\"https://info.iso.com/content/start.action\"},{\"LinkID\":147,\"LinkType\":\"UWOps\",\"LinkSortOrder\":120,\"LinkDescription\":\"Midwest Technical Inspections\",\"LinkURL\":\"http://www.mtinspections.com/index.html\"},{\"LinkID\":148,\"LinkType\":\"UWOps\",\"LinkSortOrder\":130,\"LinkDescription\":\"ODEN Policy Terminator\",\"LinkURL\":\"https://www.odenpt.com/Launch.asp?Launch=1\"},{\"LinkID\":149,\"LinkType\":\"UWOps\",\"LinkSortOrder\":140,\"LinkDescription\":\"PATS\",\"LinkURL\":\"http://pats/companysummary.aspx\"},{\"LinkID\":150,\"LinkType\":\"UWOps\",\"LinkSortOrder\":150,\"LinkDescription\":\"ProMetrix\",\"LinkURL\":\"https://prometrixweb.iso.com/\"},{\"LinkID\":151,\"LinkType\":\"UWOps\",\"LinkSortOrder\":160,\"LinkDescription\":\"Policy # Generator\",\"LinkURL\":\"http://png.berkleyluxurygroup.com/\"},{\"LinkID\":180,\"LinkType\":\"UWOps\",\"LinkSortOrder\":165,\"LinkDescription\":\"Protection Class\",\"LinkURL\":\"http://acw.berkley-bts.com/AdvClientSearch/login.html\"},{\"LinkID\":152,\"LinkType\":\"UWOps\",\"LinkSortOrder\":170,\"LinkDescription\":\"Property Shark\",\"LinkURL\":\"https://www.propertyshark.com/mason/\"},{\"LinkID\":153,\"LinkType\":\"UWOps\",\"LinkSortOrder\":180,\"LinkDescription\":\"UW Forms\",\"LinkURL\":\"http://forms.berkleyluxurygroup.com/\"},{\"LinkID\":154,\"LinkType\":\"UWOps\",\"LinkSortOrder\":190,\"LinkDescription\":\"UW Ops Documents\",\"LinkURL\":\"file://wrbts/companies/CSM/Underwriting Shared Operations\"}]";
            return json;
        }

        public string GetUsersJson()
        {
            var json = "[{\"UserID\":\"acastell\",\"UserFirstName\":\"Anthony\",\"UserLastName\":\"Castella\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"ACastella@clermont.wrberkley.com\",\"UserColor\":\"#4682B4\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Tony\"},{\"UserID\":\"aescalona\",\"UserFirstName\":\"Anna Rosalynn\",\"UserLastName\":\"Escalona\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"AEscalona@clermont.wrberkley.com\",\"UserColor\":\"#9932CC\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Lynn\"},{\"UserID\":\"afamiglietti\",\"UserFirstName\":\"Anthony\",\"UserLastName\":\"Famiglietti\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"afamiglietti@clermont.wrberkley.com\",\"UserColor\":\"#556B2F\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Anthony\"},{\"UserID\":\"agobetz\",\"UserFirstName\":\"Amy\",\"UserLastName\":\"Gobetz\",\"UserRole\":\"HR\",\"UserEmailAddress\":\"AGobetz@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"HR\",\"CalendarName\":\"Amy\"},{\"UserID\":\"BMattucci\",\"UserFirstName\":\"Belinda\",\"UserLastName\":\"Mattucci\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"BMattucci@clermont.wrberkley.com\",\"UserColor\":\"#9932CC\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Belinda\"},{\"UserID\":\"cedowd\",\"UserFirstName\":\"Cecelia\",\"UserLastName\":\"Dowd\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"CeDowd@clermont.wrberkley.com\",\"UserColor\":\"#00c0ef\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Cecelia\"},{\"UserID\":\"cgford\",\"UserFirstName\":\"Christine\",\"UserLastName\":\"Ford\",\"UserRole\":\"Legal\",\"UserEmailAddress\":\"CGFord@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"Legal\",\"CalendarName\":\"Christine\"},{\"UserID\":\"cpietropollo\",\"UserFirstName\":\"Carol\",\"UserLastName\":\"Pietropollo\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"CPietropollo@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Carol\"},{\"UserID\":\"csedereas\",\"UserFirstName\":\"Christian\",\"UserLastName\":\"Sedereas\",\"UserRole\":\"Legal\",\"UserEmailAddress\":\"CSedereas@clermont.wrberkley.com\",\"UserColor\":\"#800000\",\"PrimaryCalendar\":\"Legal\",\"CalendarName\":\"Christian\"},{\"UserID\":\"cslattery\",\"UserFirstName\":\"Caitlin\",\"UserLastName\":\"Slattery\",\"UserRole\":\"IT\",\"UserEmailAddress\":\"CSlattery@clermont.wrberkley.com\",\"UserColor\":\"#9932CC\",\"PrimaryCalendar\":\"IT\",\"CalendarName\":\"Caitlin\"},{\"UserID\":\"cspeer\",\"UserFirstName\":\"Cathy\",\"UserLastName\":\"Speer\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"cspeer@clermont.wrberkley.com\",\"UserColor\":\"#4682B4\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Cathy\"},{\"UserID\":\"dmerlo\",\"UserFirstName\":\"Debra\",\"UserLastName\":\"Merlo\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"dmerlo@clermont.wrberkley.com\",\"UserColor\":\"#C0C0C0\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Deb\"},{\"UserID\":\"dmorales\",\"UserFirstName\":\"Doel\",\"UserLastName\":\"Morales\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"DMorales@clermont.wrberkley.com\",\"UserColor\":\"#000080\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Doel\"},{\"UserID\":\"dwessel\",\"UserFirstName\":\"David\",\"UserLastName\":\"Wessel\",\"UserRole\":\"Accounting\",\"UserEmailAddress\":\"DWessel@clermont.wrberkley.com\",\"UserColor\":\"#800000\",\"PrimaryCalendar\":\"Accounting\",\"CalendarName\":\"Dave W\"},{\"UserID\":\"dxbell\",\"UserFirstName\":\"David\",\"UserLastName\":\"Bell\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"DBell@clermont.wrberkley.com\",\"UserColor\":\"#FF4500\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Dave B\"},{\"UserID\":\"efrank\",\"UserFirstName\":\"Eugene\",\"UserLastName\":\"Frank\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"EFrank@clermont.wrberkley.com\",\"UserColor\":\"#556B2F\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Gene\"},{\"UserID\":\"FBrodie\",\"UserFirstName\":\"Faaizah\",\"UserLastName\":\"Brodie\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"FBrodie@clermont.wrberkley.com\",\"UserColor\":\"#000080\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Faaizah\"},{\"UserID\":\"fportalatin\",\"UserFirstName\":\"Felisha\",\"UserLastName\":\"Portalatin\",\"UserRole\":\"Accounting\",\"UserEmailAddress\":\"FPortalatin@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"Accounting\",\"CalendarName\":\"Felisha\"},{\"UserID\":\"hryerson\",\"UserFirstName\":\"Howard\",\"UserLastName\":\"Ryerson\",\"UserRole\":\"Compliance\",\"UserEmailAddress\":\"HRyerson@clermont.wrberkley.com\",\"UserColor\":\"#800000\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Howard\"},{\"UserID\":\"jecannon\",\"UserFirstName\":\"Jeniece\",\"UserLastName\":\"Cannon\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"JeCannon@clermont.wrberkley.com\",\"UserColor\":\"#9932CC\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Jeniece\"},{\"UserID\":\"jferrent\",\"UserFirstName\":\"John\",\"UserLastName\":\"Ferrentino\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"JFerrentino@clermont.wrberkley.com\",\"UserColor\":\"#C0C0C0\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"John\"},{\"UserID\":\"jmcqueen\",\"UserFirstName\":\"Janice\",\"UserLastName\":\"McQueen\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"JMcQueen@clermont.wrberkley.com\",\"UserColor\":\"#696969\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Janice\"},{\"UserID\":\"jstanczyk\",\"UserFirstName\":\"Jessica\",\"UserLastName\":\"Stanczyk\",\"UserRole\":\"Compliance\",\"UserEmailAddress\":\"jstanczyk@clermont.wrberkley.com\",\"UserColor\":\"#696969\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Jessica\"},{\"UserID\":\"jstrum\",\"UserFirstName\":\"Jodi\",\"UserLastName\":\"Strum\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"JStrum@clermont.wrberkley.com\",\"UserColor\":\"#FF0000\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Jodi\"},{\"UserID\":\"jtobia\",\"UserFirstName\":\"John\",\"UserLastName\":\"Tobia\",\"UserRole\":\"IT\",\"UserEmailAddress\":\"jtobia@clermont.wrberkley.com\",\"UserColor\":\"#e59c49\",\"PrimaryCalendar\":\"IT\",\"CalendarName\":\"John\"},{\"UserID\":\"jvigneau\",\"UserFirstName\":\"Jay\",\"UserLastName\":\"Vigneaux\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"JVigneaux@clermont.wrberkley.com\",\"UserColor\":\"#000080\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Jay\"},{\"UserID\":\"jxjohnson\",\"UserFirstName\":\"Jean\",\"UserLastName\":\"Johnson\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"JJohnson@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Jean\"},{\"UserID\":\"khouston\",\"UserFirstName\":\"Kristy\",\"UserLastName\":\"Houston\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"khouston@clermont.wrberkley.com\",\"UserColor\":\"#87CEFA\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Kristy\"},{\"UserID\":\"knavarra\",\"UserFirstName\":\"Kathy\",\"UserLastName\":\"Navarra\",\"UserRole\":\"Legal\",\"UserEmailAddress\":\"knavarra@clermont.wrberkley.com\",\"UserColor\":\"#4682B4\",\"PrimaryCalendar\":\"Legal\",\"CalendarName\":\"Kathy\"},{\"UserID\":\"koloughlin\",\"UserFirstName\":\"Keith\",\"UserLastName\":\"O'Loughlin\",\"UserRole\":\"IT\",\"UserEmailAddress\":\"koloughlin@clermont.wrberkley.com\",\"UserColor\":\"#4682B4\",\"PrimaryCalendar\":\"IT\",\"CalendarName\":\"Keith\"},{\"UserID\":\"lkushner\",\"UserFirstName\":\"Lauren\",\"UserLastName\":\"Kushner\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"lkushner@clermont.wrberkley.com\",\"UserColor\":\"#87CEFA\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Lauren\"},{\"UserID\":\"lrankin\",\"UserFirstName\":\"Lisa\",\"UserLastName\":\"Rankin\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"LRankin@clermont.wrberkley.com\",\"UserColor\":\"#FF4500\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Lisa\"},{\"UserID\":\"Lzahabian\",\"UserFirstName\":\"Lydie\",\"UserLastName\":\"Zahabian\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"LZahabian@clermont.wrberkley.com\",\"UserColor\":\"#00c0ef\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Lydie\"},{\"UserID\":\"mbiesiada\",\"UserFirstName\":\"Mary Ellen\",\"UserLastName\":\"Biesiada\",\"UserRole\":\"Accounting\",\"UserEmailAddress\":\"MBiesiada@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"Accounting\",\"CalendarName\":\"Mary Ellen\"},{\"UserID\":\"MBueno\",\"UserFirstName\":\"Michael\",\"UserLastName\":\"Bueno\",\"UserRole\":\"IT\",\"UserEmailAddress\":\"MBueno@clermont.wrberkley.com\",\"UserColor\":\"#800000\",\"PrimaryCalendar\":\"IT\",\"CalendarName\":\"Michael\"},{\"UserID\":\"mgiuffri\",\"UserFirstName\":\"Michelle\",\"UserLastName\":\"Giuffrida\",\"UserRole\":\"HR\",\"UserEmailAddress\":\"MGiuffrida@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"HR\",\"CalendarName\":\"Michelle\"},{\"UserID\":\"mimann\",\"UserFirstName\":\"Michele\",\"UserLastName\":\"Mann\",\"UserRole\":\"IT\",\"UserEmailAddress\":\"MMann@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"IT\",\"CalendarName\":\"Michele\"},{\"UserID\":\"mlazarre\",\"UserFirstName\":\"Mia\",\"UserLastName\":\"Lazarre\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"MLazarre@clermont.wrberkley.com\",\"UserColor\":\"#FF0000\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Mia\"},{\"UserID\":\"mperez\",\"UserFirstName\":\"Mercedes\",\"UserLastName\":\"Perez\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"mperez@clermont.wrberkley.com\",\"UserColor\":\"#3CB371\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Mercedes\"},{\"UserID\":\"nogola\",\"UserFirstName\":\"Nicholas\",\"UserLastName\":\"Ogola\",\"UserRole\":\"IT\",\"UserEmailAddress\":\"nogola@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"IT\",\"CalendarName\":\"Nick\"},{\"UserID\":\"rbaer\",\"UserFirstName\":\"Robin\",\"UserLastName\":\"Baer\",\"UserRole\":\"Legal\",\"UserEmailAddress\":\"RBaer@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"Legal\",\"CalendarName\":\"Robin\"},{\"UserID\":\"rbates\",\"UserFirstName\":\"Robert\",\"UserLastName\":\"Bates\",\"UserRole\":\"Legal\",\"UserEmailAddress\":\"rbates@clermont.wrberkley.com\",\"UserColor\":\"#000080\",\"PrimaryCalendar\":\"Legal\",\"CalendarName\":\"Robert\"},{\"UserID\":\"rbuchert\",\"UserFirstName\":\"Robert\",\"UserLastName\":\"Buchert\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"rbuchert@clermont.wrberkley.com\",\"UserColor\":\"#800000\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Bob\"},{\"UserID\":\"rgadaleta\",\"UserFirstName\":\"Rita\",\"UserLastName\":\"Gadaleta\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"RGadaleta@clermont.wrberkley.com\",\"UserColor\":\"#4682B4\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Rita\"},{\"UserID\":\"rlee\",\"UserFirstName\":\"Rebecca\",\"UserLastName\":\"Lee\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"relee@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Rebecca\"},{\"UserID\":\"rmauro\",\"UserFirstName\":\"Richard\",\"UserLastName\":\"Mauro\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"RMauro@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Rich M\"},{\"UserID\":\"rpantovic\",\"UserFirstName\":\"Rada\",\"UserLastName\":\"Pantovic\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"RPantovic@clermont.wrberkley.com\",\"UserColor\":\"#3CB371\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Rada\"},{\"UserID\":\"rself\",\"UserFirstName\":\"Rick\",\"UserLastName\":\"Self\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"RSelf@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Rick\"},{\"UserID\":\"rstarkie\",\"UserFirstName\":\"Richard\",\"UserLastName\":\"Starkie\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"RStarkie@clermont.wrberkley.com\",\"UserColor\":\"#FF0000\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Richard S\"},{\"UserID\":\"rwilletts\",\"UserFirstName\":\"Richard\",\"UserLastName\":\"Willetts\",\"UserRole\":\"UW\",\"UserEmailAddress\":\"rwilletts@clermont.wrberkley.com\",\"UserColor\":\"#28669e\",\"PrimaryCalendar\":\"UW\",\"CalendarName\":\"Rich W\"},{\"UserID\":\"samiller\",\"UserFirstName\":\"Stephanie\",\"UserLastName\":\"Miller\",\"UserRole\":\"Actuary\",\"UserEmailAddress\":\"SAMiller@clermont.wrberkley.com\",\"UserColor\":\"#696969\",\"PrimaryCalendar\":\"Executive\",\"CalendarName\":\"Stephanie\"},{\"UserID\":\"shayes\",\"UserFirstName\":\"Sharon\",\"UserLastName\":\"Hayes\",\"UserRole\":\"Claims\",\"UserEmailAddress\":\"shayes@clermont.wrberkley.com\",\"UserColor\":\"#696969\",\"PrimaryCalendar\":\"Claims\",\"CalendarName\":\"Sharon\"},{\"UserID\":\"sschorli\",\"UserFirstName\":\"Sandra\",\"UserLastName\":\"Schorling\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"sschorling@clermont.wrberkley.com\",\"UserColor\":\"#800000\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Sandi\"},{\"UserID\":\"swhittem\",\"UserFirstName\":\"Shirley\",\"UserLastName\":\"Whittemore\",\"UserRole\":\"UWOps\",\"UserEmailAddress\":\"SWhittemore@clermont.wrberkley.com\",\"UserColor\":\"#f012be\",\"PrimaryCalendar\":\"UWOps\",\"CalendarName\":\"Shirley\"},{\"UserID\":\"vramnarace\",\"UserFirstName\":\"Viro\",\"UserLastName\":\"Ramnarace\",\"UserRole\":\"Accounting\",\"UserEmailAddress\":\"VRamnarace@clermont.wrberkley.com\",\"UserColor\":\"#4169e1\",\"PrimaryCalendar\":\"Accounting\",\"CalendarName\":\"Viro\"},{\"UserID\":\"wjohnston\",\"UserFirstName\":\"William\",\"UserLastName\":\"Johnston\",\"UserRole\":\"Executive\",\"UserEmailAddress\":\"WJohnston@clermont.wrberkley.com\",\"UserColor\":\"#0f543e\",\"PrimaryCalendar\":\"Executive\",\"CalendarName\":\"Bill\"}]";
            return json;
        }
        public string GetUserCalendarJson()
        {
            var json = "[{\"UserCalendarID\":3,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-05-29T00:00:00\",\"EventEndDate\":\"2017-05-29T00:00:00\",\"EventDescription\":\"CSM Holiday - Memorial Day\"},{\"UserCalendarID\":4,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-07-04T00:00:00\",\"EventEndDate\":\"2017-07-04T00:00:00\",\"EventDescription\":\"CSM Holiday - Independence Day\"},{\"UserCalendarID\":49,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-08-11T00:00:00\",\"EventEndDate\":\"2017-08-11T00:00:00\",\"EventDescription\":\"Sick Day\"},{\"UserCalendarID\":51,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-08-21T00:00:00\",\"EventEndDate\":\"2017-08-25T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":5,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-09-04T00:00:00\",\"EventEndDate\":\"2017-09-04T00:00:00\",\"EventDescription\":\"CSM Holiday - Labor Day\"},{\"UserCalendarID\":393,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-10-19T00:00:00\",\"EventEndDate\":\"2017-10-19T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":707,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-11-10T00:00:00\",\"EventEndDate\":\"2017-11-10T00:00:00\",\"EventDescription\":\"1/2 Day PM\"},{\"UserCalendarID\":6,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-11-23T00:00:00\",\"EventEndDate\":\"2017-11-24T00:00:00\",\"EventDescription\":\"CSM Holiday - Thanksgiving\"},{\"UserCalendarID\":737,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-11-27T00:00:00\",\"EventEndDate\":\"2017-11-27T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":928,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-12-06T00:00:00\",\"EventEndDate\":\"2017-12-06T00:00:00\",\"EventDescription\":\"Time Off 1/2 Day PM\"},{\"UserCalendarID\":585,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-12-08T00:00:00\",\"EventEndDate\":\"2017-12-08T00:00:00\",\"EventDescription\":\"CSM Holiday Party 12:30pm\"},{\"UserCalendarID\":7,\"UserID\":\"Shared\",\"EventStartDate\":\"2017-12-25T00:00:00\",\"EventEndDate\":\"2017-12-25T00:00:00\",\"EventDescription\":\"CSM Holiday - Christmas Day\"},{\"UserCalendarID\":1054,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-12-28T00:00:00\",\"EventEndDate\":\"2017-12-28T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":1053,\"UserID\":\"nogola\",\"EventStartDate\":\"2017-12-29T00:00:00\",\"EventEndDate\":\"2017-12-29T00:00:00\",\"EventDescription\":\"Time Off - PM\"},{\"UserCalendarID\":8,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-01-01T00:00:00\",\"EventEndDate\":\"2018-01-01T00:00:00\",\"EventDescription\":\"CSM Holiday - New Year's Day\"},{\"UserCalendarID\":178,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-01-15T00:00:00\",\"EventEndDate\":\"2018-01-15T00:00:00\",\"EventDescription\":\"CSM Holiday - Martin Luther King Day\"},{\"UserCalendarID\":1467,\"UserID\":\"nogola\",\"EventStartDate\":\"2018-01-26T00:00:00\",\"EventEndDate\":\"2018-01-26T00:00:00\",\"EventDescription\":\"1/2 Time Off\"},{\"UserCalendarID\":1582,\"UserID\":\"nogola\",\"EventStartDate\":\"2018-02-09T00:00:00\",\"EventEndDate\":\"2018-02-09T00:00:00\",\"EventDescription\":\"Time Off\"},{\"UserCalendarID\":179,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-02-19T00:00:00\",\"EventEndDate\":\"2018-02-19T00:00:00\",\"EventDescription\":\"CSM Holiday - President's Day\"},{\"UserCalendarID\":180,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-05-28T00:00:00\",\"EventEndDate\":\"2018-05-28T00:00:00\",\"EventDescription\":\"CSM Holiday - Memorial Day\"},{\"UserCalendarID\":181,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-07-04T00:00:00\",\"EventEndDate\":\"2018-07-04T00:00:00\",\"EventDescription\":\"CSM Holiday - Independence Day\"},{\"UserCalendarID\":182,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-09-03T00:00:00\",\"EventEndDate\":\"2018-09-03T00:00:00\",\"EventDescription\":\"CSM Holiday - Labor Day\"},{\"UserCalendarID\":183,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-11-22T00:00:00\",\"EventEndDate\":\"2018-11-23T00:00:00\",\"EventDescription\":\"CSM Holiday - Thanksgiving\"},{\"UserCalendarID\":185,\"UserID\":\"Shared\",\"EventStartDate\":\"2018-12-25T00:00:00\",\"EventEndDate\":\"2018-12-25T00:00:00\",\"EventDescription\":\"CSM Holiday - Christmas Day\"}]";
            return json;
        }
        public IList<CalendarEvent> GetUserCalendar(string userID)
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["csmContext"].ConnectionString;
            DAL dal = new DAL(connectionString);
            List<CalendarEvent> userCalendar = new List<CalendarEvent>();

            GetUsers();
            SqlParameter[] par ={
                                new SqlParameter("UserID", "nogola")
                            };


            //DataSet ds = dal.SelectPolicyList("intra.usp_Get_User_Calendar", par);
            //string JSONresult;
            //JSONresult = JsonConvert.SerializeObject(ds.Tables[0]);

            var json = GetUserCalendarJson();
            DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
            if (dt.Rows.Count != 0)
            {

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[1].ToString().Trim().ToUpper().Equals("SHARED"))
                    {
                        userCalendar.Add(new CalendarEvent
                        {
                            userCalendarId = Convert.ToInt32(dr[0]),
                            UserID = dr[1].ToString(),
                            EventStartDate = (DateTime)dr[2],
                            EventEndDate = (DateTime)dr[3],
                            start = (DateTime)dr[2],
                            end = ((DateTime)dr[3]).AddHours(23).AddMinutes(59).AddSeconds(59),
                            allDay = false,
                            color = "#a1915e", //"#f39c12",
                            title = "Holiday",
                            tooltip = "Holiday",
                            EventDescription = "Holiday"
                        });
                    }
                    else
                    {
                        cmUser user = GetUser(dr[1].ToString());
                        string eventDescription = dr.Field<string>("EventDescription");
                        userCalendar.Add(new CalendarEvent
                        {
                            userCalendarId = Convert.ToInt32(dr[0]),
                            UserID = dr[1].ToString(),
                            EventStartDate = (DateTime)dr[2],
                            EventEndDate = (DateTime)dr[3],
                            start = (DateTime)dr[2],
                            end = ((DateTime)dr[3]).AddHours(23).AddMinutes(59).AddSeconds(59),
                            allDay = false,
                            color = "#00c0ef",
                            title = "Time Off",
                            tooltip = "Time Off",
                            EventDescription = "Time Off"
                        });
                    }
                }

            }
            return userCalendar;

        }
        public Task<List<csmLink>> GetUserLinks(string userID)
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["csmContext"].ConnectionString;
            DAL dal = new DAL(connectionString);
            List<csmLink> userLinks = new List<csmLink>();
          // GetUsers();

            cmUser user = GetUser(userID);

            if (HttpContext.Current.Cache["AllLinks"] != null)
            {
                userLinks = ((List<csmLink>)HttpContext.Current.Cache["AllLinks"]).Where(l => l.LinkType.ToUpper().Equals(user.UserRole.ToUpper())).ToList();
               return Task.Run(() => userLinks);
            }
            else
            {
                SqlParameter[] par ={
                                new SqlParameter("UserID", userID)
                            };

                DataSet ds = dal.SelectPolicyList("intra.usp_Get_All_Links", par);
                if (ds.Tables[0].Rows.Count != 0)
                {

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        userLinks.Add(new csmLink
                        {
                            LinkID = dr.Field<short>("LinkID"),
                            LinkType = dr.Field<string>("LinkType"),
                            LinkSortOrder = dr.Field<byte>("LinkSortOrder"),
                            LinkDescription = dr.Field<string>("LinkDescription"),
                            LinkURL = dr.Field<string>("LinkURL"),
                            Target = dr.Field<string>("LinkURL").Contains("file:") ? "" : "_blank"
                        });
                    }

                }
            }
            return Task.Run(() => userLinks.Where(l => l.LinkType.Equals(user.UserRole)).Where(l => l.LinkType.ToUpper().Equals(user.UserRole.ToUpper())).ToList());

        }

        public Task <List<csmLink>> GetAllLinks()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["csmContext"].ConnectionString;
            DAL dal = new DAL(connectionString);
            List<csmLink> csmLinks = new List<csmLink>();

            //check if links are cached first
            if (HttpContext.Current.Cache["AllLinks"] != null)
            {
                csmLinks = (List<csmLink>)HttpContext.Current.Cache["AllLinks"];
                return Task.Run(() => csmLinks);
            }
            else
            {
                SqlParameter[] par ={
                              new SqlParameter()
                            };

                //DataSet ds = dal.GetDataNoParams("intra.usp_Get_All_Links");
                var json = TeamLinksJson();
                DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));
                //if (ds != null)
                //{
                //    string JSONresult;
                //    JSONresult = JsonConvert.SerializeObject(ds.Tables[0]);
                //}

                if (dt.Rows.Count != 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        csmLinks.Add(new csmLink
                        {
                            LinkID = Convert.ToInt16(dr[0]),
                            LinkType = dr[1].ToString(),
                            LinkSortOrder = Convert.ToByte(dr[0]),
                            LinkDescription = dr[3].ToString(),
                            LinkURL = dr[4].ToString(),
                            Target = dr[4].ToString().Contains("file:") ? "" : "_blank"
                        });
                    }

                }
                HttpContext.Current.Cache["AllLinks"] = csmLinks; 
            }

            return Task.Run(() => csmLinks);

        }
        public Task<List<string>> GetAnnouncements()
        {
            string connectionString =
              ConfigurationManager.ConnectionStrings["csmContext"].ConnectionString;
            DAL dal = new DAL(connectionString);
            List<string> announcements = new List<string>();

            if (HttpContext.Current.Cache["Announcements"] != null)
            {
                announcements = (List<string>)HttpContext.Current.Cache["Announcements"];
                return Task.Run(() => announcements);
            }
            else
            {
                DataSet ds = dal.GetDataNoParams("intra.usp_Get_Announcements");
                if (ds.Tables[0].Rows.Count != 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        announcements.Add(dr.Field<string>("AnnouncementText"));
                    }
                }
                HttpContext.Current.Cache["Announcements"] = announcements;
            }
            return Task.Run(() => announcements);

        }

        public cmUser GetUser(string userID)
        {
            using (var db = new csmContext())
            {
                if (HttpContext.Current.Cache["Users"] != null)
                {
                    return ((List<cmUser>)HttpContext.Current.Cache["Users"]).FirstOrDefault(u => u.UserID.Equals(userID));
                }
                else
                {
                    DAL dal = new DAL(connectionString);
                    var json = GetUsersJson();
                    DataTable dt = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

                    if (dt.Rows.Count != 0)
                    {

                        foreach (DataRow dr in dt.Rows)
                        {
                            users.Add(new cmUser
                            {
                                UserID = dr.Field<string>("UserID"),
                                UserFirstName = dr.Field<string>("UserFirstName"),
                                UserLastName = dr.Field<string>("UserLastName"),
                                UserRole = dr.Field<string>("UserRole"),
                                UserEmailAddress = dr.Field<string>("UserEmailAddress"),
                                UserColor = dr.Field<string>("UserColor"),
                                DefaultCalendar = dr.Field<string>("PrimaryCalendar"),
                                CalendarName = dr.Field<string>("CalendarName")
                            }
                                );
                        }

                    }
                    HttpContext.Current.Cache["Users"] = users;
                    return users.FirstOrDefault(u => u.UserID.Equals(userID));
                }
            }
        }


        //public string GetDefaultCalendar(string userID)
        //{
        //    using (var db = new csmContext())
        //    {
        //        return db.CalendarGroups.Where(u => u.UserID.Equals(userID) && u.IsPrimary == true).Select(d => d.CalendarGroup1).FirstOrDefault();
        //    }
        //}
        public void Dispose()
        {
            GC.Collect();
        }
    }
}