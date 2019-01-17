using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Autofac;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace XTrade.MainServer
{
    [Authorize]
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
                var list = MainService.GetAllJobsList();
                //var list = MainService.GetRunningJobs();
                int i = 1;
                foreach (var job in list)
                jobs.Add(CreateJobView(i++, job));
                return jobs;
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
                var list = MainService.GetRunningJobs();
                int i = 1;
                foreach (var job in list)
                    jobs.Add(CreateJobView(i++, job.Value));
                return jobs;
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
            }
            return null;
        }

        private ScheduledJobView CreateJobView(int i, ScheduledJobInfo job)
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


        public class JobParam
        {
            public string Group { get; set; }
            public string Name { get; set; }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public HttpResponseMessage Post(JobParam query)
        { 
            try
            {
                if (query == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Params passed to RunJob method!");
                }
                MainService.RunJobNow(query.Group, query.Name);
                return Request.CreateResponse(HttpStatusCode.OK, $"Job {query.Name} Launched!");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [AcceptVerbs("POST")]
        [HttpPost]
        public HttpResponseMessage Stop(JobParam query)
        {
            try
            {
                if (query == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Params passed to RunJob method!");
                }
                MainService.StopJobNow(query.Group, query.Name);
                return Request.CreateResponse(HttpStatusCode.OK, $"Job {query.Name} Stop Request Sent!");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }


    }
}
