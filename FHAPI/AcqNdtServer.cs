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
        public AcqNdtServer(string host, string port, TransportType transportType = TransportType.TCP , int dataConnectionTimeout = 2)
        {
            Host = host;
            Port = port;
            DataConnectionTimeout = dataConnectionTimeout;
            RPCP = XmlRpcProxyGen.Create<IXmlRpcProxy>();
            ((XmlRpcClientProtocol)RPCP).Url = RPCServerURL;


            FullList = RPCP.SystemListMethods().ToList();
            FullList.ForEach(x =>
            {
                if (x.StartsWith(ACKRPCNamespace))
                {
                    MethodsList.Add(x.Replace(ACKRPCNamespace, ""));
                }
                else
                {
                    MethodsList.Add(x);
                }
            });
        }
        #region PROPERTIES
        public string? Host { get; set; }
        public string? Port { get; set; }
        public string? RPCServerURL => $"http://{Host}:{Port}/RPC2";
        public TransportType TransportType { get; set; }
        public IXmlRpcProxy RPCP { get; set; }
        public string? ACKRPCNamespace { get; set; } = "acq.";
        public List<string> FullList { get; set; } = new List<string>();
        public List<string> MethodsList { get; set; } = new List<string>();
        public int DataConnectionTimeout { get; set; }
        #endregion
        #region METHODS
        public void LoadTemplate(string filename)
        {
            string data = "";
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];

            using (var fs = new FileStream(filename , FileMode.Open , FileAccess.Read))
            {
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string partial = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    data += partial;
                }
            }
        }
        #endregion
    }
}
