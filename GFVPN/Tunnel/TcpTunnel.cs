using GFVPN.NAT;
using GFVPN.Service;
using GFVPN.Utils;

using Java.Net;
using Java.Nio;
using Java.Nio.Channels;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GFVPN.Tunnel
{
    public abstract class TcpTunnel : Java.Lang.Object, IKeyHandler
    {

        public static long sessionCount;
        protected InetSocketAddress mDestAddress;
        private SocketChannel mInnerChannel;
        private Selector mSelector;
        public bool isHttpsRequest { get; set; }
        private TcpTunnel mBrotherTunnel;
        private bool mDisposed;
        private InetSocketAddress mServerEP;
        short portKey;
        ConcurrentQueue<ByteBuffer> needWriteData = new ConcurrentQueue<ByteBuffer>();

        public TcpTunnel(SocketChannel innerChannel, Selector selector)
        {
            mInnerChannel = innerChannel;
            mSelector = selector;

            sessionCount++;
        }

        public TcpTunnel(InetSocketAddress serverAddress, Selector selector, short portKey)
        {
            SocketChannel innerChannel = SocketChannel.Open();
            innerChannel.ConfigureBlocking(false);
            mInnerChannel = innerChannel;
            mSelector = selector;
            mServerEP = serverAddress;
            this.portKey = portKey;

            sessionCount++;
        }

        public void onKeyReady(SelectionKey key)
        {
            if (key.IsReadable)
            {
                onReadable(key);
            }
            else if (key.IsWritable)
            {
                onWritable(key);
            }
            else if (key.IsConnectable)
            {
                onConnectable();
            }
        }

        protected abstract void onConnected();

        protected abstract bool isTunnelEstablished();

        protected abstract void beforeSend(ByteBuffer buffer);

        protected abstract void afterReceived(ByteBuffer buffer);

        protected abstract void onDispose();

        public void setBrotherTunnel(TcpTunnel brotherTunnel)
        {
            mBrotherTunnel = brotherTunnel;
        }


        public void connect(InetSocketAddress destAddress)
        {
            if (VpnServiceHelper.protect(mInnerChannel.Socket()))
            {
                mDestAddress = destAddress;
                mInnerChannel.Register(mSelector, Operations.Connect, this);
                mInnerChannel.Connect(mServerEP);
                Debug.WriteLine($"Connecting to {mServerEP}");
            }
            else
            {
                throw new Exception("VPN protect socket failed.");
            }
        }

        public void onConnectable()
        {
            try
            {
                if (mInnerChannel.FinishConnect())
                {
                    onConnected();
                    Debug.WriteLine($"Connected to {mServerEP}");
                }
                else
                {
                    Debug.Fail($"Connect to {mServerEP} failed.");
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"Connect to {mServerEP.ToString()} failed: {ex}");
                Dispose();
            }
        }

        protected void beginReceived()
        {
            if (mInnerChannel.IsBlocking)
            {
                mInnerChannel.ConfigureBlocking(false);
            }

            mSelector.Wakeup();
            mInnerChannel.Register(mSelector, Operations.Read, this);
        }

        public void onReadable(SelectionKey key)
        {
            try
            {
                var buffer = ByteBuffer.Allocate(FirewallVpnService.MUTE_SIZE);

                buffer.Clear();

                int bytesRead = mInnerChannel.Read(buffer);

                if (bytesRead > 0)
                {
                    buffer.Flip();
                    afterReceived(buffer);

                    sendToBrother(key, buffer);

                }
                else if (bytesRead < 0)
                {
                    Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"onReadable catch an exception: {ex.ToString()}");
                Dispose();
            }
        }



        protected void sendToBrother(SelectionKey key, ByteBuffer buffer)
        {
            if (isTunnelEstablished() && buffer.HasRemaining)
            {
                mBrotherTunnel.getWriteDataFromBrother(buffer);
            }
        }

        private void getWriteDataFromBrother(ByteBuffer buffer)
        {
            if (buffer.HasRemaining && (needWriteData.Count == 0))
            {
                int writeSize = 0;

                try
                {
                    writeSize = write(buffer);
                }
                catch (Exception ex)
                {
                    writeSize = 0;

                    Debug.WriteLine(ex.ToString());
                }

                if (writeSize > 0)
                {
                    return;
                }
            }

            needWriteData.Enqueue(buffer);

            try
            {
                mSelector.Wakeup();
                mInnerChannel.Register(mSelector, Operations.Read | Operations.Write, this);
            }
            catch (ClosedChannelException ex)
            {
                ex.PrintStackTrace();
            }
        }

        protected int write(ByteBuffer buffer)
        {
            int byteSendSum = 0;

            beforeSend(buffer);

            while (buffer.HasRemaining)
            {
                int byteSent = mInnerChannel.Write(buffer);

                byteSendSum += byteSent;

                if (byteSent == 0)
                {
                    break;
                }
            }

            return byteSendSum;
        }


        public void onWritable(SelectionKey key)
        {
            try
            {
                if (!needWriteData.TryDequeue(out ByteBuffer mSendRemainBuffer))
                {
                    return;
                }

                write(mSendRemainBuffer);

                if (needWriteData.Count == 0)
                {
                    try
                    {
                        mSelector.Wakeup();
                        mInnerChannel.Register(mSelector, Operations.Read, this);
                    }
                    catch (ClosedChannelException ex)
                    {
                        ex.PrintStackTrace();
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"onWritable catch an exception: {ex.ToString()}");

                Dispose();
            }
        }

        protected void onTunnelEstablished()
        {
            beginReceived();
            mBrotherTunnel.beginReceived();
        }

        public void dispose()
        {
            disposeInternal(true);
        }

        void disposeInternal(bool disposeBrother)
        {
            if (!mDisposed)
            {
                try
                {
                    mInnerChannel.Close();
                }
                catch (Exception ex)
                {
                    Debug.Fail($"InnerChannel close catch an exception: {ex.ToString()}");
                }

                if (mBrotherTunnel != null && disposeBrother)
                {
                    mBrotherTunnel.disposeInternal(false);
                }

                mInnerChannel = null;
                mSelector = null;
                mBrotherTunnel = null;
                mDisposed = true;
                sessionCount--;

                onDispose();
                NatSessionManager.removeSession(portKey);
            }
        }
    }
}