using System;
using System.Collections;
using System.Collections.Concurrent;

namespace FG_U_FW
{
    public class ChatClient : Net.ClientBase
    {
        public override void ConnectResult(bool _succ)
        {
            WaitReceive().Start();
        }

        public override void Decode()
        {
            m_ReceiveQueue.Enqueue(null);
        }

        IEnumerator WaitReceive()
        {
            while(m_IsConnect)
            {
                yield return m_ReceiveQueue;
                while(m_ReceiveQueue.Count>0)
                {
                    object data=null;
                    if(m_ReceiveQueue.TryDequeue(out data))
                    {

                    }
                }
            }
        }
        
    }
}