using System;
using System.Threading;
using BusinessObjects;
using FXBusinessLogic.BusinessObjects;
using FXBusinessLogic.ThriftServer;
using log4net;
using Thrift.Server;
using Thrift.Transport;

namespace FXBusinessLogic.Thrift
{
    public class AppServiceServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppServiceServer));
        public static TServer server;
        protected static short port;
        protected Thread myThread;

        public AppServiceServer()
        {
            server = null;
            port = fxmindConstants.AppService_PORT;
        }

        /*public bool StartServer()
        {
            myThread = new Thread(Run);
            myThread.Start();
            return true;
        }*/

        public static void Stop()
        {
            if (server != null)
            {
                log.Info("Shutting Down AppServiceServer Server...");
                server.Stop();
            }
        }

        public static void Run()
        {
            try
            {
                var strPort = MainService.thisGlobal.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_NETSERVERPORT);
                Int16 tryPortValue = port;
                if (Int16.TryParse(strPort, out tryPortValue))
                    port = tryPortValue;

                //Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                var handler = new AppServiceHandler();
                var processor = new AppService.Processor(handler);
                TServerTransport serverTransport = new TServerSocket(port);

                //server = new TSimpleServer(processor, serverTransport);
                // Use this for a multithreaded server. This method works faster.
                server = new TThreadPoolServer(processor, serverTransport);

                log.Info("AppService.NET Server listening... on Port: " + port + " as type: " + server.GetType().Name);
                server.Serve();
            }
            catch (Exception x)
            {
                log.Error(x.ToString());
            }

            server = null;
        }
    }
}