using Java.Lang;
using Java.Util;
using System;
using Java.Util.Concurrent;
using System.Collections.Generic;
using GFVPN;
using System.Collections;

/*package com.fqxd.gftools.vpn.nat;

import com.fqxd.gftools.vpn.processparse.PortHostService;
import com.fqxd.gftools.vpn.utils.CommonMethods;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;*/

namespace GFVPN.NAT
{
    public class NatSessionManager
    {
        
        static readonly int MAX_SESSION_COUNT = 64;
        private static readonly long SESSION_TIME_OUT_NS = 60 * 1000L;
        //private static readonly ConcurrentHashMap<Short, NatSession> sessions = new ConcurrentHashMap<Short, NatSession>();
        private static readonly Dictionary<short, NatSession> sessions = new Dictionary<short, NatSession>();

        public static NatSession getSession(short portKey)
        {
            return sessions[portKey];
        }

        public static int getSessionCount()
        {
            return sessions.Count;
        }

        static void clearExpiredSessions()
        {
            long now = ConvertUtils.GetCurrentTimeMillis();

            var it = sessions.GetEnumerator();

            foreach (var next in sessions)
            {
                if ((now - next.Value.lastRefreshTime) > SESSION_TIME_OUT_NS)
                {
                    sessions.Remove(next.Key);
                }
            }

            /*Set<Map.Entry<Short, NatSession>> entries = sessions.();
            Iterator<Map.Entry<Short, NatSession>> iterator = entries.iterator();

            while (iterator.hasNext())
            {
                Map.Entry<Short, NatSession> next = iterator.next();

                if (now - next.getValue().lastRefreshTime > SESSION_TIME_OUT_NS)
                {
                    iterator.remove();
                }
            }*/
        }

        public static void clearAllSession()
        {
            sessions.Clear();
        }

        public static List<NatSession> getAllSession()
        {
            var natSessions = new List<NatSession>();
            
            natSessions.AddRange(sessions.Values);

            return natSessions;

            /*ArrayList<NatSession> natSessions = new ArrayList<>();
            Set<Map.Entry<Short, NatSession>> entries = sessions.entrySet();
            Iterator<Map.Entry<Short, NatSession>> iterator = entries.iterator();
            while (iterator.hasNext())
            {
                Map.Entry<Short, NatSession> next = iterator.next();
                natSessions.add(next.getValue());
            }
            return natSessions;*/
        }

        public static NatSession createSession(short portKey, int remoteIP, short remotePort, string type)
        {
            if (sessions.Count > MAX_SESSION_COUNT)
            {
                clearExpiredSessions();
            }

            NatSession session = new NatSession
            {
                lastRefreshTime = ConvertUtils.GetCurrentTimeMillis(),
                remoteIP = remoteIP,
                remotePort = remotePort,
                localPort = portKey
            };


            if (session.remoteHost == null)
            {
                session.remoteHost = CommonMethods.ipIntToString(remoteIP);
            }

            session.type = type;
            session.refreshIpAndPort();
            sessions.Add(portKey, session);

            return session;
        }

        public static void removeSession(short portKey)
        {
            sessions.Remove(portKey);
        }
    }
}