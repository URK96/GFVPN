using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;

using Java.IO;
using Java.Lang;
using Java.Util.Concurrent.Atomic;

using Org.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using Math = System.Math;

namespace GFVPN.Utils
{
    public class ACache : Java.Lang.Object
    {
        public const int TIME_HOUR = 60 * 60;
        public const int TIME_DAY = TIME_HOUR * 24;
        private const int MAX_SIZE = 1000 * 1000 * 50;
        private const int MAX_COUNT = int.MaxValue;
        private static Dictionary<string, ACache> mInstanceMap = new Dictionary<string, ACache>();
        private ACacheManager mCache;

        public static ACache get(Context context)
        {
            return get(context, "ACache");
        }

        public static ACache get(Context context, string cacheName)
        {
            File f = new File(context.FilesDir, cacheName);

            return get(f, MAX_SIZE, MAX_COUNT);
        }

        public static ACache get(File cacheDir)
        {
            return get(cacheDir, MAX_SIZE, MAX_COUNT);
        }

        public static ACache get(Context context, long max_zise, int max_count)
        {
            File f = new File(context.FilesDir, "ACache");

            return get(f, max_zise, max_count);
        }

        public static ACache get(File cacheDir, long maxSise, int maxCount)
        {
            ACache manager;
            string key = cacheDir.AbsolutePath + myPid();

            if (!mInstanceMap.TryGetValue(key, out manager))
            {
                manager = new ACache(cacheDir, maxSise, maxCount);
                mInstanceMap.Add(key, manager);
            }

            return manager;
        }

        private static string myPid()
        {
            return $"_{Android.OS.Process.MyPid()}";
        }

        private ACache(File cacheDir, long maxSize, int maxCount)
        {
            if (!cacheDir.Exists() && !cacheDir.Mkdirs())
            {
                throw new Java.Lang.RuntimeException($"can't make dirs in {cacheDir.AbsolutePath}");
            }

            mCache = new ACacheManager(cacheDir, maxSize, maxCount);
        }


        public void put(string key, string value)
        {
            File file = mCache.newFile(key);
            BufferedWriter writer = null;

            try
            {
                writer = new BufferedWriter(new FileWriter(file), 1024);
                writer.Write(value);
            }
            catch (IOException ex)
            {
                ex.PrintStackTrace();
            }
            finally
            {
                if (writer != null)
                {
                    try
                    {
                        writer.Flush();
                        writer.Dispose();
                    }
                    catch (IOException ex)
                    {
                        ex.PrintStackTrace();
                    }
                }

                mCache.put(file);
            }
        }


        public void put(string key, string value, int saveTime)
        {
            put(key, Utils.newStringWithDateInfo(saveTime, value));
        }


        public string getAsString(string key)
        {
            File file = mCache.get(key);

            if (!file.Exists())
            {
                return null;
            }

            bool removeFile = false;
            BufferedReader reader = null;

            try
            {
                reader = new BufferedReader(new FileReader(file));
                string readString = "";
                string currentLine;

                while ((currentLine = reader.ReadLine()) != null)
                {
                    readString += currentLine;
                }
                if (!Utils.isDue(readString))
                {
                    return Utils.clearDateInfo(readString);
                }
                else
                {
                    removeFile = true;

                    return null;
                }
            }
            catch (IOException ex)
            {
                ex.PrintStackTrace();

                return null;
            }
            finally
            {
                if (reader != null)
                {
                    try
                    {
                        reader.Dispose();
                    }
                    catch (IOException ex)
                    {
                        ex.PrintStackTrace();
                    }
                }

                if (removeFile)
                {
                    remove(key);
                }
            }
        }

        public void put(string key, JSONObject value)
        {
            put(key, value.ToString());
        }


        public void put(string key, JSONObject value, int saveTime)
        {
            put(key, value.ToString(), saveTime);
        }


        public JSONObject getAsJSONObject(string key)
        {
            string jsonString = getAsString(key);

            try
            {
                var obj = new JSONObject(jsonString);

                return obj;
            }
            catch (System.Exception ex)
            {
                VPNLog.d("ACache", ex.ToString());

                return null;
            }
        }

        public void put(string key, JSONArray value)
        {
            put(key, value.ToString());
        }


        public void put(string key, JSONArray value, int saveTime)
        {
            put(key, value.ToString(), saveTime);
        }


        public JSONArray getAsJSONArray(string key)
        {
            string jsonString = getAsString(key);

            try
            {
                var obj = new JSONArray(jsonString);

                return obj;
            }
            catch (System.Exception ex)
            {
                VPNLog.d("ACache", ex.ToString());

                return null;
            }
        }


        public void put(string key, byte[] value)
        {
            var file = mCache.newFile(key);
            FileOutputStream output = null;

            try
            {
                output = new FileOutputStream(file);

                output.Write(value);
            }
            catch (System.Exception ex)
            {
                VPNLog.d("ACache", ex.ToString());
            }
            finally
            {
                if (output != null)
                {
                    try
                    {
                        output.Flush();
                        output.Dispose();
                    }
                    catch (IOException ex)
                    {
                        ex.PrintStackTrace();
                    }
                }

                mCache.put(file);
            }
        }


        public void put(string key, byte[] value, int saveTime)
        {
            put(key, Utils.newByteArrayWithDateInfo(saveTime, value));
        }

        public byte[] getAsBinary(string key)
        {
            RandomAccessFile raFile = null;
            bool removeFile = false;

            try
            {
                File file = mCache.get(key);

                if (!file.Exists())
                {
                    return null;
                }

                raFile = new RandomAccessFile(file, "r");
                byte[] byteArray = new byte[(int)raFile.Length()];

                raFile.Read(byteArray);

                if (!Utils.isDue(byteArray))
                {
                    return Utils.clearDateInfo(byteArray);
                }
                else
                {
                    removeFile = true;

                    return null;
                }
            }
            catch (System.Exception ex)
            {
                VPNLog.d("ACache", ex.ToString());

                return null;
            }
            finally
            {
                if (raFile != null)
                {
                    try
                    {
                        raFile.Dispose();
                    }
                    catch (IOException ex)
                    {
                        ex.PrintStackTrace();
                    }
                }

                if (removeFile)
                {
                    remove(key);
                }
            }
        }

        public void put(string key, ISerializable value)
        {
            put(key, value, -1);
        }


        public void put(string key, ISerializable value, int saveTime)
        {
            using var baos = new System.IO.MemoryStream();

            try
            {
                using var oos = new ObjectOutputStream(baos);

                oos.WriteObject((Java.Lang.Object)value);

                byte[] data = baos.ToArray();

                if (saveTime != -1)
                {
                    put(key, data, saveTime);
                }
                else
                {
                    put(key, data);
                }
            }
            catch (System.Exception ex)
            {
                VPNLog.d("ACache", ex.ToString());
            }
        }

        public object getAsObject(string key)
        {
            byte[] data = getAsBinary(key);

            if (data != null)
            {
                using var bais = new System.IO.MemoryStream(data);

                try
                {
                    using var ois = new ObjectInputStream(bais);

                    var reObject = ois.ReadObject();

                    return reObject;
                }
                catch (System.Exception)
                {
                    
                }
            }

            return null;
        }

        public bool getAsBoolean(string key, bool defaultBoolean)
        {
            byte[] data = getAsBinary(key);

            if (data != null)
            {
                using System.IO.MemoryStream bias = new System.IO.MemoryStream(data);

                try
                {
                    using var ois = new ObjectInputStream(bias);

                    var reObject = ois.ReadObject();

                    return (bool)reObject;
                }
                catch (System.Exception)
                {
                    return defaultBoolean;
                }
            }

            return defaultBoolean;
        }

        public void put(string key, Bitmap value)
        {
            put(key, Utils.Bitmap2Bytes(value));
        }


        public void put(string key, Bitmap value, int saveTime)
        {
            put(key, Utils.Bitmap2Bytes(value), saveTime);
        }

        public Bitmap getAsBitmap(string key)
        {
            if (getAsBinary(key) == null)
            {
                return null;
            }
            return Utils.Bytes2Bimap(getAsBinary(key));
        }


        public void put(string key, Drawable value)
        {
            put(key, Utils.drawable2Bitmap(value));
        }


        public void put(string key, Drawable value, int saveTime)
        {
            put(key, Utils.drawable2Bitmap(value), saveTime);
        }


        public Drawable getAsDrawable(string key)
        {
            if (getAsBinary(key) == null)
            {
                return null;
            }

            return Utils.bitmap2Drawable(Utils.Bytes2Bimap(getAsBinary(key)));
        }

        public File file(string key)
        {
            File f = mCache.newFile(key);

            if (f.Exists())
            {
                return f;
            }

            return null;
        }


        public bool remove(string key)
        {
            return mCache.remove(key);
        }

        public void clear()
        {
            mCache.clear();
        }

        public class ACacheManager : Java.Lang.Object
        {
            private AtomicLong cacheSize;
            private AtomicInteger cacheCount;
            private long sizeLimit;
            private int countLimit;
            private ConcurrentDictionary<File, long> lastUsageDates = new ConcurrentDictionary<File, long>();
            protected File cacheDir;

            public ACacheManager(File cacheDir, long sizeLimit, int countLimit)
            {
                this.cacheDir = cacheDir;
                this.sizeLimit = sizeLimit;
                this.countLimit = countLimit;

                cacheSize = new AtomicLong();
                cacheCount = new AtomicInteger();

                calculateCacheSizeAndCacheCount();
            }


            private void calculateCacheSizeAndCacheCount()
            {
                new Thread(new Runnable(() =>
                {
                    int size = 0;
                    int count = 0;
                    var cachedFiles = cacheDir.ListFiles();

                    if (cachedFiles != null)
                    {
                        foreach (var cachedFile in cachedFiles)
                        {
                            size += (int)calculateSize(cachedFile);
                            count += 1;
                            lastUsageDates.TryAdd(cachedFile, cachedFile.LastModified());
                        }

                        cacheSize.Set(size);
                        cacheCount.Set(count);
                    }
                }));
            }

            internal void put(File file)
            {
                int curCacheCount = cacheCount.Get();

                while (curCacheCount + 1 > countLimit)
                {
                    long freedSize = removeNext();

                    cacheSize.AddAndGet(-freedSize);

                    curCacheCount = cacheCount.AddAndGet(-1);
                }

                cacheCount.AddAndGet(1);

                long valueSize = calculateSize(file);
                long curCacheSize = cacheSize.Get();

                while (curCacheSize + valueSize > sizeLimit)
                {
                    long freedSize = removeNext();
                    curCacheSize = cacheSize.AddAndGet(-freedSize);
                }

                cacheSize.AddAndGet(valueSize);

                long currentTime = ConvertUtils.GetCurrentTimeMillis();

                file.SetLastModified(currentTime);
                lastUsageDates.TryAdd(file, currentTime);
            }

            internal File get(string key)
            {
                var file = newFile(key);
                long currentTime = ConvertUtils.GetCurrentTimeMillis();

                file.SetLastModified(currentTime);
                lastUsageDates.TryAdd(file, currentTime);

                return file;
            }

            internal File newFile(string key)
            {
                return new File(cacheDir, key);
            }

            internal bool remove(string key)
            {
                var image = get(key);

                return image.Delete();
            }

            internal void clear()
            {
                lastUsageDates.Clear();
                cacheSize.Set(0);

                var files = cacheDir.ListFiles();

                if (files != null)
                {
                    foreach (var f in files)
                    {
                        f.Delete();
                    }
                }
            }


            private long removeNext()
            {
                if (lastUsageDates.IsEmpty)
                {
                    return 0;
                }

                long oldestUsage = 0;
                File mostLongUsedFile = null;

                lock (lastUsageDates)
                {
                    foreach (var entry in lastUsageDates)
                    {
                        if (mostLongUsedFile == null)
                        {
                            mostLongUsedFile = entry.Key;
                            oldestUsage = entry.Value;
                        }
                        else
                        {
                            long lastValueUsage = entry.Value;

                            if (lastValueUsage < oldestUsage)
                            {
                                oldestUsage = lastValueUsage;
                                mostLongUsedFile = entry.Key;
                            }
                        }
                    }
                }

                long fileSize = calculateSize(mostLongUsedFile);

                if (mostLongUsedFile.Delete())
                {
                    lastUsageDates.TryRemove(mostLongUsedFile, out long temp);
                }

                return fileSize;
            }

            private long calculateSize(File file)
            {
                return file.Length();
            }
        }

        private static class Utils
        {
            internal static bool isDue(string str)
            {
                return isDue(Encoding.Default.GetBytes(str));
            }


            internal static bool isDue(byte[] data)
            {
                string[] strs = getDateInfoFromDate(data);

                if (strs != null && strs.Length == 2)
                {
                    string saveTimeStr = strs[0];

                    while (saveTimeStr.StartsWith("0"))
                    {
                        saveTimeStr = saveTimeStr.Substring(1, saveTimeStr.Length);
                    }

                    long saveTime = long.Parse(saveTimeStr);
                    long deleteAfter = long.Parse(strs[1]);

                    if (ConvertUtils.GetCurrentTimeMillis() > (saveTime + deleteAfter * 1000))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal static string newStringWithDateInfo(int second, string strInfo)
            {
                return createDateInfo(second) + strInfo;
            }

            internal static byte[] newByteArrayWithDateInfo(int second, byte[] data2)
            {
                byte[] data1 = Encoding.Default.GetBytes(createDateInfo(second));
                byte[] retdata = new byte[data1.Length + data2.Length];

                Array.Copy(data1, 0, retdata, 0, data1.Length);
                Array.Copy(data2, 0, retdata, data1.Length, data2.Length);

                return retdata;
            }

            internal static string clearDateInfo(string strInfo)
            {
                if (strInfo != null && hasDateInfo(Encoding.Default.GetBytes(strInfo)))
                {
                    strInfo = strInfo.Substring(strInfo.IndexOf(mSeparator) + 1, strInfo.Length);
                }

                return strInfo;
            }

            internal static byte[] clearDateInfo(byte[] data)
            {
                if (hasDateInfo(data))
                {
                    return copyOfRange(data, indexOf(data, mSeparator) + 1, data.Length);
                }

                return data;
            }

            private static bool hasDateInfo(byte[] data)
            {
                return (data != null) && (data.Length > 15) && (data[13] == '-') && (indexOf(data, mSeparator) > 14);
            }

            private static string[] getDateInfoFromDate(byte[] data)
            {
                if (hasDateInfo(data))
                {
                    string saveDate = Encoding.Default.GetString(copyOfRange(data, 0, 13));
                    string deleteAfter = Encoding.Default.GetString(copyOfRange(data, 14, indexOf(data, mSeparator)));

                    return new string[] { saveDate, deleteAfter };
                }

                return null;
            }

            private static int indexOf(byte[] data, char c)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == c)
                    {
                        return i;
                    }
                }
                return -1;
            }

            private static byte[] copyOfRange(byte[] original, int from, int to)
            {
                int newLength = to - from;

                if (newLength < 0)
                {
                    throw new IllegalArgumentException(from + " > " + to);
                }

                byte[] copy = new byte[newLength];

                Array.Copy(original, from, copy, 0, Math.Min(original.Length - from, newLength));

                return copy;
            }

            private const char mSeparator = ' ';

            private static string createDateInfo(int second)
            {
                string currentTime = ConvertUtils.GetCurrentTimeMillis().ToString();

                while (currentTime.Length < 13)
                {
                    currentTime = $"0{currentTime}";
                }

                return $"{currentTime}-{second}{mSeparator}";
            }

            internal static byte[] Bitmap2Bytes(Bitmap bm)
            {
                if (bm == null)
                {
                    return null;
                }

                var baos = new System.IO.MemoryStream();

                bm.Compress(Bitmap.CompressFormat.Png, 100, baos);

                return baos.ToArray();
            }

            internal static Bitmap Bytes2Bimap(byte[] b)
            {
                if (b.Length == 0)
                {
                    return null;
                }

                return BitmapFactory.DecodeByteArray(b, 0, b.Length);
            }


            internal static Bitmap drawable2Bitmap(Drawable drawable)
            {
                if (drawable == null)
                {
                    return null;
                }

                int w = drawable.IntrinsicWidth;
                int h = drawable.IntrinsicHeight;

                var config = (drawable.Opacity != -1) ? Bitmap.Config.Argb8888 : Bitmap.Config.Rgb565;
                var bitmap = Bitmap.CreateBitmap(w, h, config);
                var canvas = new Canvas(bitmap);

                drawable.SetBounds(0, 0, w, h);
                drawable.Draw(canvas);

                return bitmap;
            }


            [SuppressWarnings(Value = new string[] { "deprecation" })]
            internal static Drawable bitmap2Drawable(Bitmap bm)
            {
                if (bm == null)
                {
                    return null;
                }

                return new BitmapDrawable(bm);
            }
        }
    }
}