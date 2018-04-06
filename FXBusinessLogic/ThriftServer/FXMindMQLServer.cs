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
    public class FXMindMQLServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FXMindMQLServer));
        public static TServer server;
        protected static short port;
        protected Thread myThread;

        public FXMindMQLServer()
        {
            server = null;
            port = fxmindConstants.FXMindMQL_PORT;
        }

        /*
         * public bool StartServer()
        {

            myThread = new Thread(Run);
            myThread.Start();
            return true;
        }
        */

        public static void Stop()
        {
            if (server != null)
            {
                log.Info("Shutting Down FXMindMQLServer Server...");
                server.Stop();
            }
        }

        public static void Run()
        {
            try
            {
                var strPort = MainService.thisGlobal.GetGlobalProp(MainService.SETTINGS_PROPERTY_THRIFTPORT);
                Int16 tryPortValue = port;
                if (Int16.TryParse(strPort, out tryPortValue))
                    port = tryPortValue;

                Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                var handler = new FXMindMQLHandler();
                var processor = new FXMindMQL.Processor(handler);
                TServerTransport serverTransport = new TServerSocket(port);

                //server = new TSimpleServer(processor, serverTransport);
                // Use this for a multithreaded server. This method works faster.
                server = new TThreadPoolServer(processor, serverTransport);

                log.Info("FXMindMQLServer listening... on Port: " + port.ToString());
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