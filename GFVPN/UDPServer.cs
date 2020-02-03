using Android.Net;
using GFVPN.Tunnel;
using GFVPN.Utils;
using Java.Lang;
using Java.Nio.Channels;
using LruCacheNet;
using System.Collections.Concurrent;

namespace GFVPN
{ 
public class UDPServer : IRunnable
{
    private string TAG = typeof(UDPServer).getSimpleName();
private VpnService vpnService;
//private ConcurrentLinkedQueue<Packet> outputQueue;
        private ConcurrentQueue<Packet> outputQueue;
private Selector selector;

private bool isClose = false;

private const int MAX_UDP_CACHE_SIZE = 50;
private MyLRUCache<short, UDPTunnel> udpConnections = new MyLRUCache<short, UDPTunnel>(MAX_UDP_CACHE_SIZE, new MyLRUCache<short, UDPTunnel>.CleanupCallback<UDPTunnel>() {
                @Override
                public void cleanUp(UDPTunnel udpTunnel)
{
    udpTunnel.close();
}

            });


    public void start()
{
    Thread thread = new Thread(this, "UDPServer");
    thread.start();
}

public UDPServer(VpnService vpnService, ConcurrentLinkedQueue<Packet> outputQueue)
{
    try
    {
        selector = Selector.open();
    }
    catch (IOException e)
    {
        e.printStackTrace();
    }
    this.vpnService = vpnService;
    this.outputQueue = outputQueue;
    this.selector = selector;

}


public void processUDPPacket(Packet packet, short portKey, Context context)
{
    UDPTunnel udpConn = getUDPConn(portKey);
    if (udpConn == null)
    {
        udpConn = new UDPTunnel(vpnService, selector, this, packet, outputQueue, portKey, context);
        putUDPConn(portKey, udpConn);
        udpConn.initConnection();
    }
    else
    {
        udpConn.processPacket(packet);
    }
}


public void closeAllUDPConn()
{
    synchronized(udpConnections) {
        Iterator<Map.Entry<Short, UDPTunnel>> it = udpConnections.entrySet().iterator();
        while (it.hasNext())
        {
            it.next().getValue().close();
            it.remove();
        }
    }
}


public void closeUDPConn(UDPTunnel connection)
{
    synchronized(udpConnections) {
        connection.close();
        udpConnections.remove(connection.getPortKey());
    }
}

public UDPTunnel getUDPConn(short portKey)
{
    synchronized(udpConnections) {
        return udpConnections.get(portKey);
    }
}

void putUDPConn(short ipAndPort, UDPTunnel connection)
{
    synchronized(udpConnections) {
        udpConnections.put(ipAndPort, connection);
    }

}

@Override
    public void run()
{
    try
    {
        while (true)
        {
            int select = selector.select();
            if (select == 0)
            {
                Thread.sleep(5);
            }
            Iterator<SelectionKey> keyIterator = selector.selectedKeys().iterator();
            while (keyIterator.hasNext())
            {
                SelectionKey key = keyIterator.next();
                if (key.isValid())
                {
                    try
                    {
                        Object attachment = key.attachment();
                        if (attachment instanceof KeyHandler) {
                            ((KeyHandler)attachment).onKeyReady(key);
                        }

                    }
                    catch (Exception ex)
                    {
                        if (AppDebug.IS_DEBUG)
                        {
                            ex.printStackTrace(System.err);
                        }

                        DebugLog.e("TcpProxyServer iterate SelectionKey catch an exception: %s", ex);
                    }
                }
                keyIterator.remove();
            }


        }
    }
    catch (Exception e)
    {
        if (AppDebug.IS_DEBUG)
        {
            e.printStackTrace(System.err);
        }

        DebugLog.e("TcpProxyServer catch an exception: %s", e);
    }
    finally
    {
        this.stop();
        DebugLog.i("TcpServer thread exited.");
    }
}

private void stop()
{
    try
    {
        selector.close();
        selector = null;
    }
    catch (Exception e)
    {

    }
}

}
}