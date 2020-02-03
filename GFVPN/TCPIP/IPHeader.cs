/*package com.fqxd.gftools.vpn.tcpip;

import com.fqxd.gftools.vpn.utils.CommonMethods;*/

using GFVPN.Utils;

namespace GFVPN.TCPIP
{
	public class IPHeader
	{
		public const short IP = 0x0800;
		public const byte ICMP = 1;
		public const byte TCP = 6;
		public const byte UDP = 17;
		public const byte offset_proto = 9;
		public const int offset_src_ip = 12;
		public const  int offset_dest_ip = 16;
		const byte offset_ver_ihl = 0;
		const byte offset_tos = 1; 
		const short offset_tlen = 2; 
		const short offset_identification = 4;
		const short offset_flags_fo = 6; 
		const byte offset_ttl = 8; 
		const short offset_crc = 10;
		const int offset_op_pad = 20;

		public byte[] mData;
		public int mOffset;

		public IPHeader(byte[] data, int offset)
		{
			mData = data;
			mOffset = offset;
		}

		public void Default()
		{
			setHeaderLength(20);
			setTos(0);
			setTotalLength(0);
			setIdentification(0);
			setFlagsAndOffset(0);
			setTTL(64);
		}

		public int getDataLength()
		{
			return getTotalLength() - getHeaderLength();
		}

		public int getHeaderLength()
		{
			return (mData[mOffset + offset_ver_ihl] & 0x0F) * 4;
		}

		public void setHeaderLength(int value)
		{
			mData[mOffset + offset_ver_ihl] = (byte)((4 << 4) | (value / 4));
		}

		public byte getTos()
		{
			return mData[mOffset + offset_tos];
		}

		public void setTos(byte value)
		{
			mData[mOffset + offset_tos] = value;
		}

		public int getTotalLength()
		{
			return CommonMethods.readShort(mData, mOffset + offset_tlen) & 0xFFFF;
		}

		public void setTotalLength(int value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_tlen, (short)value);
		}

		public int getIdentification()
		{
			return CommonMethods.readShort(mData, mOffset + offset_identification) & 0xFFFF;
		}

		public void setIdentification(int value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_identification, (short)value);
		}

		public short getFlagsAndOffset()
		{
			return CommonMethods.readShort(mData, mOffset + offset_flags_fo);
		}

		public void setFlagsAndOffset(short value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_flags_fo, value);
		}

		public byte getTTL()
		{
			return mData[mOffset + offset_ttl];
		}

		public void setTTL(byte value)
		{
			mData[mOffset + offset_ttl] = value;
		}

		public byte getProtocol()
		{
			return mData[mOffset + offset_proto];
		}

		public void setProtocol(byte value)
		{
			mData[mOffset + offset_proto] = value;
		}

		public short getCrc()
		{
			return CommonMethods.readShort(mData, mOffset + offset_crc);
		}

		public void setCrc(short value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_crc, value);
		}

		public int getSourceIP()
		{
			return CommonMethods.readInt(mData, mOffset + offset_src_ip);
		}

		public void setSourceIP(int value)
		{
			CommonMethods.writeInt(mData, mOffset + offset_src_ip, value);
		}

		public int getDestinationIP()
		{
			return CommonMethods.readInt(mData, mOffset + offset_dest_ip);
		}

		public void setDestinationIP(int value)
		{
			CommonMethods.writeInt(mData, mOffset + offset_dest_ip, value);
		}

		public override string ToString()
		{
			return $"{CommonMethods.ipIntToString(getSourceIP())}->{CommonMethods.ipIntToString(getDestinationIP())} Pro ={getProtocol().ToString()}, HLen={getHeaderLength().ToString()}";
		}
	}
}