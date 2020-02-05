using System;
using System.Collections;
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

        
    }
    
}
