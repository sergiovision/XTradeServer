using System;
using System.Configuration;
using System.IO;
using System.Net;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace BusinessLogic
{
    internal static class ConnectionHelper
    {
        public static string connString;
        private readonly static object lockObject = new object();
        private static ISessionFactory _sessionFactory;

        public static string getMysqlConnectionString()
        {
            connString = ConfigurationManager.ConnectionStrings["XTrade.MySQLConnection"].ConnectionString;
            return connString;
        }

        public static ISession CreateNewSession()
        {
            if (_sessionFactory == null)
            {
                string connection = getMysqlConnectionString();
                var mysqlConfig = MySQLConfiguration.Standard.ConnectionString(connection);
                _sessionFactory = FluentNHibernate.Cfg.Fluently.Configure().Database(mysqlConfig)
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Repo.DBAdviser>())
                .BuildSessionFactory();
            }

            /*            
             *            ISessionFactory _sessionFactory = FluentNHibernate.Cfg.Fluently.Configure()
                            .Database(getConnectionString())
                            .Mappings(x => x.FluentMappings.AddFromAssemblyOf<UsersMap>())
                            //.ExposeConfiguration(cfg => new SchemaExport(cfg).Create(false, true)) //when i removed this line it doesn't
                            //remove the elements from the db
                            .BuildSessionFactory(); 
            */
            lock (lockObject) // Session is not thread safe thus - should be locked.
            {
                return _sessionFactory.OpenSession();
            }
        }

        private static string GetComputer_InternetIP()
        {
            // check IP using DynDNS's service
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org");
            // IMPORTANT: set Proxy to null, to drastically INCREASE the speed of request
            request.Proxy = null;
            WebResponse response = request.GetResponse();
            var stream = new StreamReader(response.GetResponseStream());


            // read complete response
            string html = stream.ReadToEnd();

            // replace everything and keep only IP
            string ipAddress = html.Replace(
                    "<html><head><title>Current IP Check</title></head><body>Current IP Address: ", string.Empty)
                .Replace("</body></html>", string.Empty);
            char[] trim = {'\r', '\n'};
            ipAddress = ipAddress.TrimEnd(trim);
            return ipAddress;
        }
    }
}