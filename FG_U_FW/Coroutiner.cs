using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FG_U_FW
{
    public class Coroutiner : MonoBehaviour
    {
        public static class Config
        {
            public const int MAX_IO_COUNT = 5;
        }

        public static int IOCount{get;private set;}
        static MonoBehaviour mb;
        static Queue<IEnumerator> ioQueue = new Queue<IEnumerator>();

        static void tryInit()
        {
            if(!mb)
            {
                mb=Main.I;
                IOCount=0;
            }
        }

        public static Coroutine Start(IEnumerator _ie)
        {
            tryInit();
            return mb.StartCoroutine(_ie);
        }

        public static void Stop(Coroutine _cor)
        {
            tryInit();
            mb.StopCoroutine(_cor);
        }

        public static void StartIO(IEnumerator _ie)
        {
            tryInit();
            ioQueue.Enqueue(_ie);
            updateIOQueue();
        }

        static void updateIOQueue()
        {
            while(ioQueue.Count>0 && IOCount<Config.MAX_IO_COUNT)
            {
                mb.StartCoroutine(io(ioQueue.Dequeue()));
            }
        }

        static IEnumerator io(IEnumerator _ie)
        {
            IOCount++;
            yield return _ie;
            IOCount--;
            updateIOQueue();
        }

        public static Coroutine Delay(float _time,Action _callback)
        {
            return delay(_time,_callback).Start();
        }

        static IEnumerator delay(float _time,Action _callback)
        {
            yield return new WaitForSeconds(_time);
            _callback?.Invoke();
        }

        public static Coroutine DelayByFrame(int _count,Action _callback)
        {
            if(_count>0)
            {
                return delayByFrame(_count,_callback).Start();
            }
            return null;
        }

        static IEnumerator delayByFrame(int _count,Action _callback)
        {
            for (int i = 0; i < _count; i++)
            {
                yield return new WaitForEndOfFrame();
            }
            _callback?.Invoke();
        }
    }

    public static class CoroutineExpand
    {
        public static Coroutine Start(this IEnumerator _ie)
        {
            return Coroutiner.Start(_ie);
        }
        
        public static void StartIO(this IEnumerator _ie)
        {
            Coroutiner.StartIO(_ie);
        }

        public static void Stop(this Coroutine _cor)
        {
            Coroutiner.Stop(_cor);
        }
    }
}
