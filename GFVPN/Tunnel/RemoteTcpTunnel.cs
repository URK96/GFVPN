using Android.Content;
using Android.OS;

using GFVPN.NAT;
using GFVPN.ParseProcess;
using GFVPN.Utils;

using Java.IO;
using Java.Lang;
using Java.Net;
using Java.Nio;
using Java.Nio.Channels;

using StringBuilder = System.Text.StringBuilder;

namespace GFVPN.Tunnel
{
    public class RemoteTcpTunnel : RawTcpTunnel
    {
        Context context;
        TcpDataSaveHelper helper;
        NatSession session;
        private Handler handler;

        public RemoteTcpTunnel(InetSocketAddress serverAddress, Selector selector, short portKey, Context context) : base(serverAddress, selector, portKey)
        {
            this.context = context;
            session = NatSessionManager.getSession(portKey);
            string helperDir = new StringBuilder()
                        .Append(VPNConstants.DATA_DIR)
                        .Append(TimeFormatUtil.formatYYMMDDHHMMSS(session.vpnStartTime))
                        .Append("/")
                        .Append(session.getUniqueName())
                        .ToString();

            helper = new TcpDataSaveHelper(helperDir, context);
            handler = new Handler(Looper.MainLooper);
        }

        protected override void afterReceived(ByteBuffer buffer)
        {
            base.afterReceived(buffer);
            refreshSessionAfterRead(buffer.Limit());

            var saveDataBuilder = new TcpDataSaveHelper.SaveData.Builder()
            {
                IsRequest = false,
                NeedParseData = buffer.ToArray<byte>(),
                Length = buffer.Limit(),
                OffSet = 0
            };

            helper.addData(saveDataBuilder.build());
        }

        protected override void beforeSend(ByteBuffer buffer)
        {
            base.beforeSend(buffer);

            var saveDataBuilder = new TcpDataSaveHelper.SaveData.Builder()
            {
                IsRequest = true,
                NeedParseData = buffer.ToArray<byte>(),
                Length = buffer.Limit(),
                OffSet = 0,
            };

            helper.addData(saveDataBuilder.build());
            refreshAppInfo();
        }

        private void refreshAppInfo()
        {
            if (session.appInfo != null)
            {
                return;
            }

            if (PortHostService.getInstance() != null)
            {
                ThreadProxy.getInstance().execute(new Runnable(() =>
                {
                    PortHostService.getInstance().refreshSessionInfo();
                }));
            }
        }

        private void refreshSessionAfterRead(int size)
        {
            session.receivePacketNum++;
            session.receiveByteNum += size;
        }

        protected override void onDispose()
        {
            base.onDispose();
            handler.PostDelayed(new Runnable(() =>
            {
                ThreadProxy.getInstance().execute(new Runnable(() =>
                {
                    if (session.receiveByteNum == 0 && session.bytesSent == 0)
                    {
                        return;
                    }

                    string configFileDir = $"{VPNConstants.CONFIG_DIR}{TimeFormatUtil.formatYYMMDDHHMMSS(session.vpnStartTime)}";
                    var parentFile = new File(configFileDir);

                    if (!parentFile.Exists())
                    {
                        parentFile.Mkdirs();
                    }

                    var file = new File(parentFile, session.getUniqueName());

                    if (file.Exists())
                    {
                        return;
                    }

                    ACache configACache = ACache.get(parentFile);

                    configACache.put(session.getUniqueName(), session);
                })); 
            }), 1000);

        }
    }
}