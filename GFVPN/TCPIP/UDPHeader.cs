﻿using GFVPN.Utils;

namespace GFVPN.TCPIP
{
	public class UDPHeader
	{
		const short offset_src_port = 0;
		const short offset_dest_port = 2;
		const short offset_tlen = 4;
		const short offset_crc = 6;

		public byte[] mData;
		public int mOffset;

		public UDPHeader(byte[] data, int offset)
		{
			mData = data;
			mOffset = offset;
		}

		public short getSourcePort()
		{
			return CommonMethods.readShort(mData, mOffset + offset_src_port);
		}

		public void setSourcePort(short value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_src_port, value);
		}

		public short getDestinationPort()
		{
			return CommonMethods.readShort(mData, mOffset + offset_dest_port);
		}

		public void setDestinationPort(short value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_dest_port, value);
		}

		public int getTotalLength()
		{
			return CommonMethods.readShort(mData, mOffset + offset_tlen) & 0xFFFF;
		}

		public void setTotalLength(int value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_tlen, (short)value);
		}

		public short getCrc()
		{
			return CommonMethods.readShort(mData, mOffset + offset_crc);
		}

		public void setCrc(short value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_crc, value);
		}

		public override string ToString()
		{
			return $"{(getSourcePort() & 0xFFFF).ToString()}->{(getDestinationPort() & 0xFFFF).ToString()}";
		}
	}
}