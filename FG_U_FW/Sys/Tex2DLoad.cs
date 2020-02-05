using System;
using UnityEngine;

namespace FG_U_FW
{
    public class Tex2DLoad : IAsyncLoad<Texture2D>,ISys
    {
        public void Cancel(string _uri, Action<Texture2D> _callback)
        {
        }

        public void Clear()
        {
        }

        public void Init()
        {
        }

        public void Load(string _uri, Action<Texture2D> _callback)
        {
            Debug.LogFormat("tex2D.load {0} {1}",_uri,_callback);
        }
    }
}