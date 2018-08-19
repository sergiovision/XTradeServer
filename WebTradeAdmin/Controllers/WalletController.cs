using BusinessObjects;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace WebTradeAdmin.Controllers {

    [Authorize]
    public class WalletController : Controller {
        public ActionResult Index() {

            return View();
        }

        [HttpGet]
        public ActionResult GetRange(DataSourceLoadOptions loadOptions)
        {
            HttpResponseMessage res = null;
            try
            {
                int id = 0;
                string fromDate = new DateTime(2018, 1, 1).ToString(fxmindConstants.SOLRDATETIMEFORMAT);
                string toDate = DateTime.UtcNow.ToString(fxmindConstants.SOLRDATETIMEFORMAT);
                string uri = $"http://localhost:2013/api/wallet/GetRange?id={id}&fromDate={fromDate}&toDate={toDate}";

                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                request.Method = "get";
                //request.Headers.Add("apikey: Apikey");
                string stringResponse = string.Empty;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    stringResponse = reader.ReadToEnd();
                    //var result = JsonConvert.DeserializeObject<List<T>>(stringResponse);
                    return Content(stringResponse, "application/json");
                    //return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            }
            catch (Exception e)
            {
                //log.Info(e.ToString());
                res = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                res.Content = new StringContent(e.ToString());
            }
            return Content("Error GetRange");
            // return Content(JsonConvert.SerializeObject(SampleData.StockPrices), "application/json");
        }
    }
}