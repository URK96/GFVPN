using Android.Util;
using Java.Lang;

namespace GFVPN
{
    public class VPNLog
    {
        public static bool isMakeDebugLog = true;

        public static void makeDebugLog(bool isMake)
        {
            isMakeDebugLog = isMake;
        }

        public static void d(string tag, string message)
        {
            if (isMakeDebugLog)
            {
                Log.Debug(tag, message);
            }
        }

        public static void v(string tag, string message)
        {
            Log.Verbose(tag, message);
        }

        public static void i(string tag, string message)
        {
            Log.Info(tag, message);
        }

        public static void w(string tag, string message)
        {
            Log.Warn(tag, message);
        }

        public static void w(string tag, string message, Throwable e)
        {
            Log.Warn(tag, message, e);
        }

        public static void e(string tag, string message)
        {
            Log.Error(tag, message);
        }
    }
}