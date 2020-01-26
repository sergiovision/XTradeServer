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
    public class PropsController : BaseController
    {
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<DynamicProperties> Get()
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.GetAllProperties();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public DynamicProperties GetInstance([FromUri]short entityType, [FromUri]  int objId)
        {
            try
            {
                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                return ds.GetPropertiesInstance(entityType, objId);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return null;
        }

        [AcceptVerbs("PUT")]
        [HttpPut]
        public HttpResponseMessage SaveInstance(DynamicProperties props)
        {
            try
            {
                if (props == null)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError,
                        "Empty DynamicProperties passed to Put method!");

                var ds = MainService.Container.Resolve<DataService>();
                if (ds == null)
                    return null;
                if (ds.SavePropertiesInstance(props))
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