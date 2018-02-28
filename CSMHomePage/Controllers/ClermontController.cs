using CSMHomePage.Classes;
using CSMHomePage.Data;
using CSMHomePage.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace CSMHomePage.Controllers
{

    public class ClermontController : ApiController
    {
        private List<Permission> Permissions { get; set; }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("Entries")]
        public IHttpActionResult GetUserTeamCalendar()
        {
            using (Repository repo = new Repository())
            {
                repo.connectionString =
                      System.Configuration.ConfigurationManager.ConnectionStrings["csmContext"]
                          .ConnectionString;
                //log.Info("test");
                var res = Json(repo.GetUserTeamCalendar(Utilities.GetUser()));
                return res;
            }
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("TeamCalendar/{team}")]
        public IHttpActionResult GetTeamCalendar(string team)
        {
            using (Repository repo = new Repository())
            {
                string clermont_team = string.Empty;
                switch (team.Trim())
                {
                    case "Underwriting":
                        clermont_team = "UW";
                        break;
                    case "UW Operations":
                        clermont_team = "UWOps";
                        break;
                    default:
                        clermont_team = team;
                        break;
                }
                repo.connectionString =
                      System.Configuration.ConfigurationManager.ConnectionStrings["csmContext"]
                          .ConnectionString;

                return Ok(repo.GetTeamCalendar(clermont_team));
            }
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("UserEntries")]
        public IHttpActionResult GetUserCalendar()
        {
            using (Repository repo = new Repository())
            {
                repo.connectionString =
                      System.Configuration.ConfigurationManager.ConnectionStrings["csmContext"]
                          .ConnectionString;

                return Json(repo.GetUserCalendar(Utilities.GetUser()));
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("AllLinks")]
        public async Task<IHttpActionResult> GetAllLinks()
        {
            using (Repository repo = new Repository())
            {
                LinksVM links = new LinksVM();
                if (HttpContext.Current.Cache["AllLinks"] != null)
                {
                    links.AllLinks = (List<csmLink>)HttpContext.Current.Cache["AllLinks"];
                }
                else
                {
                  links.AllLinks = await repo.GetAllLinks();
                  HttpContext.Current.Cache["AllLinks"] = links.AllLinks;
                }
                links.UserLinks = await repo.GetUserLinks(Utilities.GetUser());
                return Ok(links);
            }
        }
        //[System.Web.Http.HttpGet]
        [System.Web.Http.Route("ClearCache")]
        public IHttpActionResult ClearCache()
        {
            HttpContext.Current.Cache.Remove("AllLinks"); // clear cache by force
            HttpContext.Current.Cache.Remove("Users");
            HttpContext.Current.Cache.Remove("Announcements");
            return Ok("success");
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("Announcements")]
        public IHttpActionResult GetAnnouncements()
        {
            using (Repository repo = new Repository())
            {
                return Ok(repo.GetAnnouncements());
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("User")]
        public IHttpActionResult GetUser()
        {
            using (Repository repo = new Repository())
            {
                return Ok("nogola");
            }
        }
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("EventExists")]
        public IHttpActionResult EventExists(DateTime? startDate, DateTime? endDate, string eventDescription)
        {
            using (Repository repo = new Repository())
            {
                return Ok(repo.EventExists(startDate, endDate, eventDescription));
            }
        }
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("SaveEvent")]
        public IHttpActionResult SaveEvent(DateTime? startDate, DateTime? endDate, string eventDescription)
        {
            using (Repository repo = new Repository())
            {
                return Ok(repo.SaveEvent(startDate, endDate != null ? endDate : startDate, eventDescription, Utilities.GetUser()));
            }
        }
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateEvent")]
        public IHttpActionResult UpdateEvent(int userCalendarID, DateTime? startDate, DateTime? endDate, string eventDescription)
        {
            using (Repository repo = new Repository())
            {
                return Ok(repo.UpdateEvent(userCalendarID, startDate, endDate, eventDescription, Utilities.GetUser()));
            }
        }
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("DeleteEvent")]
        public IHttpActionResult DeleteEvent(int userCalendarID)
        {
            using (Repository repo = new Repository())
            {
                return Ok(repo.DeleteEvent(userCalendarID));
            }
        }

        private readonly string baseUri = ConfigurationManager.AppSettings["baseUri"];

        public List<Permission> GetPermissions(string userId)
        {
            string uri = baseUri + Utilities.GetUser();
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            using (HttpClient httpClient = new HttpClient(handler))
            {
                Task<String> response = httpClient.GetStringAsync(uri);
             //   var result = Task
                return JsonConvert.DeserializeObjectAsync<List<Permission>>(response.Result).Result;
            }

        }
    }
}
