using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Tools
{
    public static class DLM
    {
        //private static Dictionary<object, LockClass> _locks = new Dictionary<object, LockClass>();
        private static Dictionary<int, int> _lockHashcodes = new Dictionary<int, int>();    //Key = hashcode of name of object, Value = thread id
        private static object _lock = new object();

        public static void TryLock<T>(ref T obj, int threadId = -1)
        {
            if (threadId == -1)
                threadId = Thread.CurrentThread.ManagedThreadId;

            //string name = $"{GetCaller()}.{nameof(obj)}";
            int hashcode = nameof(obj).GetHashCode() + GetCaller().GetHashCode();
            bool hasObject = false;

            lock (_lock)
            {
                if (!_lockHashcodes.ContainsKey(hashcode))
                    _lockHashcodes.Add(hashcode, threadId);
                else
                    hasObject = true;
            }

            if (!hasObject)
                return;

            while (_lockHashcodes.ContainsKey(hashcode))
                Thread.Sleep(1);
        }

        public static void RemoveLock<T>(ref T obj, int threadId = -1)
        {
            if (threadId == -1)
                threadId = Thread.CurrentThread.ManagedThreadId;

            //string name = $"{GetCaller()}.{nameof(obj)}";
            int hashcode = nameof(obj).GetHashCode() + GetCaller().GetHashCode();

            lock (_lock)
            {
                if (_lockHashcodes.ContainsKey(hashcode) &&
                    _lockHashcodes.ContainsValue(threadId))
                        _lockHashcodes.Remove(hashcode, out threadId);
            }
        }

        private static string GetCaller()
        {
            var trace = new StackTrace(2);
            var frame = trace.GetFrame(0);
            var caller = frame.GetMethod();
            var callingClass = caller.DeclaringType.Name;
            return callingClass;
        }

        public static int GetCurrentID<T>()
        { 
            return 0;
        }
    }

    internal class LockClass
    {
        public object Locker { get; set; }
        public bool IsInUse { get; set; }
    }
}
