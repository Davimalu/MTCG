using MTCG.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Interfaces
{
    public interface IHttpEndpoint
    {
        (int, string?) HandleRequest(HTTPHeader headers, string? body);
    }
}
