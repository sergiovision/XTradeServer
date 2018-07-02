using log4net;
using System;
using System.Collections.Generic;
using Thrift.Protocol;

namespace BusinessObjects
{

    public class AppServiceClient : ThriftClient<AppService.Client>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppServiceClient));
        public AppServiceClient(string host, short port)
        { 
            Host = host;
            Port = port;
            InitBase();
        }

        public override AppService.Client CreateClient(TProtocol p)
        {
            client = new AppService.Client(p);
            return client;
        }

    }
}