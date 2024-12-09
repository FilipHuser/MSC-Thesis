using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CookComputing.XmlRpc;

namespace FHAPI
{
    public enum TransportType 
    {
        TCP,
        UDP
    }
    internal class AcqNdtServer
    {
        public AcqNdtServer(string host, string port, TransportType transportType = TransportType.TCP)
        {
            Host = host;
            Port = port;
            RPCP = XmlRpcProxyGen.Create<IXmlRpcProxy>();
            ((XmlRpcClientProtocol)RPCP).Url = RPCServerURL;
        }
        public string? Host { get; set; }
        public string? Port { get; set; }
        public string? RPCServerURL => $"http://{Host}:{Port}/RPC2";
        public TransportType TransportType { get; set; }
        public IXmlRpcProxy RPCP { get; set; }
    }
}
