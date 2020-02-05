using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace FG_U_FW
{
    
    public class Main:MonoSingleton<Main>
    {
        [RuntimeInitializeOnLoadMethod]
        static void onGameStart()
        {
            Camera.main.gameObject.AddComponent<FGBBT>();
        }

        Hashtable m_sysTab = new Hashtable();

        public T Sys<T>() where T:class,ISys
        {
            var type = typeof(T);
            if(!m_sysTab.Contains(type))
            {
                T sys = Activator.CreateInstance<T>();
                sys.Init();
                m_sysTab.Add(type,sys);
            }
            return m_sysTab[type] as T;
        }

        ConcurrentQueue<Action> m_childQueue = new ConcurrentQueue<Action>();
        public void ChildToMainThread(Action _callback)
        {
            if(_callback!=null)
            {
                m_childQueue.Enqueue(_callback);
            }
        }

        void Update()
        {
            while(m_childQueue.Count>0)
            {
                Action action;
                if(m_childQueue.TryDequeue(out action))
                {
                    action();
                }
            }

        }

        void OnApplicationQuit()
        {
            Clear();
        }

        public void Clear()
        {
            var ie = m_sysTab.Values.GetEnumerator();
            while(ie.MoveNext())
            {
                (ie.Current as ISys).Clear();
            }
            m_sysTab.Clear();
            Coroutiner.Clear();
            m_childQueue = new ConcurrentQueue<Action>();
        }
        
    }
    
}
