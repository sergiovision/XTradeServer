using BusinessObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;
using ThriftMQL;

namespace BusinessObjects
{

    public class FXMindMQLClient : ThriftClient<FXMindMQL.Client> 
    {

        public FXMindMQLClient(long port)
        {
            Host = "127.0.0.1";
            Port = port;

            InitBase();

            if (String.IsNullOrEmpty(ThriftCalls._FullFilePath))
                ThriftCalls.InitDLL(this);

        }


        public override FXMindMQL.Client CreateClient(TProtocol p)
        {
            client = new FXMindMQL.Client(p);
            return client;
        }

    }
}
