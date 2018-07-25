using BusinessObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace BusinessObjects
{

    public class FXMindMQLClient : ThriftClient<FXMindMQL.Client> 
    {

        public FXMindMQLClient(long port)
        {
            Host = "127.0.0.1";
            Port = (int)port;
            InitBase();
        }

        public override FXMindMQL.Client CreateClient(TProtocol p)
        {
            client = new FXMindMQL.Client(p);
            return client;
        }

    }
}
