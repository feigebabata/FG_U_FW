﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FG_U_FW
{
    public class UI : ISys 
    {
        public static class Config
        {
            /// <summary>
            /// 隐藏界面时的位置
            /// </summary>
            /// <returns></returns>
            public static readonly Vector3 HidePos = new Vector3(3000,0,0);
        }

        /// <summary>
        /// ui界面栈记录
        /// </summary>
        /// <returns></returns>
        UIStack m_uiStack = new UIStack();

        /// <summary>
        /// ui界面回收池
        /// </summary>
        /// <returns></returns>
        UIPool m_uiPool = new UIPool();  

        /// <summary>
        /// ui界面父节点
        /// </summary>      
        Transform m_panelParent;

        /// <summary>
        /// ui界面ab文件 加载后不卸载 只在Clear的时候卸载
        /// </summary>
        AssetBundle m_panelAB;

        /// <summary>
        /// 清空所有ui界面
        /// </summary>
        public void Clear()
        {
            while(m_uiStack.Peek())
            {
                var ui = m_uiStack.Pop();
                destroy(ui);
            }

            if(m_panelAB)
            {
                m_panelAB.Unload(true);
                m_panelAB=null;
            }
            m_uiPool.Clear();
            m_uiStack.Clear();
            destroyAll();
            m_panelParent=null;
            Resources.UnloadUnusedAssets();
        }
        
        public void Update()
        {
            //监听系统返回键
            if(Input.GetKeyDown(KeyCode.Escape) && m_uiStack.Count>0)
            {
                m_uiStack.Peek().OnClickBack();
            }
        }

        /// <summary>
        /// 关闭当前界面
        /// </summary>
        public void CloseCurUI()
        {
            destroy(m_uiStack.Pop());
            if(m_uiStack.Count>0)
            {
                show(m_uiStack.Peek(),null);
            }
        }

        public void Open<T>(OpenMode _mode=OpenMode.Open,object _data=null,Action _finish=null) where T : UIBase
        {
            open<T>(_mode,_data,_finish).Start();
        }

        IEnumerator open<T>(OpenMode _mode=OpenMode.Open,object _data=null,Action _finish=null)
        {
            if(!m_panelParent)
            {
                if(!GameObject.Find("UIRoot/Canvas/Panels"))
                {
                    PrefabLoader loader = new PrefabLoader("UIRoot");
                    yield return loader;
                    GameObject uiRoot = loader.Prefab;
                    uiRoot.name="UIRoot";
                    uiRoot.transform.position=Vector3.zero;
                }
                m_panelParent = GameObject.Find("UIRoot/Canvas/Panels").transform;
            }

            Type newUIType = typeof(T);
            if(m_uiStack.Count>0)
            {
                switch(_mode)
                {
                    case OpenMode.Open:
                    {
                        if(m_uiStack.Peek().GetType()!=newUIType)
                        {
                            unfocus(m_uiStack.Peek());
                            hide(m_uiStack.Peek());
                            yield return create(newUIType);
                            show(m_uiStack.Peek(),_data);
                            focus(m_uiStack.Peek());
                        }
                    }
                    break;
                    case OpenMode.Top:
                    {
                        if(m_uiStack.Peek().GetType()!=newUIType)
                        {
                            unfocus(m_uiStack.Peek());
                            hide(m_uiStack.Peek());
                            UIBase ui = m_uiStack.Find((_ui)=>{return _ui.GetType()==newUIType;});
                            if(ui)
                            {
                                m_uiStack.Remove(ui);
                                m_uiStack.Push(ui);
                            }
                            else
                            {
                                yield return create(newUIType);
                            }
                            show(ui,_data);
                            focus(ui);
                        }
                    }
                    break;
                    case OpenMode.Overlay:
                    {
                        unfocus(m_uiStack.Peek());
                        yield return create(newUIType);
                        show(m_uiStack.Peek(),_data);
                        focus(m_uiStack.Peek());
                    }
                    break;
                    case OpenMode.Back:
                    {
                        if(m_uiStack.Peek().GetType()!=newUIType)
                        {
                            if(m_uiStack.Find((_ui)=>{return _ui.GetType()==newUIType;})==null)
                            {
                                unfocus(m_uiStack.Peek());
                                hide(m_uiStack.Peek());
                                yield return create(newUIType);
                                show(m_uiStack.Peek(),_data);
                                focus(m_uiStack.Peek());
                            }
                            else
                            {
                                while(m_uiStack.Peek().GetType()!=newUIType)
                                {
                                    unfocus(m_uiStack.Peek());
                                    hide(m_uiStack.Peek());
                                    destroy(m_uiStack.Pop());
                                }
                                UIBase ui = m_uiStack.Peek();
                                show(ui,_data);
                                focus(ui);
                            }
                        }
                    }
                    break;
                }
            }
            else
            {
                yield return create(newUIType);
                show(m_uiStack.Peek(),_data);
                focus(m_uiStack.Peek());
            }
            _finish?.Invoke();
        }

        void focus(UIBase _ui)
        {
            if(_ui.m_State == CycleState.Focus)
            {
                return;
            }
            _ui.m_State = CycleState.Focus;
            _ui.OnFocus();
        }

        void unfocus(UIBase _ui)
        {
            if(_ui.m_State == CycleState.UnFocus)
            {
                return;
            }
            _ui.m_State = CycleState.UnFocus;
            _ui.OnUnFocus();
        }

        /// <summary>
        /// 创建ui界面 优先从回收池中获取
        /// </summary>
        /// <param name="_uiType"></param>
        /// <param name="_data"></param>
        IEnumerator create(Type _uiType)
        {
            UIBase ui = m_uiPool.Pull(_uiType);
            if(ui==null)
            {
                PrefabLoader loader = new PrefabLoader(_uiType.Name);
                yield return loader;
                loader.Prefab.transform.SetParent(m_panelParent,false);
                ui = loader.Prefab.GetComponent<UIBase>();
                ui.gameObject.name = _uiType.Name;
            }
            ui.gameObject.SetActive(true);
            ui.transform.localPosition = Config.HidePos;
            m_uiStack.Push(ui);
            ui.OnCreate();
            ui.m_State = CycleState.Create;
        }

        /// <summary>
        /// 隐藏界面 ui位置移出视图之外 活动状态仍未true
        /// </summary>
        /// <param name="_ui"></param>
        void hide(UIBase _ui)
        {
            if(_ui.m_State == CycleState.Hide)
            {
                return;
            }
            _ui.OnHide();
            _ui.m_State = CycleState.Hide;
            _ui.transform.localPosition = Config.HidePos;
        }

        /// <summary>
        /// 移除界面 活动状态设为false 收入回收池
        /// </summary>
        /// <param name="_ui"></param>
        void destroy(UIBase _ui)
        {
            _ui.OnDestroy();
            _ui.m_State = CycleState.Destroy;
            _ui.gameObject.SetActive(false);
            m_uiPool.Push(_ui);

        }


        /// <summary>
        /// 显示界面 ui界面移至视图内
        /// </summary>
        /// <param name="_ui"></param>
        /// <param name="_data"></param>
        void show(UIBase _ui,object _data)
        {
            if(_ui.m_State==CycleState.Show)
            {
                return;
            }
            _ui.transform.SetSiblingIndex(m_panelParent.childCount-1);
            _ui.transform.localPosition = Vector3.zero;
            _ui.OnShow(_data);
            _ui.m_State = CycleState.Show;
        }

        /// <summary>
        /// 删除所有ui界面的Gameobject
        /// </summary>
        void destroyAll()
        {
            if(m_panelParent)
            {
                var desArr = new GameObject[m_panelParent.childCount];
                for (int i = 0; i < m_panelParent.childCount; i++)
                {
                    desArr[i] = m_panelParent.GetChild(i).gameObject;
                }
                for (int i = 0; i < desArr.Length; i++)
                {
                    GameObject.Destroy(desArr[i]);
                }
            }
        }

        class PrefabLoader : IEnumerator
        {
            public object Current => null;

            public GameObject Prefab;

            public PrefabLoader(string _path)
            {

            }

            public bool MoveNext()
            {
                return Prefab;
            }

            public void Reset(){}
        }

        /// <summary>
        /// ui界面基类 子类名需要和UI预制件名称相同
        /// </summary>
        public class UIBase : MonoBehaviour,ICycle
        {
            [HideInInspector]
            public CycleState m_State;

            /// <summary>
            /// 系统返回键和UI返回键的响应 按需求在子类中可自定义响应逻辑
            /// </summary>
            public virtual void OnClickBack()
            {
                Main.I.Sys<UI>().CloseCurUI();
            }

            public virtual void OnCreate()
            {

            }

            public virtual void OnShow(object _data)
            {
                
            }

            public virtual void OnHide()
            {
                
            }

            public virtual void OnDestroy()
            {
                
            }

            public void OnFocus()
            {
                
            }

            public void OnUnFocus()
            {
                
            }
        }



        public enum OpenMode
        {
            /// <summary>
            /// 打开界面在栈顶则忽略 否则新建界面并隐藏栈顶界面 默认打开模式
            /// </summary>
            Open,

            /// <summary>
            /// 新建界面不隐藏栈顶界面 常用于弹窗之类
            /// </summary>
            Overlay,

            /// <summary>
            /// 打开界面在栈顶则忽略 栈中无则新建否则弹栈至打开界面
            /// </summary>
            Back,

            /// <summary>
            /// 打开界面在栈顶则忽略 栈中无则新建否则将打开界面移至栈顶
            /// </summary>
            Top,
        }


        public class UIPool
        {
            List<UIBase> m_uiList = new List<UIBase>();

            public void Push(UIBase _ui)
            {
                m_uiList.Add(_ui);
            }

            public UIBase Pull(Type type)
            {
                var ui = m_uiList.Find((_ui)=>{return _ui.GetType()==type;});
                if(ui)
                {
                    m_uiList.Remove(ui);
                }
                return ui;
            }
            
            public int Count
            {
                get
                {
                    return m_uiList.Count;
                }
            }

            public void Clear()
            {
                m_uiList.Clear();
            }
        }
        
        public class UIStack
        {
            List<UIBase> m_uiList = new List<UIBase>();
            
            public int Count
            {
                get
                {
                    return m_uiList.Count;
                }
            }

            public void Clear()
            {
                m_uiList.Clear();
            }

            public void Push(UIBase _ui)
            {
                // Debug.Log("pop "+_ui.GetType());
                m_uiList.Insert(0,_ui);
            }

            public UIBase Peek()
            {
                if(Count>0)
                {
                    return m_uiList[0];
                }
                return null;
            }

            public UIBase Pop()
            {
                if(Count>0)
                {
                    var ui = m_uiList[0];
                    m_uiList.RemoveAt(0);
                    // Debug.Log("pop "+ui.GetType());
                    return ui;
                }
                return null;
            }

            public bool Remove(UIBase _ui)
            {
                return m_uiList.Remove(_ui);
            }

            public UIBase Find(Predicate<UIBase> _match)
            {
                return m_uiList.Find(_match);
            }
        }
    }


}