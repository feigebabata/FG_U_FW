using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace FG_U_FW
{
    public class ChatClient : Net.ClientBase
    {
        protected override void ConnectResult(bool _succ)
        {
            Debug.Log("连接结果 "+_succ);
            
            string msg = "客户端发送";
            byte[] msgData=System.Text.Encoding.UTF8.GetBytes(msg);
            byte[] sendData = new byte[msgData.Length+2];
            Array.Copy(BitConverter.GetBytes((ushort)msgData.Length),sendData,2);
            Array.Copy(msgData,0,sendData,2,msgData.Length);
            Send(sendData);
            Main.I.ChildToMainThread(()=>
            {
                WaitReceive().Start();
            });
        }

        protected override void Decode()
        {
            m_ReceiveQueue.Enqueue(null);
        }

        protected override void OnError(string _msg)
        {
            Debug.LogError(_msg);
        }

        IEnumerator WaitReceive()
        {
            Debug.Log("111");
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