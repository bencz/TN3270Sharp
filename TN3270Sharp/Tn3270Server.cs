﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TN3270Sharp
{
    public class Tn3270Server
    {
        private string IpAddress { get; set; }
        private int Port { get; set; }

        public Tn3270Server(int port)
            : this("127.0.0.1", port)
        {
        }

        public Tn3270Server(string ipAddress, int port)
            : this(ipAddress, port, "IBM037")
        {

        }

        public Tn3270Server(string ipAddress, int port, string defaultEbcdicEncoding)
        {
            IpAddress = ipAddress;
            Port = port;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Ebcdic.SetEbcdicEncoding(defaultEbcdicEncoding);
        }

        public void StartListener(Func<bool> breakCondition, Action whenHasNewConnection, Action whenConnectionIsClosed, Action<ITn3270ConnectionHandler> handleConnectionAction)
        {
            var server = new TcpListener(IPAddress.Parse(IpAddress), Port);
            server.Start();
            
            while (!breakCondition())
            {
                TcpClient client = server.AcceptTcpClient();
                new Thread(() =>
                {
                    whenHasNewConnection();

                    using (Tn3270ConnectionHandler tn3270ConnectionHandler = new Tn3270ConnectionHandler(client))
                    {
                        tn3270ConnectionHandler.NegotiateTelnet();
                        handleConnectionAction(tn3270ConnectionHandler);
                    }

                    whenConnectionIsClosed();
                }).Start();
            }

            server.Stop();
        }

        
    }
}
