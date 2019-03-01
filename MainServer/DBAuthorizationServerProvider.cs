using Autofac;
using BusinessObjects;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace XTrade.MainServer
{
    public class DBAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.CompletedTask;
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            var MainService = Program.Container.Resolve<IMainService>();

            Person result = MainService.LoginPerson(context.UserName, context.Password);
            if (result == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                context.Validated();
                return Task.FromResult<object>(null);
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Role, result.Privilege));
            identity.AddClaim(new Claim("sub", context.UserName));

            var props = new AuthenticationProperties(new Dictionary<string, string>
            {
                {
                    "as:client_id", context.ClientId == null ? string.Empty : context.ClientId
                },
                {
                    "userName", context.UserName
                }
            });

            var ticket = new AuthenticationTicket(identity, props);

            context.Validated(ticket);
            return Task.CompletedTask;
        }
    }
}