using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Autofac;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace XTrade.MainServer
{
    // Angular example of login
    // https://github.com/tjoudeh/AngularJSAuthentication
    //http://bitoftech.net/2014/06/01/token-based-authentication-asp-net-web-api-2-owin-asp-net-identity/
    [AllowAnonymous]
    [RoutePrefix("api")]
    public class PersonController : BaseController
    {
        [HttpPost]
        public IHttpActionResult Register(Person userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Implement register user
            //IdentityResult result = await _repo.RegisterUser(userModel);

            //IHttpActionResult errorResult = GetErrorResult(result);

            //if (errorResult != null)
            //{
            //    return errorResult;
            //}

            return Ok();
        }
 
    }
}
