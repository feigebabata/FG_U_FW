using UnityEngine;
using System;
using System.Collections.Concurrent;

namespace FG_U_FW
{
    public class Data : ISys
    {
        ConcurrentDictionary<Type,IDataBase> m_databases = new ConcurrentDictionary<Type, IDataBase>();
        public void Clear()
        {
            
        }    
        
        public T DataBase<T>() where T:class,IDataBase
        {
            var type = typeof(T);
            IDataBase database = default;
            if(!m_databases.ContainsKey(type))
            {
                database = Activator.CreateInstance<T>();
                if(!m_databases.TryAdd(type,database))
                {
                    Debug.LogError("[Data.DataBase]添加失败");
                }
            }
            else
            {
                if(!m_databases.TryGetValue(type,out database))
                {
                    Debug.LogError("[Data.DataBase]获取失败");
                }

            }
            return database as T;
        }

        public void GC()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public interface IDataBase
        {
            void Clear();
        }
    }
}