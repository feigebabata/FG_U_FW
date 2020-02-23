using System;
using UnityEngine;

namespace FG_U_FW
{
    public class Tex2DLoad : OnlyAsyncWait<Texture2D>,ISys
    {
        public void Clear()
        {
        }

        public void Init()
        {
        }

        protected override void addWait(string _url)
        {
            Debug.LogFormat("[Tex2DLoad.addWait] {0}",_url);
        }

        protected override void removeWait(string _url)
        {
            Debug.LogFormat("[Tex2DLoad.removeWait] {0}",_url);
        }
    }
}