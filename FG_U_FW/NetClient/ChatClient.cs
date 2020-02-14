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
            ushort msgLength = BitConverter.ToUInt16(m_Buffer,0);
            if(msgLength<=m_BufferSize)
            {
                string msg = System.Text.Encoding.UTF8.GetString(m_Buffer,2,m_Buffer.Length-2);

                m_BufferSize -= msgLength;
                if(m_BufferSize>0)
                {
                    Array.Copy(m_Buffer,msgLength,m_Buffer,0,m_BufferSize);
                }
                
                // Debug.LogWarning("客户端接收:"+msg);

                m_ReceiveQueue.Enqueue(msg);
            }
        }

        protected override void OnError(string _msg)
        {
            Debug.LogError(_msg);
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
                        Debug.LogWarning("客户端接收:"+data as string);
                    }
                }
            }
        }
        
    }
}