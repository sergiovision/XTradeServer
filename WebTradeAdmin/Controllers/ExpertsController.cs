using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace WebTradeAdmin.Controllers {
    [Authorize]
    public class ExpertsController : Controller {
        protected static readonly ILog log = LogManager.GetLogger(typeof(ExpertsController));

        public ActionResult Index() {
            return View();
        }

        [HttpGet]
        public ActionResult GenerateDeployScript()
        {
            HttpResponseMessage res = null;
            try
            {
                string uri = $"http://localhost:2013/api/experts/GenerateDeployScripts";
                HttpClient client = new HttpClient();
                var response = client.GetAsync(uri);
                if (response != null)
                   response.Wait();
                return Redirect("~/Experts/Index");
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

        [HttpGet]
        public ActionResult Deploy(int id)
        {
            HttpResponseMessage res = null;
            try
            {
                string uri = $"http://localhost:2013/api/experts/DeployScript?ID={id}";
                HttpClient client = new HttpClient();
                var response = client.GetAsync(uri);
                if (response != null)
                    response.Wait();
                return Redirect("~/Experts/Index");
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