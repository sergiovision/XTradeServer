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

namespace XTrade.MainServer
{
    [RoutePrefix("api")]
    [Authorize]
    public class ExpertsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<ExpertsCluster> Get()
        {
            try
            {
                //User.Identity.
                return MainService.GetClusters();
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        /*
        [HttpGet]
        [AcceptVerbs("GET")]
        public Adviser Get(int id)
        {
            var MainService = Program.Container.Resolve<IMainService>();
            if (MainService != null)
            {
                var wb = MainService.GetExperts();
                return wb.Where(d => d.ID == id).FirstOrDefault();
            }
            return null;
        }
        */

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<Adviser> GetByTerminal(int id)
        {
            try
            {
                var wb = MainService.GetAdvisers();
                return wb.Where(d => d.TerminalId == id);
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<Terminal> GetTerminals()
        {
            try
            {
                return MainService.GetTerminals();
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        /*[HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<PositionInfo> Positions()
        {
            try
            {
                return MainService.GetPositions();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }
        */

        [AllowAnonymous]
        [HttpGet]
        [AcceptVerbs("GET")]
        public string GenerateDeployScripts()
        {
            try
            { 
                string sourceFolder = MainService.GetGlobalProp(xtradeConstants.SETTINGS_PROPERTY_MQLSOURCEFOLDER);
                MainService.DeployToTerminals(sourceFolder);
                return "OK";
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return "FAILED";
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public string DeployScript(int ID)
        {
            try
            {
                return MainService.DeployToAccount(ID);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return $"Deploy to Account: {ID} FAILED: " + e.ToString();
            }
        }

        [AcceptVerbs("PUT")]
        [HttpPut]
        public HttpResponseMessage Put(Terminal terminal)
        {
            try
            {
                if (terminal == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Terminal passed to Put method!");
                }
                if (MainService.UpdateTerminals(terminal))
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to update");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [AcceptVerbs("PUT")]
        [HttpPut]
        public HttpResponseMessage UpdateAdviserState(Adviser adviser)
        {
            try
            {
                if (adviser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Empty Adviser passed to UpdateAdviserState method!");
                }
                if (MainService.UpdateAdviser(adviser))
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Failed to update");
            }
            catch (Exception e)
            {
                log.Info(e.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}

