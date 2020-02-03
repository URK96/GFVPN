/*package com.fqxd.gftools.vpn.processparse;

import android.content.Context;
import android.os.Handler;
import android.os.SystemClock;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.util.Arrays;
import java.util.Map;
import java.util.Scanner;
import java.util.concurrent.ConcurrentHashMap;*/

using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Java.IO;
using System.Text;
using Java.Lang;
using Java.Util;
using StringBuilder = System.Text.StringBuilder;

namespace GFVPN.ParseProcess
{
    public class NetFileManager
    {
        public const int TYPE_TCP = 0;
        public const int TYPE_TCP6 = 1;
        public const int TYPE_UDP = 2;
        public const int TYPE_UDP6 = 3;
        public const int TYPE_RAW = 4;
        public const int TYPE_RAW6 = 5;
        public const int TYPE_MAX = 6;

        public const int MSG_NET_CALLBACK = 1;
        private const string TAG = "NetFileManager";

        private Context context;
        private Handler handler;
        private readonly static int DATA_LOCAL = 2;
        private readonly static int DATA_REMOTE = 3;
        private readonly static int DATA_UID = 8;
        //Map<Integer, Integer> processHost = new ConcurrentHashMap<>();
        Dictionary<int, int> processHost = new Dictionary<int, int>();
        private File[] file;
        private long[] lastTime;
        private StringBuilder sbBuilder = new StringBuilder();

        public void init(Context context)
        {
            this.context = context;
            const string PATH_TCP = "/proc/net/tcp";
            const string PATH_TCP6 = "/proc/net/tcp6";
            const string PATH_UDP = "/proc/net/udp";
            const string PATH_UDP6 = "/proc/net/udp6";
            const string PATH_RAW = "/proc/net/raw";
            const string PATH_RAW6 = "/proc/net/raw6";

            file = new File[TYPE_MAX];
            file[0] = new File(PATH_TCP);
            file[1] = new File(PATH_TCP6);
            file[2] = new File(PATH_UDP);
            file[3] = new File(PATH_UDP6);
            file[4] = new File(PATH_RAW);
            file[5] = new File(PATH_RAW6);

            lastTime = new long[TYPE_MAX];
        }

        internal static class InnerClass
        {
            internal static NetFileManager instance = new NetFileManager();
        }
        public static NetFileManager getInstance()
        {
            return InnerClass.instance;
        }

        public void execute(string[] cmmand, string directory, int type)
        {
            NetInfo netInfo = null;
            string sTmp = null;
            var builder = new ProcessBuilder(cmmand);

            if (directory != null)
            {
                builder.Directory(new File(directory));
            }

            builder.RedirectErrorStream(true);
            var process = builder.Start();

            var s = new Scanner(process.InputStream);

            s.UseDelimiter("\n");

            while (s.HasNextLine)
            {
                sTmp = s.NextLine();
                netInfo = parseDataNew(Java.Lang.String.(sTmp));

                if (netInfo != null)
                {
                    netInfo.type = type;
                    saveToMap(netInfo);
                }
            }
        }

        private int strToInt(string value, int iHex, int iDefault)
        {
            int iValue = iDefault;
            if (value == null)
            {
                return iValue;
            }

            try
            {
                iValue = Integer.ParseInt(value, iHex);
                iValue = int.Parse(value, System.Globalization.NumberStyles.);
            }
            catch (NumberFormatException e)
            {
                e.PrintStackTrace();
            }

            return iValue;
        }

        private long strToLong(string value, int iHex, int iDefault)
        {
            long iValue = iDefault;
            if (value == null)
            {
                return iValue;
            }

            try
            {
                iValue = Long.ParseLong(value, iHex);
            }
            catch (NumberFormatException e)
            {
                e.PrintStackTrace();
            }

            return iValue;
        }

        private NetInfo parseDataNew(string sData)
        {
            string[] sSplitItem = sData.Split("\\s+");
            string sTmp = null;

            if (sSplitItem.Length < 9)
            {
                return null;
            }

            var netInfo = new NetInfo();

            sTmp = sSplitItem[DATA_LOCAL];
            string[] sSourceItem = sTmp.Split(":");

            if (sSourceItem.Length < 2)
            {
                return null;
            }

            netInfo.sourPort = strToInt(sSourceItem[1], 16, 0);

            sTmp = sSplitItem[DATA_REMOTE];
            string[] sDesItem = sTmp.Split(":");

            if (sDesItem.Length < 2)
            {
                return null;
            }

            netInfo.port = strToInt(sDesItem[1], 16, 0);

            sTmp = sDesItem[0];
            int len = sTmp.Length;

            if (len < 8)
            {
                return null;
            }

            sTmp = sTmp.Substring(len - 8);
            netInfo.ip = strToLong(sTmp, 16, 0);

            sbBuilder.Clear();
            sbBuilder.Append(strToInt(sTmp.Substring(6, 8), 16, 0))
                    .Append(".")
                    .Append(strToInt(sTmp.Substring(4, 6), 16, 0))
                    .Append(".")
                    .Append(strToInt(sTmp.Substring(2, 4), 16, 0))
                    .Append(".")
                    .Append(strToInt(sTmp.Substring(0, 2), 16, 0));

            sTmp = sbBuilder.ToString();
            netInfo.address = sTmp;

            if (sTmp == "0.0.0.0")
            {
                return null;
            }

            sTmp = sSplitItem[DATA_UID];
            netInfo.uid = strToInt(sTmp, 10, 0);

            return netInfo;
        }

        private void saveToMap(NetInfo netInfo)
        {
            if (netInfo == null)
            {
                return;
            }
            //   VPNLog.d(TAG, "saveToMap  port " + netInfo.getSourPort() + " uid " + netInfo.getUid());

            processHost.Add(netInfo.sourPort, netInfo.uid);

        }

        public void read(int type)
        {
            try
            {
                switch (type)
                {
                    case TYPE_TCP:
                        string[] ARGS = { "cat", "/proc/net/tcp" };
                        execute(ARGS, "/", TYPE_TCP);

                        break;
                    case TYPE_TCP6:
                        string[] ARGS1 = { "cat", "/proc/net/tcp6" };
                        execute(ARGS1, "/", TYPE_TCP6);
                        break;
                    case TYPE_UDP:
                        string[] ARGS2 = { "cat", "/proc/net/udp" };
                        execute(ARGS2, "/", TYPE_UDP);
                        break;
                    case TYPE_UDP6:
                        string[] ARGS3 = { "cat", "/proc/net/udp6" };
                        execute(ARGS3, "/", TYPE_UDP6);
                        break;
                    case TYPE_RAW:
                        string[] ARGS4 = { "cat", "/proc/net/raw" };
                        execute(ARGS4, "/", TYPE_UDP);
                        break;
                    case TYPE_RAW6:
                        string[] ARGS5 = { "cat", "/proc/net/raw6" };
                        execute(ARGS5, "/", TYPE_UDP6);
                        break;
                    default:
                        break;
                }
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }


        public void refresh()
        {
            long start = SystemClock.CurrentThreadTimeMillis();

            for (int i = 0; i < TYPE_MAX; i++)
            {
                long iTime = file[i].LastModified();

                if (iTime != lastTime[i])
                {
                    read(i);
                    lastTime[i] = iTime;
                }
            }
        }

        public int? getUid(int port)
        {
            try
            {
                int uid = processHost[port];
                //   VPNLog.i(TAG, "getUid : port is   " + port + "   uid is " + uid);
                return uid;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}