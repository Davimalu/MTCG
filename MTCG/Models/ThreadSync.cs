using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class ThreadSync
    {
        public static Object DatabaseLock = new Object();
    }
}
