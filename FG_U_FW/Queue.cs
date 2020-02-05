using System.Collections.Generic;

namespace FG_U_FW
{
    public class Queue<T>
    {
        List<T> m_list = new List<T>();

        public int Count
        {
            get{return m_list.Count;}
        }
        public T Dequeue()
        {
            if(m_list.Count>0)
            {
                T t = m_list[0];
                m_list.Remove(t);
                return t;
            }
            return default(T);
        }

        public void Enqueue(T _t)
        {
            m_list.Add(_t);
        }

        public void Clear()
        {
            m_list.Clear();
        }

        public T Find(System.Predicate<T> _pre)
        {
            return m_list.Find(_pre);
        }
        
        public int RemoveAll(System.Predicate<T> _pre)
        {
            return m_list.RemoveAll(_pre);
        }
    }
}
