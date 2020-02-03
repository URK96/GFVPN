/*package com.fqxd.gftools.vpn.processparse;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.graphics.drawable.Drawable;
import android.text.TextUtils;
import android.util.Log;
import android.util.LruCache;

import com.fqxd.gftools.R;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Support.V4.Util;
using Android.Util;
using Java.IO;
using Java.Lang;
using LruCacheNet;

namespace GFVPN.ParseProcess
{
    public class AppInfo : Java.Lang.Object, ISerializable
    {
        private static Drawable defaultIcon = null;
        private static LruCache<string, IconInfo> iconCache = new LruCache<string, IconInfo>(50);
        public readonly string allAppName;
        public readonly string leaderAppName;
        public readonly PackageNames pkgs;

        public class Entry
        {
            public string AppName { get; private set; }
            public string PkgName { get; private set; }

            public Entry(string appName, string pkgName)
            {
                AppName = appName;
                PkgName = pkgName;
            }
        }

        public class IconInfo
        {
            public long Date { get; private set; }
            public Drawable Icon { get; private set; }

            public IconInfo(long date, Drawable icon)
            {
                Date = date;
                Icon = icon;
            }
        }

        private AppInfo(string leaderAppName, string allAppName, string[] pkgs)
        {
            this.leaderAppName = leaderAppName;
            this.allAppName = allAppName;
            this.pkgs = PackageNames.newInstance(pkgs);
        }

        public static AppInfo createFromUid(Context context, int uid)
        {
            var pm = context.PackageManager;
            var list = new List<Entry>();

            if (uid > 0)
            {
                try
                {
                    string[] pkgNames = pm.GetPackagesForUid(uid);
                    if (pkgNames == null || pkgNames.Length <= 0)
                    {
                        list.Add(new Entry("System", "nonpkg.noname"));
                    }
                    else
                    {
                        foreach (var pkgName in pkgNames)
                        {
                            if (pkgName != null)
                            {
                                try
                                {
                                    var appPackageInfo = pm.GetPackageInfo(pkgName, 0);
                                    string appName = null;

                                    if (appPackageInfo != null)
                                    {
                                        appName = appPackageInfo.ApplicationInfo.LoadLabel(pm).ToString();
                                    }

                                    if (string.IsNullOrEmpty(appName))
                                    {
                                        appName = pkgName;
                                    }

                                    list.Add(new Entry(appName, pkgName));
                                }
                                catch (PackageManager.NameNotFoundException)
                                {
                                    list.Add(new Entry(pkgName, pkgName));
                                }
                            }
                        }
                    }
                }
                catch (RuntimeException)
                {
                    Log.Info("NRFW", "error getPackagesForUid(). package manager has died");
                    return null;
                }
            }

            if (list.Count == 0)
            {
                list.Add(new Entry("System", "root.uid=0"));
            }

            list.Sort();

            /*Collections.sort(list, new Comparator<Entry>()
            {
                    public int compare(Entry lhs, Entry rhs)
            {
                int ret = lhs.appName.compareToIgnoreCase(rhs.appName);
                if (ret == 0)
                {
                    return lhs.pkgName.compareToIgnoreCase(rhs.pkgName);
                }
                return ret;
            }
        });*/
            var pkgs = new string[list.Count];
            var apps = new string[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                pkgs[i] = list[i].PkgName;
                apps[i] = list[i].AppName;
            }

            var sb = new System.Text.StringBuilder();

            return new AppInfo(apps[0], sb.AppendJoin(',', apps).ToString(), pkgs);
        }

        public static Drawable getIcon(Context ctx, string pkgName)
        {
            return getIcon(ctx, pkgName, false);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Drawable getIcon(Context context, string pkgName, bool onlyPeek)
        {
            Drawable drawable = null;

            lock (typeof(AppInfo))
            {
                IconInfo iconInfo;

                if (defaultIcon == null)
                {
                    defaultIcon = context.Resources.GetDrawable(Resource.Drawable.gf_icon);
                }

                var pm = context.PackageManager;
                PackageInfo appPackageInfo = null;

                try
                {
                    long lastUpdate = appPackageInfo.LastUpdateTime;

                    appPackageInfo = pm.GetPackageInfo(pkgName, 0);
                    iconInfo = (IconInfo)iconCache.Get(pkgName);

                    if ((iconInfo != null) && (iconInfo.Date == lastUpdate) && (iconInfo.Icon != null))
                    {
                        drawable = iconInfo.Icon;
                    }
                }
                catch (PackageManager.NameNotFoundException)
                {

                }

                if (appPackageInfo != null)
                {
                    if (!onlyPeek)
                    {
                        drawable = appPackageInfo.ApplicationInfo.LoadIcon(pm);
                        iconInfo = new IconInfo(appPackageInfo.LastUpdateTime, drawable);

                        iconCache.Add(pkgName, iconInfo);
                    }
                }
                else
                {
                    iconCache.Remove(pkgName);

                    drawable = defaultIcon;
                }
            }
            return drawable;
        }
    }
}