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

namespace WebTradeAdmin.Controllers
{
    public class ExpertsApiController : BaseApiController<Adviser>
    {
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            return base.Get(loadOptions, "experts");
        }

        public HttpResponseMessage Get(int id, DataSourceLoadOptions loadOptions)
        {
            return base.Get(loadOptions, $"experts/GetByAccount/{id}");
        }

        /*
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

        */
    }
}
