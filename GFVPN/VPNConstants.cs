using Android.OS;

namespace GFVPN
{
    internal static class VPNConstants
    {
        internal const int BUFFER_SIZE = 2560;
        internal const int MAX_PAYLOAD_SIZE = 2520;
        internal static readonly string BASE_DIR = Environment.ExternalStorageDirectory.AbsolutePath + "/GF_Tool/Conversation/";
        internal static readonly string DATA_DIR = BASE_DIR + "data/";
        internal static readonly string CONFIG_DIR = BASE_DIR + "config/";
        internal const string VPN_SP_NAME = "vpn_sp_name";
        internal const string IS_UDP_NEED_SAVE = "isUDPNeedSave";
        internal const string IS_UDP_SHOW = "isUDPShow";
        internal const string DEFAULT_PACKAGE_ID = "default_package_id";
        internal const string DEFAULT_PACAGE_NAME = "default_package_name";
    }
}