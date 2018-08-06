using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Mvc;
using log4net;
using BusinessObjects;
using Newtonsoft.Json;

namespace WebTradeAdmin.Controllers {

    [Authorize]
    public class JobsController : Controller {
        protected static readonly ILog log = LogManager.GetLogger(typeof(JobsController));

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RunJob(string Group, string Name)
        {
            HttpResponseMessage res = null;
            try
            {
                string uri = $"http://localhost:2013/api/jobs/Put";
                HttpClient client = new HttpClient();
                ScheduledJobView job = new ScheduledJobView();
                job.Group = Group;
                job.Name = Name;
                var response = client.PutAsJsonAsync<ScheduledJobView>(uri, job);
                response.Wait();
                return Redirect("~/Jobs/Index");
                //return View("~/Views/Jobs/Index.cshtml");
                //return response.Result;
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                res = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                res.Content = new StringContent(e.ToString());
                return View(res);
                //return res;
            }
        }


    }
}