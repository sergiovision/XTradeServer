using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using System;
using System.Collections.Generic;
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
                return MainService.GetDeals();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<DealInfo> GetToday()
        {
            try
            {
                var ds = MainService.Container.Resolve<ITerminalEvents>();
                if (ds == null)
                    return null;
                return ds.GetTodayDeals();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public TodayStat GetTodayStat()
        {
            try
            {
                var ds = MainService.Container.Resolve<ITerminalEvents>();
                if (ds == null)
                    return null;
                return ds.GetTodayStat();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return null;
        }


        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<MetaSymbolStat> MetaSymbolStatistics([FromUri] int type)
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.MetaSymbolStatistics(type);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public HttpResponseMessage ClosePosition([FromUri] int account, [FromUri] int Magic, [FromUri] int Ticket)
        {
            try
            {
                SignalInfo signalPos = null;
                if (Ticket > 0)
                {
                    signalPos = MainService.CreateSignal(SignalFlags.Terminal, account, EnumSignals.SIGNAL_CLOSE_POSITION);
                } else
                {
                    signalPos = MainService.CreateSignal(SignalFlags.Expert, Magic, EnumSignals.SIGNAL_CLOSE_POSITION);
                }
                signalPos.Value = Ticket;
                signalPos.Data = Magic.ToString();
                MainService.PostSignalTo(signalPos);
                var terminals = MainService.Container.Resolve<ITerminalEvents>();
                if (terminals != null)
                    terminals.DeletePosition(Ticket);

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
                foreach (var cluster in clusters)
                {
                    signalC = MainService.CreateSignal(SignalFlags.Cluster, cluster.Id,
                        EnumSignals.SIGNAL_ACTIVE_ORDERS);
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