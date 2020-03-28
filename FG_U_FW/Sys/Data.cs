using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace FG_U_FW
{
    public class Data : ISys
    {
        ConcurrentDictionary<Type,DataBase> m_databases = new ConcurrentDictionary<Type, DataBase>();
        public void Clear()
        {
            
        }    
        
        public T DataBases<T>() where T:DataBase
        {
            var type = typeof(T);
            DataBase database = default;
            if(!m_databases.ContainsKey(type))
            {
                database = Activator.CreateInstance<T>();
                if(!m_databases.TryAdd(type,database))
                {
                    Debug.LogError("[Data.DataBase]添加失败");
                }
            }
            else
            {
                if(!m_databases.TryGetValue(type,out database))
                {
                    Debug.LogError("[Data.DataBase]获取失败");
                }

            }
            return database as T;
        }

        public void GC()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public abstract class DataBase
        {
            
            public string Name
            {
                get;
                protected set;
            }
            
            public int Count
            {
                get;
                protected set;
            }
            
            public DataBase(string _name,int _count)
            {
                Name = _name;
                Count = _count;
            }
        }

        public abstract class DBTable
        {
            public DBTable(string _name,int _count)
            {
                if(string.IsNullOrEmpty(_name))
                {
                    Name = this.GetType().Name;
                }
                else
                {
                    Name = _name;
                }
                Count = _count;
            }
            public string Name
            {
                get;
                protected set;
            }
            public virtual string ToInsertSql()
            {
                Type tableType = this.GetType();
                var fields = tableType.GetFields();

                StringBuilder sql_sb = new StringBuilder($"insert into {Name}(");
                for (int i = 0; i < fields.Length; i++)
                {
                    sql_sb.Append(fields[i].Name);
                    if(i!=fields.Length-1)
                    {
                        sql_sb.Append(",");
                    }
                }
                sql_sb.Append(") values(");

                for (int i = 0; i < fields.Length; i++)
                {
                    sql_sb.Append(fields[i].GetValue(this));
                    if(i!=fields.Length-1)
                    {
                        sql_sb.Append(",");
                    }
                }
                sql_sb.Append(")");
                return sql_sb.ToString();
            }

            public static string CreateSql<T>(string _tableName=null) where T : DBTable
            {
                Type tableType = typeof(T);
                if(_tableName==null)
                {
                    _tableName = tableType.Name;
                }
                var fields = tableType.GetFields();

                StringBuilder sql_sb = new StringBuilder($"create table {_tableName}(");
                for (int i = 0; i < fields.Length; i++)
                {
                    sql_sb.AppendFormat("{0} {1}",fields[i].Name,fields[i].FieldType.Name);
                    if(i!=fields.Length-1)
                    {
                        sql_sb.Append(",");
                    }
                }
                sql_sb.Append(")");
                
                return sql_sb.ToString();
            }

            public static string UpdateSql(string _tableName,int _whereIdx,params object[] _datas)
            {
                StringBuilder sql_sb = new StringBuilder($"update {_tableName} set ");
                for (int i = 0; i < _whereIdx; i++)
                {
                    if(i%2==0)//单数 是key
                    {
                        sql_sb.Append(_datas[i]);
                    }
                    else
                    {
                        sql_sb.AppendFormat("={0}",_datas[i]);
                        if(i!=_whereIdx-1)
                        {
                            sql_sb.Append(",");
                        }
                    }
                }
                
                if(_whereIdx < _datas.Length)
                {
                    sql_sb.Append(" where ");
                }

                for (int i = _whereIdx; i < _datas.Length; i++)
                {
                    if(i%2==0)//单数 是key
                    {
                        sql_sb.Append(_datas[i]);
                    }
                    else
                    {
                        sql_sb.AppendFormat("={0}",_datas[i]);
                        if(i!=_datas.Length-1)
                        {
                            sql_sb.Append(" and ");
                        }
                    }
                }
                return sql_sb.ToString();
            }

            public static string DelectsSql(string _tableName,bool _and_or=true,params object[] _datas)
            {
                StringBuilder sql_sb = new StringBuilder($"delete from {_tableName}");
                if(_datas.Length>0)
                {
                    sql_sb.Append(" where ");
                }
                for (int i = 0; i < _datas.Length; i++)
                {
                    if(i%2==0)//单数 是key
                    {
                        sql_sb.Append(_datas[i]);
                    }
                    else
                    {
                        sql_sb.AppendFormat("={0}",_datas[i]);
                        if(i!=_datas.Length-1)
                        {
                            if(_and_or)
                            {
                                sql_sb.Append(" and ");
                            }
                            else
                            {
                                sql_sb.Append(" or ");
                            }
                        }
                    }
                }
                return sql_sb.ToString();
            }
            
            public int Count
            {
                get;
                protected set;
            }
        }
    }
}