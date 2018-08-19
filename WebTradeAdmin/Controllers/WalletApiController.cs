using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using BusinessObjects;
using System.Net;
using System.IO;
using System.Net.Http.Formatting;

namespace WebTradeAdmin.Controllers
{
    public class WalletApiController : BaseApiController<WalletBalance>
    {
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            return base.Get(loadOptions, "wallet");
        }
       
        public HttpResponseMessage Put(FormDataCollection form)
        {
            HttpResponseMessage res = null;
            try
            {
                //FormDataCollection form
                string uri = $"http://localhost:2013/api/wallet/Put";
                HttpClient client = new HttpClient();                
                var response = client.PutAsJsonAsync<FormDataCollection>(uri, form);
                response.Wait();
                return response.Result.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                res = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                res.Content = new StringContent(e.ToString());
                return res;
            }
        }

        /*
        // GET: Jobs
        public ActionResult Index()
        {
            return View();
        }

        // GET: Jobs/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Jobs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Jobs/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Jobs/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Jobs/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        */

    }
}
