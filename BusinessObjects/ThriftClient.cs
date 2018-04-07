using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace BusinessObjects
{
    public abstract class ThriftClient<T> : IDisposable
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public T client { get; set; }

        protected TSocket transport;
        protected TProtocol protocol;

        protected void InitBase()
        {
            transport = new TSocket(Host, Port);
            protocol = new TBinaryProtocol(transport);
            client = CreateClient(protocol);
            transport.Open();
        }

        public void Dispose()
        {
            if ((transport != null) && transport.IsOpen)
            {
                transport.Close();
                var disp = client as IDisposable;
                if (disp != null)
                    disp.Dispose();
            }
        }

        public abstract T CreateClient(TProtocol p);
    }
}
