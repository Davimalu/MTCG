using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class ThreadSync
    {
        public static ConcurrentDictionary<string, bool> ConnectedUsers = new ConcurrentDictionary<string, bool>();
        public static Object UserLock = new Object();
        public static Object DatabaseLock = new Object();
    }
}
