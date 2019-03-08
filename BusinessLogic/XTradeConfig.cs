using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public static class XTradeConfig
    {
        public static string ConnectionStringName()
        {
            string result = ConfigurationManager.AppSettings["ConnectionStringName"];
            return result;
        }

        public static string ConnectionString()
        {
            string result = ConfigurationManager.AppSettings[ConnectionStringName()];
            return result;
        }

        public static string AngularDir()
        {
            try
            {
                string result = ConfigurationManager.AppSettings["AngularDir"];
                return result;
            } catch
            {
                return xtradeConstants.ANGULAR_DIR;
            }
        }

        public static short WebPort()
        {
            try
            {
                string result = ConfigurationManager.AppSettings["WebPort"];
                return Int16.Parse(result);
            }
            catch
            {
                return xtradeConstants.WebBackend_PORT;
            }
        }


    }
}
