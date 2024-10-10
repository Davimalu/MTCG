using MTCG.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Interfaces
{
    public interface IHttpEndpoint
    {
        (int, string?) HandleRequest(string method, string body, AuthService AuthService);
    }
}
