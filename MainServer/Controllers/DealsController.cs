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
using BusinessLogic.Repo;

namespace XTrade.MainServer
{
    [RoutePrefix("api")]
    [Authorize]
    public class DealsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<DealInfo> Get()
        {
            try
            {
                //User.Identity.
                return MainService.GetDeals();
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<MetaSymbolStat> MetaSymbolStatistics()
        {
            try
            {
                var ds =  MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
#if DEBUG
                return ds.MetaSymbolStatistics(true);
#else
                return ds.MetaSymbolStatistics(false);
#endif
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public HttpResponseMessage ClosePosition([FromUri]int Magic, [FromUri]int Ticket)
        {
            try
            {
                SignalInfo signalPos = MainService.CreateSignal(SignalFlags.Expert, Magic, EnumSignals.SIGNAL_CLOSE_POSITION);
                signalPos.Value = (int)Ticket;
                MainService.PostSignalTo(signalPos);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }

        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public HttpResponseMessage RefreshAll()
        {
            try
            {
                List<ExpertsCluster> clusters = MainService.GetClusters();
                SignalInfo signalC = null;
                foreach(var cluster in clusters)
                {
                    signalC = MainService.CreateSignal(SignalFlags.Cluster, cluster.Id, EnumSignals.SIGNAL_ACTIVE_ORDERS);
                    MainService.PostSignalTo(signalC);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }


    }
}

