﻿using System;
using System.Collections.Generic;
using System.Text;
using Scorpio;
using Scorpio.Userdata;
namespace Scorpio.Variable
{
    public abstract class ScorpioMethod
    {
        public UserdataMethod Method { get; protected set; }        //函数引用
        public string MethodName { get; protected set; }            //函数名字
        public abstract object Call(ScriptObject[] parameters);     //调用函数
        public abstract ScorpioMethod MakeGenericMethod(ScriptObject[] parameters); //声明泛型函数
    }
    //实例函数
    public class ScorpioObjectMethod : ScorpioMethod
    {
        private object m_Object;
        public ScorpioObjectMethod(object obj, string name, UserdataMethod method)
        {
            m_Object = obj;
            Method = method;
            MethodName = name;
        }
        public override object Call(ScriptObject[] parameters)
        {
            return Method.Call(m_Object, parameters);
        }
        public override ScorpioMethod MakeGenericMethod(ScriptObject[] parameters)
        {
            return new ScorpioObjectMethod(m_Object, MethodName, Method.MakeGenericMethod(parameters));
        }
    }
    //类函数
    public class ScorpioTypeMethod : ScorpioMethod
    {
        private Script m_script;
        //所在的类
        private Type m_Type;
        public ScorpioTypeMethod(Script script, string name, UserdataMethod method, Type type)
        {
            m_script = script;
            m_Type = type;
            Method = method;
            MethodName = name;
        }
        public override object Call(ScriptObject[] parameters)
        {
            int length = parameters.Length;
            Util.Assert(length > 0, m_script, "length > 0");
            if (length > 1) {
                ScriptObject[] pars = new ScriptObject[parameters.Length - 1];
                Array.Copy(parameters, 1, pars, 0, pars.Length);
                if (parameters[0] is ScriptNumber)
                    return Method.Call(Util.ChangeType_impl(parameters[0].ObjectValue, m_Type), pars);
                else
                    return Method.Call(parameters[0].ObjectValue, pars);
            } else {
                return Method.Call(parameters[0].ObjectValue, new ScriptObject[0]);
            }
        }
        public override ScorpioMethod MakeGenericMethod(ScriptObject[] parameters)
        {
            return new ScorpioTypeMethod(m_script, MethodName, Method.MakeGenericMethod(parameters), m_Type);
        }
    }
    //静态函数
    public class ScorpioStaticMethod : ScorpioMethod
    {
        public ScorpioStaticMethod(string name, UserdataMethod method)
        {
            Method = method;
            MethodName = name;
        }
        public override object Call(ScriptObject[] parameters)
        {
            return Method.Call(null, parameters);
        }
        public override ScorpioMethod MakeGenericMethod(ScriptObject[] parameters)
        {
            return new ScorpioStaticMethod(MethodName, Method.MakeGenericMethod(parameters));
        }
    }
}
