using Java.Text;
using Java.Util;

namespace GFVPN.Utils
{
    public class TimeFormatUtil
    {
        private static DateFormat HHMMSSSFormat = new SimpleDateFormat("HH:mm:ss:s", Locale.Default);
        private static DateFormat formatYYMMDDHHMMSSFormat = new SimpleDateFormat("yyyy:MM:dd_HH:mm:ss:s", Locale.Default);

        public static string formatHHMMSSMM(long time)
        {
            var date = new Date(time);

            return HHMMSSSFormat.Format(date);
        }
        public static string formatYYMMDDHHMMSS(long time)
        {
            var date = new Date(time);

            return formatYYMMDDHHMMSSFormat.Format(date);
        }
    }
}