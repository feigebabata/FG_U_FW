using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace FG_U_FW
{
    public class Net : ISys
    {
        public static class Config
        {
            public const int BUFFER_MAX_SIZE=1024*16;
        }

        Hashtable m_clientTab = new Hashtable();
        Hashtable m_serverTab = new Hashtable();
        public void Clear()
        {
            var ie = m_clientTab.Values.GetEnumerator();
            while(ie.MoveNext())
            {
                (ie.Current as ClientBase).Close();
            }
            ie = m_serverTab.Values.GetEnumerator();
            while(ie.MoveNext())
            {
                (ie.Current as ServerBase).Close();
            }
            m_clientTab.Clear();
            m_serverTab.Clear();
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

        public T Server<T>() where T:ServerBase
        {
            var type = typeof(T);
            if(!m_serverTab.Contains(type))
            {
                T server = Activator.CreateInstance<T>();
                m_serverTab.Add(type,server);
            }
            return m_serverTab[type] as T;
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

        public enum ClientState
        {
            None,
            Connecting,
            Connected,
            Disconnected,
            Closed, 
        }

        public enum SocketResult
        {
            Succ,
            SendFail,
            ReceiveFail,
            ConnectFail,
        }

        public class NetMsgQueue : IEnumerator
        {
            Queue<object> m_msgQueue = new Queue<object>();
            public object Current
            {
                get
                {
                    if(m_msgQueue.Count>0)
                    {
                        return m_msgQueue.Dequeue();
                    }
                    return null;
                }
            }

            public bool MoveNext()
            {
                return m_msgQueue.Count==0;
            }

            public void Reset()
            {
                
            }

            public void Enqueue(object _msg)
            {
                m_msgQueue.Enqueue(_msg);
            }
        }

        public abstract class ClientBase
        {
            Socket m_client;

            protected byte[] m_buffer = new byte[Net.Config.BUFFER_MAX_SIZE];
            protected int m_bufferSize;

            protected NetMsgQueue m_receiveQueue = new NetMsgQueue();
            protected object m_msgQueueLock = new object(); 

            public ClientState State;


            public virtual void Connect(string _ip,int _port)
            {
                State=ClientState.None;
                if(!Net.NoNetwork)
                {
                    m_client = new Socket(Net.IP.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

                    State = ClientState.Connecting;
                    m_client.BeginConnect(_ip,_port,new AsyncCallback(connectResult),m_client);
                }
                else
                {
                    ConnectResult(false);
                    
                    error(SocketResult.ConnectFail,"无网络");
                }
            }

            void connectResult(IAsyncResult _ar)
            {
                var client = _ar.AsyncState as Socket;
                try
                {
                    client.EndConnect(_ar);
                    State = ClientState.Connected;
                }
                catch (System.Exception _e)
                {
                    ConnectResult(false);
                    error(SocketResult.ConnectFail,_e.Message);
                    return;
                }
                ConnectResult(true);
                receive();
            }

            protected abstract void ConnectResult(bool _succ);

            void receive()
            {
                if(State==ClientState.Connected)
                {
                    m_client.BeginReceive(m_buffer,m_bufferSize,m_buffer.Length-m_bufferSize,SocketFlags.None,new AsyncCallback(receiveResult),m_client);
                }
            }

            void receiveResult(IAsyncResult _ar)
            {
                if(m_client!=null && m_client.Connected)
                {
                    Socket client = _ar.AsyncState as Socket;
                    int size=0;
                    try
                    {
                        size = client.EndReceive(_ar);
                    }
                    catch (System.Exception _e) 
                    {
                        error(SocketResult.ReceiveFail,_e.Message);
                        return;
                    }
                    if(size>0)
                    {
                        m_bufferSize+=size;
                        Decode();
                        receive();
                    }
                }
            }

            protected abstract void Decode();

            protected void Send(byte[] _data)
            {
                if(State==ClientState.Connected)
                {
                    m_client.BeginSend(_data,0,_data.Length,SocketFlags.None,new AsyncCallback(sendResult),m_client);
                }
            }

            void sendResult(IAsyncResult _ar)
            {
                Socket client = _ar.AsyncState as Socket;
                try
                {
                    client.EndSend(_ar);
                }
                catch (System.Exception _e)
                {
                    error(SocketResult.SendFail,_e.Message);
                }
            }

            void error(SocketResult _result,string _msg)
            {
                Debug.LogErrorFormat("[ClientBase.error] state = {2} , result = {0} , msg = {1}",_result,_msg,State);
                if(State!=ClientState.Closed)
                {
                    OnError(_result,_msg);
                }
            }

            protected virtual void OnError(SocketResult _result,string _msg)
            {
                State=ClientState.Disconnected;
                disconnect();
            }


            public virtual void Close()
            {
                State=ClientState.Closed;
                disconnect();
            }

            void disconnect()
            {
                if(m_client!=null)
                {
                    try
                    {
                        m_client.Shutdown(SocketShutdown.Both);
                        m_client.Close();
                    }
                    catch(Exception _e)
                    {
                        
                    }
                    m_client=null;
                }
                m_bufferSize=0;
            }
        }

        public abstract class ServerBase
        {
            TcpListener m_listener;
            public List<Socket> m_clients = new List<Socket>();
            public ConcurrentDictionary<Socket,byte[]> m_Buffers = new ConcurrentDictionary<Socket, byte[]>();
            public ConcurrentDictionary<Socket,int> m_BufferSizes = new ConcurrentDictionary<Socket, int>();
            public void Start(int _port)
            {
                m_listener = new TcpListener(Net.IP,_port);
                m_listener.Start();
                accept();
                StartFinish(true);
            }

            protected abstract void StartFinish(bool _succ);

            void accept()
            {
                m_listener.BeginAcceptSocket(acceptResult,m_listener);
            }

            void acceptResult(IAsyncResult _ar)
            {
                if(m_listener!=null && m_listener.Server!=null)
                {
                    Socket client = null;
                    try
                    {
                        client = m_listener.EndAcceptSocket(_ar);
                    }
                    catch (System.Exception _e)
                    {
                        OnError(client,_e.Message);
                        return;
                    }
                    m_clients.Add(client);
                    m_Buffers.TryAdd(client,new byte[Net.Config.BUFFER_MAX_SIZE]);
                    m_BufferSizes.TryAdd(client,0);
                    accept();
                    reveive(client);
                }
            }

            protected virtual void OnError(Socket _client,string _msg)
            {
                Debug.LogError(_msg);
                if(_client!=null)
                {
                    _client.Shutdown(SocketShutdown.Both);
                    _client.Close();
                    m_clients.Remove(_client);
                }
            }

            void reveive(Socket _client)
            {
                byte[] buffer=null;
                int size = 0;
                m_Buffers.TryGetValue(_client,out buffer);
                m_BufferSizes.TryGetValue(_client,out size);
                _client.BeginReceive(buffer,size,buffer.Length-size,SocketFlags.None,receiveResult,_client);
            }

            void receiveResult(IAsyncResult _ar)
            {
                Socket client = _ar.AsyncState as Socket;
                if(client!=null && client.Connected)
                {
                    int size = 0;
                    try
                    {
                        size = client.EndReceive(_ar);
                    }
                    catch (System.Exception _e)
                    {
                        OnError(client,_e.Message);
                        return;
                    }
                    if(size>0)
                    {
                        int val = 0;
                        m_BufferSizes.TryGetValue(client,out val);
                        m_BufferSizes.TryUpdate(client,val+size,val);
                        Decode(client);
                    }
                    reveive(client);
                }
            }

            public void Send(Socket _client,byte[] _data,int _offset,int _length)
            {
                _client.BeginSend(_data,_offset,_length,SocketFlags.None,sendResult,_client);
            }

            void sendResult(IAsyncResult _ar)
            {
                Socket client = _ar.AsyncState as Socket;
                try
                {
                    client.EndSend(_ar);
                }
                catch (System.Exception _e)
                {
                    OnError(client,_e.Message);
                }
            }

            public void SendAll(byte[] _data,int _offset,int _length)
            {
                m_clients.ForEach((_client)=>
                {
                    Send(_client,_data,_offset,_length);
                });
            }

            

            protected abstract void Decode(Socket _client);
            
            public void Close()
            {
                var ie = m_clients.GetEnumerator();
                while(ie.MoveNext())
                {
                    ie.Current.Shutdown(SocketShutdown.Both);
                    ie.Current.Close();
                }
                m_listener?.Stop();
                m_listener=null;
                

                m_clients.Clear();
                m_Buffers.Clear();
                m_BufferSizes.Clear();
            }

        }
    }
}