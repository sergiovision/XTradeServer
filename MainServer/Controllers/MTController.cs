using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace XTrade.MainServer
{
    [RoutePrefix("api")]
    [AllowAnonymous]
    public class MTController : BaseController
    {
        [HttpPost]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SendSignal(HttpRequestMessage request)
        {
            try
            {
                var response = request.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(response.Result))
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty data passed as a parameter");
                SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(response.Result);
                var result = MainService.SendSignal(signal);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            //await Task.CompletedTask;
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [AcceptVerbs("POST")]
        public HttpResponseMessage PostSignal(HttpRequestMessage request)
        {
            try
            {
                var response = request.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(response.Result))
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "NULL Signal passed as a parameter");
                SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(response.Result);
                if (signal == null)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Broken Signal passed as a parameter");
                MainService.PostSignalTo(signal);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ListenSignal(HttpRequestMessage request)
        {
            try
            {
                var response = request.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(response.Result))
                    return Request.CreateResponse(HttpStatusCode.OK);
                SignalInfo signal = JsonConvert.DeserializeObject<SignalInfo>(response.Result);
                if (signal == null)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Broken Signal passed as a parameter");
                var result = MainService.ListenSignal(signal.ObjectId, signal.Flags);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}

