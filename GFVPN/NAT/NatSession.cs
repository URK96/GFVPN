using Java.IO;
using System;
using Java;
using GFVPN;
using GFVPN.ParseProcess;
using System.Collections.Generic;

/*package com.fqxd.gftools.vpn.nat;


import com.fqxd.gftools.vpn.processparse.AppInfo;
import com.fqxd.gftools.vpn.utils.CommonMethods;

import java.io.Serializable;*/

namespace GFVPN.NAT
{ 
    public class NatSession : Java.Lang.Object, ISerializable
    {
        public static readonly string TCP = "TCP";
        public static readonly string UDP = "UPD";
        public string type;
        public string ipAndPort;
        public int remoteIP;
        public short remotePort;
        public string remoteHost;
        public short localPort;
        public int bytesSent;
        public int packetSent;
        public long receiveByteNum;
        public long receivePacketNum;
        public long lastRefreshTime;
        public bool isHttpsSession;
        public string requestUrl;
        public string pathUrl;
        public string method;
        public AppInfo appInfo;
        public long connectionStartTime = ConvertUtils.GetCurrentTimeMillis();
        public long vpnStartTime;
        public bool isHttp;

        public override string ToString()
        {
            return string.Format("%s/%s:%d packet: %d", remoteHost, CommonMethods.ipIntToString(remoteIP), remotePort & 0xFFFF, packetSent);
        }

        public string getUniqueName()
        {
            string uinID = ipAndPort + connectionStartTime;

            return uinID.GetHashCode().ToString();
        }

    public void refreshIpAndPort()
    {
        int remoteIPStr1 = (int)(remoteIP & 0XFF000000) >> 24 & 0XFF;
        int remoteIPStr2 = (remoteIP & 0X00FF0000) >> 16;
        int remoteIPStr3 = (remoteIP & 0X0000FF00) >> 8;
        int remoteIPStr4 = remoteIP & 0X000000FF;
        string remoteIPStr = "" + remoteIPStr1 + ":" + remoteIPStr2 + ":" + remoteIPStr3 + ":" + remoteIPStr4;

        ipAndPort = type + ":" + remoteIPStr + ":" + remotePort + " " + ((int)localPort & 0XFFFF);
    }

    public string getType()
    {
        return type;
    }

    public string getIpAndPort()
    {
        return ipAndPort;
    }

    public int getRemoteIP()
    {
        return remoteIP;
    }

    public short getRemotePort()
    {
        return remotePort;
    }

    public string getRemoteHost()
    {
        return remoteHost;
    }

    public short getLocalPort()
    {
        return localPort;
    }

    public int getBytesSent()
    {
        return bytesSent;
    }

    public int getPacketSent()
    {
        return packetSent;
    }

    public long getReceiveByteNum()
    {
        return receiveByteNum;
    }

    public long getReceivePacketNum()
    {
        return receivePacketNum;
    }

    public long getRefreshTime()
    {
        return lastRefreshTime;
    }

    public bool IsHttpsSession()
    {
        return isHttpsSession;
    }

    public string getRequestUrl()
    {
        return requestUrl;
    }

    public string getPathUrl()
    {
        return pathUrl;
    }

    public string getMethod()
    {
        return method;
    }

    public AppInfo getAppInfo()
    {
        return appInfo;
    }

    public long getConnectionStartTime()
    {
        return connectionStartTime;
    }

    public long getVpnStartTime()
    {
        return vpnStartTime;
    }

        public class NatSesionComparator : IComparer<NatSession>
        {
            public int Compare(NatSession x, NatSession y)
            {
                return x == y ? 0 : y.lastRefreshTime.CompareTo(x.lastRefreshTime);
            }
        }
    }
}