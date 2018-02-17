using System;
using System.Configuration;
using System.IO;
using System.Net;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace FXBusinessLogic
{
    /*
     * Helper class to work with connections in DB for FXMind 
     * To create connection call this at app start:
     * ConnectionHelper.Connect(AutoCreateOption.DatabaseAndSchema);
     */

    internal static class FXConnectionHelper
    {
        public static String connString;
        public static Session dbSession;

        public static string getConnectionString()
        {
            return connString;
        }

        public static void Connect(Session session)
        {
            string DS = ConfigurationManager.ConnectionStrings["FXMind.ConnectionString"].ConnectionString;
            connString = DS;

            var autoCreateOption = AutoCreateOption.None; //.DatabaseAndSchema;
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(connString, autoCreateOption);
            if (dbSession == null) // first connect
            {
                //dbSession = XpoDefault.Session;
                dbSession = new Session(XpoDefault.DataLayer); // this method explicitly set SQL Driver!
            }
            else
            {
                if (session != null)
                    dbSession = session;
                dbSession.Disconnect();
                dbSession.ConnectionString = getConnectionString();
            }
        }

        public static Session GetNewSession()
        {
            return new Session(XpoDefault.DataLayer);
        }

        public static Session Session()
        {
            return dbSession;
        }

        public static IDataStore GetConnectionProvider(AutoCreateOption autoCreateOption)
        {
            return XpoDefault.GetConnectionProvider(getConnectionString(), autoCreateOption);
        }

        public static IDataStore GetConnectionProvider(AutoCreateOption autoCreateOption,
            out IDisposable[] objectsToDisposeOnDisconnect)
        {
            return XpoDefault.GetConnectionProvider(getConnectionString(), autoCreateOption,
                out objectsToDisposeOnDisconnect);
        }

        public static IDataLayer GetDataLayer(AutoCreateOption autoCreateOption)
        {
            return XpoDefault.GetDataLayer(getConnectionString(), autoCreateOption);
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
                "<html><head><title>Current IP Check</title></head><body>Current IP Address: ", string.Empty).
                Replace("</body></html>", string.Empty);
            char[] trim = {'\r', '\n'};
            ipAddress = ipAddress.TrimEnd(trim);
            return ipAddress;
        }
    }
}