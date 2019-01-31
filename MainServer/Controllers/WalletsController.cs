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
using BusinessLogic.Repo;

namespace XTrade.MainServer
{
    [Authorize]
    [RoutePrefix("api")]
    public class WalletsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<Wallet> Get()
        {
            try
            {
                return MainService.GetWalletsState(DateTime.MaxValue);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public Wallet Get(int id)
        {
            try { 
                var wb = MainService.GetWalletsState(DateTime.MaxValue);
                return wb.Where(d => d.Id.Equals(id)).FirstOrDefault();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public List<Wallet> GetRange(int id, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var w = MainService.GetWalletBalanceRange(id, fromDate, toDate);
                return w;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        // Warning: month is Zero based - January equal 0
        [HttpGet]
        [AcceptVerbs("GET")]
        public List<TimeStat> Performance([FromUri]int month, [FromUri]int period)
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.Performance(month, (TimePeriod)period);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpPut]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Put(AccountState state)
        {
            try
            {
                if (state == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty state passed to Put method!");
                }
                bool bres = MainService.UpdateAccountState(state);
                if (bres)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to Update Account State");
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
