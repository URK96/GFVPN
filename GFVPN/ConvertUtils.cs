using System;

namespace GFVPN
{
    internal class ConvertUtils
    {
        internal static long GetCurrentTimeMillis()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            return (long)ts.TotalMilliseconds;
        }
    }
}
