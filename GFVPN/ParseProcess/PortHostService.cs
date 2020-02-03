/*package com.fqxd.gftools.vpn.processparse;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.IBinder;
import androidx.annotation.Nullable;

import com.fqxd.gftools.vpn.VPNLog;
import com.fqxd.gftools.vpn.nat.NatSession;
import com.fqxd.gftools.vpn.nat.NatSessionManager;
import com.fqxd.gftools.vpn.utils.VpnServiceHelper;

import java.util.List;*/

using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using GFVPN.NAT;

namespace GFVPN.ParseProcess
{
    public class PortHostService : Service
    {
        private const string ACTION = "action";
        private const string TAG = "PortHostService";
        private static PortHostService instance;
        private bool isRefresh = false;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            NetFileManager.getInstance().init(ApplicationContext);

            instance = this;
        }

        public static PortHostService getInstance()
        {
            return instance;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            instance = null;
        }

        public List<NatSession> getAndRefreshSessionInfo()
        {
            List<NatSession> allSession = NatSessionManager.getAllSession();

            refreshSessionInfo(allSession);

            return allSession;
        }

        public void refreshSessionInfo()
        {
            List<NatSession> allSession = NatSessionManager.getAllSession();

            refreshSessionInfo(allSession);
        }

        private void refreshSessionInfo(List<NatSession> netConnections)
        {
            if (isRefresh || netConnections == null)
            {
                return;
            }

            bool needRefresh = false;

            foreach (var connection in netConnections)
            {
                if (connection.appInfo == null)
                {
                    needRefresh = true;
                    break;
                }
            }

            if (!needRefresh)
            {
                return;
            }

            isRefresh = true;

            try
            {
                NetFileManager.getInstance().refresh();

                foreach (var connection in netConnections)
                {
                    if (connection.appInfo == null)
                    {
                        int searchPort = connection.localPort & 0XFFFF;
                        int? uid = NetFileManager.getInstance().getUid(searchPort);

                        if (uid != null)
                        {
                            VPNLog.d(TAG, "can not find uid");
                            connection.appInfo = AppInfo.createFromUid(VpnServiceHelper.getContext(), uid);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                VPNLog.d(TAG, $"failed to refreshSessionInfo {e.Message}");

            }

            isRefresh = false;
        }


        public static void startParse(Context context)
        {
            Intent intent = new Intent(context, typeof(PortHostService));
            context.StartService(intent);
        }

        public static void stopParse(Context context)
        {
            Intent intent = new Intent(context, typeof(PortHostService));
            context.StopService(intent);
        }
    }
}