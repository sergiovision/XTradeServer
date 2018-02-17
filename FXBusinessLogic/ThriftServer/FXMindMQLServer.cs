using System;
using System.Threading;
using BusinessObjects;
using FXBusinessLogic.ThriftServer;
using log4net;
using Thrift.Server;
using Thrift.Transport;

namespace FXBusinessLogic.Thrift
{
    public class FXMindMQLServer 
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (FXMindMQLServer));
        public static TServer server;
        protected static short port;
        protected Thread myThread;

        public FXMindMQLServer(short p)
        {
            server = null;
            port = p;
        }

        public bool StartServer()
        {
            myThread = new Thread(Run);
            myThread.Start();
            return true;
        }

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
                System.Threading.Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                var handler = new FXMindMQLHandler();
                var processor = new FXMindMQL.Processor(handler);
                TServerTransport serverTransport = new TServerSocket(port);

                //server = new TSimpleServer(processor, serverTransport);
                // Use this for a multithreaded server. This method works faster.
                server = new TThreadPoolServer(processor, serverTransport);

                log.Info("FXMindMQLServer listening...");
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