using MTCG.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Interfaces
{
    public interface IHttpEndpoint
    {
        (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body);
    }
}
