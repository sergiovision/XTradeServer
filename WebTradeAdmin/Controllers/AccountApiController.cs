using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Web.Http;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using BusinessObjects;
using WebTradeAdmin;

namespace WebTradeAdmin.Controllers
{
    public class AccountApiController : BaseApiController<Account>
    {
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            return base.Get(loadOptions, "experts/GetAccounts");
        }

    }
}
