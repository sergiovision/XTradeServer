using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using log4net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace FXMind.MainServer
{
    public class BaseController : ApiController
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(BaseController));


        [HttpDelete]
        // DELETE api/demo/5 
        [AcceptVerbs("DELETE")]

        virtual public void Delete(int id)
        {
        }
    }
}
