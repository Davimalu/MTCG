using MTCG.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Interfaces.HTTP
{
    public interface IHttpEndpoint
    {
        (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body);
    }
}
