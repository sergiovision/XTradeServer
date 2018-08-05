using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;

namespace FXMind.MainServer
{
    [RoutePrefix("api")]
    public class ExpertsController : BaseController
    {
        // GET api/demo 
        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<Adviser> Get()
        {
            var MainService = Program.Container.Resolve<IMainService>();
            if (MainService != null)
                return MainService.GetExperts();
            return null;
        }

        // GET api/demo/5 
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
        public IEnumerable<Adviser> GetByAccount(int id)
        {
            var MainService = Program.Container.Resolve<IMainService>();
            if (MainService != null)
            {
                var wb = MainService.GetExperts();
                return wb.Where(d => d.TERMINAL_ID == id);
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<Account> GetAccounts()
        {
            var MainService = Program.Container.Resolve<IMainService>();
            if (MainService != null)
                return MainService.GetAccounts();
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public string GenerateDeployScripts()
        {
            var MainService = Program.Container.Resolve<IMainService>();
            if (MainService != null)
            {
                string sourceFolder = MainService.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_MQLSOURCEFOLDER);
                MainService.DeployToTerminals(sourceFolder);
                return "OK";
            }
            return "FAILED";
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public string DeployScript(int id)
        {
            var MainService = Program.Container.Resolve<IMainService>();
            if (MainService != null)
            {
                MainService.DeployToAccount(id);
                return "OK";
            }
            return "FAILED";
        }


    }
}
