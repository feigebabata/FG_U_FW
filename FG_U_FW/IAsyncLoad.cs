using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FG_U_FW
{
    public interface IAsyncLoad<T>
    {
        void Load(string _uri,Action<T> _callback);
        void Cancel(string _uri,Action<T> _callback);
    }
}
