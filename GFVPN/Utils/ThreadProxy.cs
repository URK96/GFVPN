/*package com.fqxd.gftools.vpn.utils;

import androidx.annotation.NonNull;

import java.util.concurrent.Executor;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.ThreadFactory;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;*/

using Java.Lang;
using Java.Util.Concurrent;
using System.Collections.Concurrent;

namespace GFVPN.Utils
{
    public class ThreadProxy
    {
        private ThreadPoolExecutor executor;

        static class InnerClass
        {
            internal static ThreadProxy instance = new ThreadProxy();
        }

        private ThreadProxy()
        {
            executor = new ThreadPoolExecutor(1, 4, 10L, TimeUnit.Milliseconds, new LinkedBlockingQueue(1024), new ThreadFactory());
        }

        public void execute(Runnable run)
        {
            executor.Execute(run);
        }

        public static ThreadProxy getInstance()
        {
            return InnerClass.instance;
        }

        private class ThreadFactory : Java.Lang.Object, IThreadFactory
        {
            public Thread NewThread(IRunnable r)
            {
                var thread = new Thread(r);

                thread.Name = "ThreadProxy";

                return thread;
            }
        }
    }
}