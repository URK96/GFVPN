using Android.Content;
using Android.OS;

using Java.IO;
using Java.Lang;

namespace GFVPN.Utils
{
    public class TcpDataSaveHelper
    {
        Context context;
        private const string TAG = "TcpDataSaveHelper";
        private string dir;
        private SaveData lastSaveData;
        private File lastSaveFile;
        int requestNum = 0;
        int responseNum = 0;
        public const string REQUEST = "request";
        public const string RESPONSE = "response";

        public TcpDataSaveHelper(string dir, Context context)
        {
            this.dir = dir;
            this.context = context;
        }

        public void addData(SaveData data)
        {
            ThreadProxy.getInstance().execute(new Runnable(() =>
            {
                if (lastSaveData == null || (lastSaveData.IsRequest ^ data.IsRequest))
                {
                    newFileAndSaveData(data);
                }
                else
                {
                    appendFileData(data);
                }

                lastSaveData = data;
            }));
        }

        private void appendFileData(SaveData data)
        {
            RandomAccessFile randomAccessFile;
            try
            {
                randomAccessFile = new RandomAccessFile(lastSaveFile.AbsolutePath, "rw");
                long length = randomAccessFile.Length();

                randomAccessFile.Seek(length);
                randomAccessFile.Write(data.NeedParseData, data.OffSet, data.PlayoffSize);
            }
            catch (Exception)
            {

            }
        }

        private void newFileAndSaveData(SaveData data)
        {
            int saveNum;

            if (data.IsRequest)
            {
                saveNum = requestNum;
                requestNum++;
            }
            else
            {
                saveNum = responseNum;
                responseNum++;
            }

            var file = new File(dir);

            if (!file.Exists())
            {
                file.Mkdirs();
            }

            string childName = $"{(data.IsRequest ? REQUEST : RESPONSE)}{saveNum}";

            lastSaveFile = new File(file, childName);

            try
            {
                using (var fileOutputStream = new FileOutputStream(lastSaveFile))
                {
                    fileOutputStream.Write(data.NeedParseData, data.OffSet, data.PlayoffSize);
                    fileOutputStream.Flush();
                }
            }
            catch (Exception ex)
            {
                VPNLog.d(TAG, $"failed to saveData {ex.Message}");
            }

            if (lastSaveFile.ToString().Contains("request") && new PacketClass().isInclude(lastSaveFile, "gf-game"))
            {
                var handler = new Handler(Looper.MainLooper);

                handler.PostDelayed(new Runnable(() =>
                {
                    new PAlarmAddClass().add(lastSaveFile);
                }), 0);
            }
        }


        public class SaveData
        {
            public bool IsRequest { get; set; }
            public byte[] NeedParseData { get; set; }
            public int OffSet { get; set; }
            public int PlayoffSize { get; set; }

            public SaveData(Builder builder)
            {
                IsRequest = builder.IsRequest;
                NeedParseData = builder.NeedParseData;
                OffSet = builder.OffSet;
                PlayoffSize = builder.Length;
            }


            public class Builder
            {
                public bool IsRequest { get; set; }
                public byte[] NeedParseData { get; set; }
                public int OffSet { get; set; }
                public int Length { get; set; }

                public Builder()
                {

                }

                public SaveData build()
                {
                    return new SaveData(this);
                }
            }
        }
    }
}