/*package com.fqxd.gftools.vpn.utils;


import android.util.Log;

import com.fqxd.gftools.vpn.tcpip.IPHeader;
import com.fqxd.gftools.vpn.tcpip.TCPHeader;
import com.fqxd.gftools.vpn.tcpip.UDPHeader;

import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.UnknownHostException;*/

using GFVPN.TCPIP;
using Java.Net;

namespace GFVPN.Utils
{
	public class CommonMethods
	{

		private const string TAG = "CommonMethods";

		public static InetAddress ipIntToInet4Address(int ip)
		{
			byte[] ipAddress = new byte[4];
			writeInt(ipAddress, 0, ip);
			try
			{
				return Inet4Address.GetByAddress(ipAddress);
			}
			catch (UnknownHostException)
			{
				return null;
			}
		}

		public static string ipIntToString(int ip)
		{
			return $"{((ip >> 24) & 0x00FF).ToString()}.{((ip >> 16) & 0x00FF).ToString()}.{((ip >> 8) & 0x00FF).ToString()}.{(ip & 0x00FF).ToString()}";
		}

		public static string ipBytesToString(byte[] ip)
		{
			return $"{(ip[0] & 0x00FF).ToString()}.{(ip[1] & 0x00FF).ToString()}.{(ip[2] & 0x00FF).ToString()}.{(ip[3] & 0x00FF).ToString()}";
		}

		public static int ipStringToInt(string ip)
		{
			string[] arrayStrings = ip.Split("\\.");

			int r = (int.Parse(arrayStrings[0]) << 24)
					| (int.Parse(arrayStrings[1]) << 16)
					| (int.Parse(arrayStrings[2]) << 8)
					| (int.Parse(arrayStrings[3]));

			return r;
		}

		public static int readInt(byte[] data, int offset)
		{
			int r = ((data[offset] & 0xFF) << 24)
					| ((data[offset + 1] & 0xFF) << 16)
					| ((data[offset + 2] & 0xFF) << 8)
					| (data[offset + 3] & 0xFF);

			return r;
		}

		public static short readShort(byte[] data, int offset)
		{
			int r = ((data[offset] & 0xFF) << 8) | (data[offset + 1] & 0xFF);

			return (short)r;
		}

		public static void writeInt(byte[] data, int offset, int value)
		{
			data[offset] = (byte)(value >> 24);
			data[offset + 1] = (byte)(value >> 16);
			data[offset + 2] = (byte)(value >> 8);
			data[offset + 3] = (byte)value;
		}

		public static void writeShort(byte[] data, int offset, short value)
		{
			data[offset] = (byte)(value >> 8);
			data[offset + 1] = (byte)(value);
		}

		public static short checksum(long sum, byte[] buf, int offset, int len)
		{
			sum += getsum(buf, offset, len);
			while ((sum >> 16) > 0)
			{
				sum = (sum & 0xFFFF) + (sum >> 16);
			}
			return (short)~sum;
		}

		public static long getsum(byte[] buf, int offset, int len)
		{
			long sum = 0;

			while (len > 1)
			{
				sum += readShort(buf, offset) & 0xFFFF;
				offset += 2;
				len -= 2;
			}

			if (len > 0)
			{
				sum += (buf[offset] & 0xFF) << 8;
			}

			return sum;
		}

		public static bool ComputeIPChecksum(IPHeader ipHeader)
		{
			short oldCrc = ipHeader.getCrc();

			ipHeader.setCrc(0);

			short newCrc = checksum(0, ipHeader.mData, ipHeader.mOffset, ipHeader.getHeaderLength());

			ipHeader.setCrc(newCrc);

			return oldCrc == newCrc;
		}

		public static bool ComputeTCPChecksum(IPHeader ipHeader, TCPHeader tcpHeader)
		{
			ComputeIPChecksum(ipHeader);

			int ipData_len = ipHeader.getDataLength();

			if (ipData_len < 0)
			{
				return false;
			}

			long sum = getsum(ipHeader.mData, ipHeader.mOffset + IPHeader.offset_src_ip, 8);

			sum += ipHeader.getProtocol() & 0xFF;
			sum += ipData_len;

			short oldCrc = tcpHeader.getCrc();

			tcpHeader.setCrc(0);

			short newCrc = checksum(sum, tcpHeader.mData, tcpHeader.mOffset, ipData_len);

			tcpHeader.setCrc(newCrc);

			return oldCrc == newCrc;
		}

		public static bool ComputeUDPChecksum(IPHeader ipHeader, UDPHeader udpHeader)
		{
			ComputeIPChecksum(ipHeader);

			int ipData_len = ipHeader.getDataLength();

			if (ipData_len < 0)
			{
				return false;
			}

			long sum = getsum(ipHeader.mData, ipHeader.mOffset + IPHeader.offset_src_ip, 8);

			sum += ipHeader.getProtocol() & 0xFF;
			sum += ipData_len;

			short oldCrc = udpHeader.getCrc();

			udpHeader.setCrc(0);

			short newCrc = checksum(sum, udpHeader.mData, udpHeader.mOffset, ipData_len);

			udpHeader.setCrc(newCrc);

			return oldCrc == newCrc;
		}
	}
}