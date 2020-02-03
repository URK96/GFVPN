using Java.Util;
using System;
using System.Collections.Generic;

namespace GFVPN.Utils
{
    public class MyLRUCache<K, V> : LinkedHashMap
    {
        private int maxSize;
        [NonSerialized]
        private CleanupCallback<V> callback;

        public MyLRUCache(int maxSize, CleanupCallback<V> callback) : base(maxSize + 1, 1, true)
        {
            this.maxSize = maxSize;
            this.callback = callback;
        }

        protected bool removeEldestEntry(Entry eldest)
        {
            if (Size() > maxSize)
            {
                callback.cleanUp(eldest.getValue());

                return true;
            }
            return false;
        }

        public interface CleanupCallback<V>
        {
            void cleanUp(V v);
        }
    }
}