using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FG_U_FW
{
    public class AssetPool<K,T> where T:class
    {
        public class Unit
        {
            public float Weights;
            public K Key;
            public T Value;
        }
        List<Unit> m_list;
        int m_maxCount;

        public AssetPool(int _maxCount)
        {
            m_maxCount = _maxCount;
            m_list = new List<Unit>(_maxCount);
        }

        public void Clear(int _count=0)
        {
            if(_count<m_list.Count)
            {
                m_list.Sort((_u1,_u2)=>{return Mathf.CeilToInt(_u1.Weights-_u2.Weights);});
                while(_count<m_list.Count)
                {
                    m_list[0].Value=null;
                    m_list.RemoveAt(0);
                }
            }
        }

        private void set(K _k,T _t)
        {
            var unit = m_list.Find((_u)=>{return _u.Key.Equals(_k);});
            if(unit!=null)
            {
                unit.Value = _t;
                unit.Weights = 1;
            }
            else
            {
                unit = new Unit();
                unit.Key = _k;
                unit.Value = _t;
                unit.Weights = 1;
                m_list.Add(unit);

                if(m_list.Count>m_maxCount)
                {
                    Clear(m_maxCount);
                }
            }
        }

        private T get(K _key)
        {
            var unit = m_list.Find((_u)=>{return _u.Key.Equals(_key);});
            if(unit!=null)
            {
                unit.Weights+=1/unit.Weights;
                return unit.Value;
            }
            return null;
        }

        public T this[K _key]
        {
            get
            {
                return get(_key);
            }
            set
            {
                set(_key,value);
            }
        }

    }
}
