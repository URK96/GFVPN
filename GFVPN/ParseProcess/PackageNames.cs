/*package com.fqxd.gftools.vpn.processparse;

import android.os.Parcel;
import android.os.Parcelable;
import android.text.TextUtils;

import java.io.Serializable;
*/

using Android.OS;
using Android.Runtime;

using Java.IO;

using System.Text;

namespace GFVPN.ParseProcess
{
    public class PackageNames : Java.Lang.Object, IParcelable, ISerializable
    {
        public readonly string[] pkgs;

        public static PackageNames newInstance(string[] pkgs)
        {
            return new PackageNames(pkgs);
        }

        public static PackageNames newInstanceFromCommaList(string pkgList)
        {
            return newInstance(pkgList.Split(","));
        }

        public string getAt(int i)
        {
            if (pkgs.Length > i)
            {
                return pkgs[i];
            }

            return null;
        }

        public string getCommaJoinedString()
        {
            var sb = new StringBuilder();

            return sb.AppendJoin(',', pkgs).ToString();
        }

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteInt(pkgs.Length);
            dest.WriteStringArray(pkgs);
        }

        internal PackageNames(string[] pkgs)
        {
            this.pkgs = pkgs;
        }

        internal PackageNames(Parcel parcel)
        {
            pkgs = new string[parcel.ReadInt()];
            parcel.ReadStringArray(pkgs);
        }
    }

    public class Creator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel parcel)
        {
            return new PackageNames(parcel);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new PackageNames[size];
        }
    }
}