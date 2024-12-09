using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookComputing.XmlRpc;

namespace FHAPI.Interfaces
{
    internal interface IRPC : IXmlRpcProxy
    {
        [XmlRpcMethod("LoadTemplate")]
        void LoadTemplate(string data);
    }
}
