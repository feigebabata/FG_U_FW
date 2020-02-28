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

        ConcurrentDictionary<Type,ISys> m_sysTab = new ConcurrentDictionary<Type, ISys>();

        public T Sys<T>() where T:class,ISys
        {
            var type = typeof(T);
            ISys sys = default;
            if(!m_sysTab.ContainsKey(type))
            {
                sys = Activator.CreateInstance<T>();
                if(!m_sysTab.TryAdd(type,sys))
                {
                    Debug.LogError("[Main.Sys]添加失败");
                }
            }
            else
            {
                if(!m_sysTab.TryGetValue(type,out sys))
                {
                    Debug.LogError("[Main.Sys]获取失败");
                }
            }
            return sys as T;
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
