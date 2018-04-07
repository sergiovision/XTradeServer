using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace FXBusinessLogic.BusinessObjects.Thrift
{

    public class FXMindMQLClient : ThriftClient<FXMindMQL.Client> 
    {
        public FXMindMQLClient(string host, ushort port)
        {
            try
            {
                Host = host;
                Port = port;
                InitBase();
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (SocketException s)
            {
                Console.WriteLine(s.ToString());
            }
        }

        public override FXMindMQL.Client CreateClient(TProtocol p)
        {
            client = new FXMindMQL.Client(p);
            return client;
        }

/*
        public List<string> ProcessStringData(Dictionary<string, string> paramsList, List<string> inputData)
        {
            List<string> list = new List<string>();
            try
            {
                //lock (this)
                {

                    transport.Open();
                    try
                    {
                        list = client.ProcessStringData(paramsList, inputData);
                    }
                    finally
                    {
                        transport.Close();
                    }
                }
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (SocketException s)
            {
                Console.WriteLine(s.ToString());
            }
            return list;
        }

        public List<double> ProcessDoubleData(Dictionary<string, string> paramsList, List<string> inputData)
        {
            List<double> list = new List<double>();
            try {
                //lock (this)
                {

                    transport.Open();
                    try
                    {
                        list = client.ProcessDoubleData(paramsList, inputData);
                    }
                    finally
                    {
                        transport.Close();
                    }
                }
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (SocketException s)
            {
                Console.WriteLine(s.ToString());
            }
            return list;
        }

        public long IsServerActive(Dictionary<string, string> paramsList)
        {
            long retval = 0;
            try
            {
                //lock (this)
                {

                    transport.Open();
                    try
                    {
                        retval = client.IsServerActive(paramsList);
                    }
                    finally
                    {
                        transport.Close();
                    }
                }
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (SocketException s)
            {
                Console.WriteLine(s.ToString());
            }
            return retval;
        }

        public void PostStatusMessage(Dictionary<string, string> paramsList)
        {
            try {
                //lock (this)
                {

                    transport.Open();
                    try
                    {
                        client.PostStatusMessage(paramsList);
                    }
                    finally
                    {
                        transport.Close();
                    }
                }
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }
            catch (SocketException s)
            {
                Console.WriteLine(s.ToString());
            }
        }

        public void Dispose()
        {
            //lock (this)
            {
                client.Dispose();
            }
        }
        */
    }
}
