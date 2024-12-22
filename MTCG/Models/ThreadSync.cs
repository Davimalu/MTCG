using System.Collections.Concurrent;

namespace MTCG.Models
{
    public class ThreadSync
    {
        public static ConcurrentDictionary<string, bool> ConnectedUsers = new ConcurrentDictionary<string, bool>();
        public static Object UserLock = new Object();
        public static Object DatabaseLock = new Object();
        public static Object PackageLock = new Object();

        public static Object CardLock = new Object();
    }
}
