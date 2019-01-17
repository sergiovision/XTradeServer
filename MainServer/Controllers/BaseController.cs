using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using log4net;

namespace XTrade.MainServer
{
    public class BaseController : ApiController
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(BaseController));

        protected IMainService MainService;
        public BaseController()
        {
            MainService = Program.Container.Resolve<IMainService>();
        }

        [HttpDelete]
        // DELETE api/demo/5 
        [AcceptVerbs("DELETE")]

        virtual public void Delete(int id)
        {
        }
    }
}
