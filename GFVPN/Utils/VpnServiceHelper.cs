using Android.App;
using Android.Content;
using GFVPN.NAT;
using GFVPN.ParseProcess;
using GFVPN.Service;
using Java.IO;
using Java.Net;
using System;
using System.Collections.Generic;

namespace GFVPN.Utils
{
    public class VpnServiceHelper
    {
        static Context context;
        public const int START_VPN_SERVICE_REQUEST_CODE = 2015;
        private static FirewallVpnService sVpnService;
        private static ISharedPreferences sp;

        public static void onVpnServiceCreated(FirewallVpnService vpnService)
        {
            sVpnService = vpnService;

            if (context == null)
            {
                context = vpnService.ApplicationContext;
            }
        }

        public static void onVpnServiceDestroy()
        {
            sVpnService = null;
        }

        public static Context getContext()
        {
            return context;
        }

        public static bool isUDPDataNeedSave()
        {

            sp = context.GetSharedPreferences(VPNConstants.VPN_SP_NAME, FileCreationMode.Private);

            return sp.GetBoolean(VPNConstants.IS_UDP_NEED_SAVE, false);
        }

        public static bool protect(Socket socket)
        {
            if (sVpnService != null)
            {
                return sVpnService.Protect(socket);
            }

            return false;
        }

        public static bool protect(DatagramSocket socket)
        {
            if (sVpnService != null)
            {
                return sVpnService.Protect(socket);
            }

            return false;
        }

        public static bool vpnRunningStatus()
        {
            if (sVpnService != null)
            {
                return sVpnService.vpnRunningStatus();
            }

            return false;
        }

        public static void changeVpnRunningStatus(Context context, bool isStart)
        {
            if (context == null)
            {
                return;
            }

            if (isStart)
            {
                var intent = FirewallVpnService.prepare(context);

                if (intent == null)
                {
                    startVpnService(context);
                }
                else
                {
                    if (context is Activity)
                    {
                        (context as Activity).StartActivityForResult(intent, START_VPN_SERVICE_REQUEST_CODE);
                    }
                }
            }
            else if (sVpnService != null)
            {
                bool stopStatus = false;

                sVpnService.setVpnRunningStatus(stopStatus);
            }
        }
        public static List<NatSession> getAllSession()
        {
            if (FirewallVpnService.lastVpnStartTimeFormat == null)
            {
                return null;
            }

            try
            {
                var file = new File(VPNConstants.CONFIG_DIR + FirewallVpnService.lastVpnStartTimeFormat);
                ACache aCache = ACache.get(file);
                string[] list = file.List();
                var baseNetSessions = new List<NatSession>();

                if (list != null)
                {
                    foreach (var fileName in list)
                    {
                        baseNetSessions.Add(aCache.getAsObject(fileName) as NatSession);
                    }
                }

                var portHostService = PortHostService.getInstance();

                if (portHostService != null)
                {
                    var aliveConnInfo = portHostService.getAndRefreshSessionInfo();

                    if (aliveConnInfo != null)
                    {
                        baseNetSessions.AddRange(aliveConnInfo);
                    }
                }

                baseNetSessions.Sort(new NatSession.NatSesionComparator());

                return baseNetSessions;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public static void startVpnService(Context context)
        {
            if (context == null)
            {
                return;
            }

            context.StartService(new Intent(context, typeof(FirewallVpnService)));
        }
    }
}