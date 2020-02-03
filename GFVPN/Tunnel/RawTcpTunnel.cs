using Java.Net;
using Java.Nio;
using Java.Nio.Channels;

namespace GFVPN.Tunnel
{
	public class RawTcpTunnel : TcpTunnel
	{

		public RawTcpTunnel(SocketChannel innerChannel, Selector selector) : base(innerChannel, selector)
		{
			
		}

		public RawTcpTunnel(InetSocketAddress serverAddress, Selector selector, short portKey) : base(serverAddress, selector, portKey)
		{
			
		}

		protected override void onConnected()
		{
			onTunnelEstablished();
		}

		protected override bool isTunnelEstablished()
		{
			return true;
		}

		protected override void beforeSend(ByteBuffer buffer)
		{
		}

		protected override void afterReceived(ByteBuffer buffer)
		{

		}

		protected override void onDispose()
		{

		}
	}
}