using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using log4net;
using Autofac;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace WebTradeAdmin.Controllers
{
    public class BaseApiController<T> : ApiController
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(BaseApiController<T>));

        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions, string controllerName)
        {
            HttpResponseMessage res = null;
            try
            {
                string uri = $"http://localhost:2013/api/{controllerName}";
            
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                request.Method = "get";
                //request.Headers.Add("apikey: Apikey");
                string stringResponse = string.Empty;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    stringResponse = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<List<T>>(stringResponse);
                    return Request.CreateResponse(DataSourceLoader.Load(result, loadOptions));
                }
            } catch (Exception e)
            {
                log.Info(e.ToString());
                res = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                res.Content = new StringContent(e.ToString());
            }
            return res;
        }
    }
}
