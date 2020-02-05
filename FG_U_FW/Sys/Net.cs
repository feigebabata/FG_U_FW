using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace FG_U_FW
{
    public class Net : ISys
    {
        Hashtable m_clientTab = new Hashtable();
        public void Clear()
        {
        }

        public void Init()
        {
        }

        public T Client<T>() where T:ClientBase
        {
            var type = typeof(T);
            if(!m_clientTab.Contains(type))
            {
                T client = Activator.CreateInstance<T>();
                m_clientTab.Add(type,client);
            }
            return m_clientTab[type] as T;
        }

        public static IPAddress IP
        {
            get
            {
                if(NoNetwork)
                {
                    return null;
                }
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                IPAddress ip = null;
                foreach (var item in ips)
                {
                    if(item.AddressFamily==AddressFamily.InterNetwork || item.AddressFamily==AddressFamily.InterNetworkV6)
                    {
                        ip = item;
                    }
                    if(item.AddressFamily==AddressFamily.InterNetwork)
                    {
                        return item;
                    }
                }
                return ip;
            }
        }

        public static bool NoNetwork
        {
            get{return Application.internetReachability==NetworkReachability.NotReachable;}
        }

        public abstract class ClientBase : IEnumerator
        {
            Socket m_client;

            public byte[] m_Buffer;
            public int m_BufferSize;

            public ConcurrentQueue<object> m_ReceiveQueue;

            public bool m_IsConnect
            {
                get
                {
                    if(m_client!=null && m_client.Connected)
                    {
                        return true;
                    }
                    return false;
                }
            }


            public void Connect(string _ip,int _port)
            {
                if(m_client!=null)
                {
                    m_client.Close();
                }
                m_client = new Socket(Net.IP.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

                IPAddress ip = IPAddress.Parse(_ip);
                m_client.BeginConnect(_ip,_port,connectResult,m_client);
            }

            void connectResult(IAsyncResult _ar)
            {
                var client = _ar.AsyncState as Socket;
                try
                {
                    client.EndConnect(_ar);
                }
                catch (System.Exception _e)
                {
                    Debug.LogError(_e.Message);
                    ConnectResult(false);
                    return;
                }
                ConnectResult(true);
                receive();
            }

            public abstract void ConnectResult(bool _succ);

            void receive()
            {
                m_client.BeginReceive(m_Buffer,m_BufferSize,m_Buffer.Length-m_BufferSize,SocketFlags.None,receiveResult,m_client);
            }

            void receiveResult(IAsyncResult _ar)
            {
                Socket client = _ar.AsyncState as Socket;
                int size=0;
                try
                {
                    size = client.EndReceive(_ar);
                }
                catch (System.Exception _e) 
                {
                    Debug.LogError(_e.Message);
                    return;
                }
                if(size>0)
                {
                    m_BufferSize+=size;
                    Decode();
                }
            }

            public abstract void Decode();

            public void Send(byte[] _data)
            {
                m_client.BeginSend(_data,0,_data.Length,SocketFlags.None,sendResult,m_client);
            }

            public void sendResult(IAsyncResult _ar)
            {
                Socket client = _ar.AsyncState as Socket;
                try
                {
                    client.EndSend(_ar);
                }
                catch (System.Exception _e)
                {
                    Debug.LogError(_e.Message);
                }
            }

            public object Current{get{return null;}}

            public bool MoveNext()
            {
                return true;
            }

            public void Reset()
            {
                
            }
        }
    }
}