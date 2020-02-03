using Android.Text;

using GFVPN.NAT;
using GFVPN.Utils;

using System;
using System.Diagnostics;
using System.Text;

/*package com.fqxd.gftools.vpn.http;

import android.text.TextUtils;


import com.fqxd.gftools.vpn.nat.NatSession;
import com.fqxd.gftools.vpn.utils.AppDebug;
import com.fqxd.gftools.vpn.utils.CommonMethods;
import com.fqxd.gftools.vpn.utils.DebugLog;

import java.util.Locale;
import java.util.TreeSet;*/

namespace GFVPN.HTTP
{
    public class HttpRequestHeaderParser
    {

        public static void parseHttpRequestHeader(NatSession session, byte[] buffer, int offset, int count)
        {
            try
            {
                switch (buffer[offset])
                {
                    //GET
                    case (byte)'G':
                    //HEAD
                    case (byte)'H':
                    //POST, PUT
                    case (byte)'P':
                    //DELETE
                    case (byte)'D':
                    //OPTIONS
                    case (byte)'O':
                    //TRACE
                    case (byte)'T':
                    //CONNECT
                    case (byte)'C':
                        GetHttpHostAndRequestUrl(session, buffer, offset, count);
                        break;
                    //SSL
                    case 0x16:
                        session.remoteHost = GetSNI(session, buffer, offset, count);
                        session.isHttpsSession = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (AppDebug.isDebug)
                {
                    //ex.printStackTrace(System.err);
                }

                Debug.Write($"Error: parseHost: {ex}");
            }
        }

        public static void GetHttpHostAndRequestUrl(NatSession session, byte[] buffer, int offset, int count)
        {
            session.isHttp = true;
            session.isHttpsSession = false;

            string headerString = new string(Encoding.UTF8.GetChars(buffer), offset, count);
            string[] headerLines = headerString.Split("\\r\\n");
            string host = getHttpHost(headerLines);

            if (!TextUtils.IsEmpty(host))
            {
                session.remoteHost = host;
            }

            paresRequestLine(session, headerLines[0]);
        }

        public static string getRemoteHost(byte[] buffer, int offset, int count)
        {
            string headerString = new string(Encoding.UTF8.GetChars(buffer), offset, count);
            string[] headerLines = headerString.Split("\\r\\n");

            return getHttpHost(headerLines);
        }

        public static string getHttpHost(string[] headerLines)
        {
            for (int i = 1; i < headerLines.Length; i++)
            {
                string[] nameValueStrings = headerLines[i].Split(":");

                if (nameValueStrings.Length == 2 || nameValueStrings.Length == 3)
                {
                    string name = nameValueStrings[0].ToLower(System.Globalization.CultureInfo.GetCultureInfo("en-us")).Trim();
                    string value = nameValueStrings[1].Trim();

                    if ("host" == name)
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        public static void paresRequestLine(NatSession session, string requestLine)
        {
            string[] parts = requestLine.Trim().Split(" ");

            if (parts.Length == 3)
            {
                string url = parts[1];

                session.method = parts[0];
                session.pathUrl = url;

                if (url.StartsWith("/"))
                {
                    if (session.remoteHost != null)
                    {
                        session.requestUrl = $"http://{session.remoteHost}{url}";
                    }
                }
                else
                {
                    if (session.requestUrl.StartsWith("http"))
                    {
                        session.requestUrl = url;
                    }
                    else
                    {
                        session.requestUrl = $"http://{url}";
                    }

                }
            }
        }

        public static String GetSNI(NatSession session, byte[] buffer, int offset, int count)
        {
            int limit = offset + count;
            //TLS Client Hello
            if (count > 43 && buffer[offset] == 0x16)
            {
                //Skip 43 byte header
                offset += 43;

                //read sessionID
                if (offset + 1 > limit)
                {
                    return null;
                }
                int sessionIDLength = buffer[offset++] & 0xFF;
                offset += sessionIDLength;

                //read cipher suites
                if (offset + 2 > limit)
                {
                    return null;
                }

                int cipherSuitesLength = CommonMethods.readShort(buffer, offset) & 0xFFFF;
                offset += 2;
                offset += cipherSuitesLength;

                //read Compression method.
                if (offset + 1 > limit)
                {
                    return null;
                }
                int compressionMethodLength = buffer[offset++] & 0xFF;
                offset += compressionMethodLength;
                if (offset == limit)
                {
                    Debug.Write("TLS Client Hello packet doesn't contains SNI info.(offset == limit)");
                    return null;
                }

                //read Extensions
                if (offset + 2 > limit)
                {
                    return null;
                }

                int extensionsLength = CommonMethods.readShort(buffer, offset) & 0xFFFF;

                offset += 2;

                if (offset + extensionsLength > limit)
                {
                    Debug.Write("TLS Client Hello packet is incomplete.");
                    return null;
                }

                while (offset + 4 <= limit)
                {
                    int type0 = buffer[offset++] & 0xFF;
                    int type1 = buffer[offset++] & 0xFF;
                    int length = CommonMethods.readShort(buffer, offset) & 0xFFFF;

                    offset += 2;

                    //have SNI
                    if (type0 == 0x00 && type1 == 0x00 && length > 5)
                    {
                        offset += 5;
                        length -= 5;

                        if (offset + length > limit)
                        {
                            return null;
                        }

                        string serverName = new string(Encoding.UTF8.GetChars(buffer), offset, length);

                        Debug.Write("SNI: %s\n", serverName);

                        session.isHttpsSession = true;

                        return serverName;
                    }
                    else
                    {
                        offset += length;
                    }

                }

                Debug.Write("TLS Client Hello packet doesn't contains Host field info.");

                return null;
            }
            else
            {
                Debug.Write("Bad TLS Client Hello packet.");

                return null;
            }
        }


    }
}