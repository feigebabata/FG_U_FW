
using System;
using System.Net.Sockets;
using UnityEngine;

namespace FG_U_FW.NetServer
{
    public class ChatServer : Net.ServerBase
    {
        
        protected override void Decode(Socket _client)
        {
            int size = 0;
            byte[] buffer = null;
            m_BufferSizes.TryGetValue(_client,out size);
            m_Buffers.TryGetValue(_client,out buffer);

            ushort msgLength = BitConverter.ToUInt16(buffer,0);
            if(msgLength<=size)
            {
                string msg = System.Text.Encoding.UTF8.GetString(buffer,2,buffer.Length-2);
                byte[] sendData = new byte[msgLength];
                Array.Copy(buffer,sendData,sendData.Length);
                int newSize = size;
                if(msgLength==size)
                {
                    newSize=0;
                }
                else
                {
                    newSize = size-msgLength;
                    Array.Copy(buffer,msgLength,buffer,0,newSize);
                }
                Debug.LogWarning("服务器接收:"+msg);
                SendAll(sendData,0,sendData.Length);
            }
        }

        protected override void StartFinish(bool _succ)
        {
            Main.I.Sys<Net>().Client<NetClient.ChatClient>().Connect(Net.IP.ToString(),6666);
        }
    }
}