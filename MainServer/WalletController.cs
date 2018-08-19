using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace FXMind.MainServer
{
    [RoutePrefix("api")]
    public class WalletController : BaseController
    {
        // GET api/demo 
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<WalletBalance> Get()
        {
            try
            {
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                    return MainService.GetWalletBalance();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public WalletBalance Get(int id)
        {
            try { 
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    var wb = MainService.GetWalletBalance();
                    return wb.Where(d => d.WALLET_ID == id).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public List<WalletBalance> GetRange(int id, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    var wb = MainService.GetWalletBalanceRange(id, fromDate, toDate);
                    return wb;
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }


        [HttpPut]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            try
            {
                if (form == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Form passed to Put method!");
                }
                var key = Convert.ToInt32(form.Get("key"));
                var values = form.Get("values");
                WalletBalance wb = new WalletBalance();
                JsonConvert.PopulateObject(values, wb);
                if (values != null)
                {
                    wb.WALLET_ID = key;
                    var MainService = Program.Container.Resolve<IMainService>();
                    bool bres = false;
                    if (MainService != null)
                    {
                         bres = MainService.UpdateWallet(wb);
                    }
                    if (bres)
                        return Request.CreateResponse(HttpStatusCode.OK);
                }
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, e.ToString());
            }
        }

        // POST api/demo
        [AcceptVerbs("POST")]
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
 
    }
}
