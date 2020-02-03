using GFVPN.Utils;

namespace GFVPN.TCPIP
{
	public class TCPHeader
	{
		public const int FIN = 1;
		public const int SYN = 2;
		public const int RST = 4;
		public const int PSH = 8;
		public const int ACK = 16;
		public const int URG = 32;

		const short offset_src_port = 0;
		const short offset_dest_port = 2;
		const int offset_seq = 4;
		const int offset_ack = 8;
		const byte offset_lenres = 12;
		const byte offset_flag = 13;
		const short offset_win = 14; 
		const short offset_crc = 16;
		const short offset_urp = 18;

		public byte[] mData;
		public int mOffset;

		public TCPHeader(byte[] data, int offset)
		{
			mData = data;
			mOffset = offset;
		}

		public int getHeaderLength()
		{
			int lenres = mData[mOffset + offset_lenres] & 0xFF;

			return (lenres >> 4) * 4;
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

		public byte getFlag()
		{
			return mData[mOffset + offset_flag];
		}

		public short getCrc()
		{
			return CommonMethods.readShort(mData, mOffset + offset_crc);
		}

		public void setCrc(short value)
		{
			CommonMethods.writeShort(mData, mOffset + offset_crc, value);
		}

		public int getSeqID()
		{
			return CommonMethods.readInt(mData, mOffset + offset_seq);
		}

		public int getAckID()
		{
			return CommonMethods.readInt(mData, mOffset + offset_ack);
		}

		public override string ToString()
		{
			return string.Format("%s%s%s%s%s%s %d->%d %s:%s",
					(getFlag() & SYN) == SYN ? "SYN" : "",
					(getFlag() & ACK) == ACK ? "ACK" : "",
					(getFlag() & PSH) == PSH ? "PSH" : "",
					(getFlag() & RST) == RST ? "RST" : "",
					(getFlag() & FIN) == FIN ? "FIN" : "",
					(getFlag() & URG) == URG ? "URG" : "",
					getSourcePort() & 0xFFFF,
					getDestinationPort() & 0xFFFF,
					getSeqID(),
					getAckID());
		}
	}
}