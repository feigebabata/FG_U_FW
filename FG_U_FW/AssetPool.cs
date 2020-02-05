using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FG_U_FW
{
    public class Pool<K,T> where T:class
    {
        public class Unit
        {
            public float Weights;
            public K Key;
            public T Value;
        }
        List<Unit> m_list = new List<Unit>();
        int m_maxCount;

        public Pool(Func<K,T> _create,int _maxCount)
        {
            m_maxCount = _maxCount;
        }

        public void Clear(int _count)
        {
            if(_count>0 && _count<m_list.Count)
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
