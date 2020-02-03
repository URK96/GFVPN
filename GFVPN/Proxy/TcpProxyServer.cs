/*package com.fqxd.gftools.vpn.proxy;


import android.content.Context;

import com.fqxd.gftools.vpn.KeyHandler;
import com.fqxd.gftools.vpn.VPNLog;
import com.fqxd.gftools.vpn.nat.NatSession;
import com.fqxd.gftools.vpn.nat.NatSessionManager;
import com.fqxd.gftools.vpn.tunnel.TcpTunnel;
import com.fqxd.gftools.vpn.tunnel.TunnelFactory;
import com.fqxd.gftools.vpn.utils.AppDebug;
import com.fqxd.gftools.vpn.utils.DebugLog;

import java.io.IOException;
import java.net.InetSocketAddress;
import java.nio.channels.SelectionKey;
import java.nio.channels.Selector;
import java.nio.channels.ServerSocketChannel;
import java.nio.channels.SocketChannel;
import java.util.Iterator;
import java.util.Set;*/

using Android.Content;
using GFVPN.NAT;
using GFVPN.Tunnel;
using GFVPN.Utils;
using Java.Lang;
using Java.Net;
using Java.Nio.Channels;
using System;
using System.Diagnostics;
using Exception = System.Exception;

namespace GFVPN.Proxy
{
    public class TcpProxyServer : Java.Lang.Object, IRunnable
    {
        private const string TAG = "TcpProxyServer";
        public bool Stopped;
        public short port;

        Selector mSelector;
        ServerSocketChannel mServerSocketChannel;
        Thread mServerThread;
        Context context;

        public TcpProxyServer(int port, Context context)
        {
            mSelector = Selector.Open();

            mServerSocketChannel = ServerSocketChannel.Open();
            mServerSocketChannel.ConfigureBlocking(false);
            mServerSocketChannel.Socket().Bind(new InetSocketAddress(port));
            mServerSocketChannel.Register(mSelector, Operations.Accept);
            this.port = (short)mServerSocketChannel.Socket().LocalPort;
            this.context = context;

            Debug.Write($"AsyncTcpServer listen on {mServerSocketChannel.Socket().InetAddress.ToString()}:{this.port & 0xFFFF} success.\n");
        }

        public void start()
        {
            mServerThread = new Thread(this, "TcpProxyServerThread");
            mServerThread.Start();
        }

        public void stop()
        {
            Stopped = true;

            if (mSelector != null)
            {
                try
                {
                    mSelector.Close();
                    mSelector = null;
                }
                catch (Java.IO.IOException ex)
                {
                    Debug.Fail($"TcpProxyServer mSelector.close() catch an exception: {ex}");
                }
            }

            if (mServerSocketChannel != null)
            {
                try
                {
                    mServerSocketChannel.Close();
                    mServerSocketChannel = null;
                }
                catch (Java.IO.IOException ex)
                {
                    if (AppDebug.isDebug)
                    {
                        ex.PrintStackTrace();
                    }

                    Debug.Fail($"TcpProxyServer mServerSocketChannel.close() catch an exception: {ex}");
                }
            }
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    int select = mSelector.Select();

                    if (select == 0)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    var selectionKeys = mSelector.SelectedKeys();

                    if (selectionKeys == null)
                    {
                        continue;
                    }

                    foreach (var key in selectionKeys)
                    {
                        if (key.IsValid)
                        {
                            try
                            {
                                if (key.IsAcceptable)
                                {
                                    VPNLog.d(TAG, "isAcceptable");
                                    onAccepted(key, context);
                                }
                                else
                                {
                                    var attachment = key.Attachment();

                                    (attachment as IKeyHandler).onKeyReady(key);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.Fail($"udp iterate SelectionKey catch an exception: {ex}");
                            }
                        }

                        selectionKeys.Remove(key);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Fail($"updServer catch an exception: {ex}");
            }
            finally
            {
                stop();
                Debug.WriteLine("udpServer thread exited.");
            }
        }

        InetSocketAddress getDestAddress(SocketChannel localChannel)
        {
            short portKey = (short)localChannel.Socket().Port;
            var session = NatSessionManager.getSession(portKey);

            if (session != null)
            {
                return new InetSocketAddress(localChannel.Socket().InetAddress, session.remotePort & 0xFFFF);
            }

            return null;
        }

        void onAccepted(SelectionKey key, Context context)
        {
            TcpTunnel localTunnel = null;

            try
            {
                var localChannel = mServerSocketChannel.Accept();

                localTunnel = TunnelFactory.wrap(localChannel, mSelector);

                short portKey = (short)localChannel.Socket().Port;
                var destAddress = getDestAddress(localChannel);

                if (destAddress != null)
                {
                    TcpTunnel remoteTunnel = TunnelFactory.createTunnelByConfig(destAddress, mSelector, portKey, context);

                    remoteTunnel.isHttpsRequest = localTunnel.isHttpsRequest;
                    remoteTunnel.setBrotherTunnel(localTunnel);
                    localTunnel.setBrotherTunnel(remoteTunnel);
                    remoteTunnel.connect(destAddress);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"TcpProxyServer onAccepted catch an exception: {ex}");

                if (localTunnel != null)
                {
                    localTunnel.dispose();
                }
            }
        }
    }
}