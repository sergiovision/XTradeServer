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
            try
            {
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                    return MainService.GetExperts();
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
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
            try
            {
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    var wb = MainService.GetExperts();
                    return wb.Where(d => d.TERMINAL_ID == id);
                }
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public IEnumerable<Account> GetAccounts()
        {
            try
            {
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                    return MainService.GetAccounts();
            } catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return null;
        }

        [HttpGet]
        [AcceptVerbs("GET")]
        public string GenerateDeployScripts()
        {
            try
            { 
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    string sourceFolder = MainService.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_MQLSOURCEFOLDER);
                    MainService.DeployToTerminals(sourceFolder);
                    return "OK";
                }
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
                var MainService = Program.Container.Resolve<IMainService>();
                if (MainService != null)
                {
                    MainService.DeployToAccount(ID);
                    return "OK";
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return "FAILED";
        }
    }
}

