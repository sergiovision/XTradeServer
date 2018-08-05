using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Autofac;
using System.Net.Http.Formatting;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace FXMind.MainServer
{
    [RoutePrefix("api")]
    public class JobsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<ScheduledJobView> Get()
        {
            try
            {
                List<ScheduledJobView> jobs = new List<ScheduledJobView>();
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    var list = MainService.GetAllJobsList();
                    //var list = MainService.GetRunningJobs();
                    int i = 1;
                    foreach (var job in list)
                        jobs.Add(CreateJobView(i++, job));
                    return jobs;
                }
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<ScheduledJobView> GetRunning()
        {
            try
            {
                List<ScheduledJobView> jobs = new List<ScheduledJobView>();
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    var list = MainService.GetRunningJobs();
                    int i = 1;
                    foreach (var job in list)
                        jobs.Add(CreateJobView(i++, job.Value));
                    return jobs;
                }
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
            }
            return null;
        }

        private ScheduledJobView CreateJobView(int i, ScheduledJob job)
        { 
            return new ScheduledJobView() {
                ID = i,
                IsRunning = job.IsRunning,
                Name = job.Name,
                Group = job.Group,
                PrevDate = new DateTime(job.PrevTime, DateTimeKind.Utc),
                NextDate = new DateTime(job.NextTime, DateTimeKind.Utc),
                Schedule = job.Schedule,
                Log = job.Log
            };
        }

        [HttpPut]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Put(ScheduledJobView form)
        {
            try
            {
                if (form == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Form passed to Put method!");
                }
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null && (form != null))
                {
                    MainService.RunJobNow(form.Group, form.Name);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
 
    }
}
