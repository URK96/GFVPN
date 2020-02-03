using Android.Content;
using GFVPN.NAT;
using Java.Net;
using Java.Nio.Channels;

namespace GFVPN.Tunnel
{
	public class TunnelFactory
	{

		public static TcpTunnel wrap(SocketChannel channel, Selector selector)
		{
			TcpTunnel tunnel = new RawTcpTunnel(channel, selector);
			var session = NatSessionManager.getSession((short)channel.Socket().Port);

			if (session != null)
			{
				tunnel.isHttpsRequest = session.isHttpsSession;
			}

			return tunnel;
		}

		public static TcpTunnel createTunnelByConfig(InetSocketAddress destAddress, Selector selector, short portKey, Context context)
		{
			return new RemoteTcpTunnel(destAddress, selector, portKey, context);
		}
	}
}