using System;
using System.Collections.Generic;

namespace FG_U_FW
{
    public abstract class OnlyAsyncWait<T>
    {
        Dictionary<Action<T>,string> m_waitloads = new Dictionary< Action<T>,string>();

        
        public void Wait(string _url,Action<T> _callback)
        {
            if(string.IsNullOrEmpty(_url))
            {
                if(m_waitloads.ContainsKey(_callback))
                {
                    m_waitloads.Remove(_callback);
                    removeWait(_url);
                }
            }
            else
            {
                if(!m_waitloads.ContainsKey(_callback))
                {
                    m_waitloads.Add(_callback,_url);
                    addWait(_url);
                }
                else if(m_waitloads[_callback] != _url)
                {
                    string oldUrl = m_waitloads[_callback];
                    m_waitloads[_callback] = _url;
                    removeWait(oldUrl);
                    addWait(_url);
                }
            }
        }

        protected abstract void addWait(string _url);
        protected abstract void removeWait(string _url);

        protected void callWait(string _url,T _data)
        {
            List<Action<T>> calls = new List<Action<T>>();
            
            using(var ie = m_waitloads.GetEnumerator())
            {
                while(ie.MoveNext())
                {
                    if(ie.Current.Value==_url)
                    {
                        calls.Add(ie.Current.Key);
                    }
                }
            }

            for (int i = 0; i < calls.Count; i++)
            {
                calls[i](_data);
                m_waitloads.Remove(calls[i]);
            }

            calls.Clear();
        }
    }
}