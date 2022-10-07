/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 29/9/2022
 * Time: 20:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace FoxShell
{


    public class Proxy
    {
        public int id = 0;
        bool disposed = false;


        public Proxy(int id)
        {
            this.id = id;
        }

        public object InvokeMethod(string method, object[] args)
        {
            return Kodnet.current.VFP.Execute(id.ToString(), method, TypeDescriptor.GetWrappedIfRequired(args));
        }

        public object GetDelegate(string method)
        {
            return new DelegateProxy(id, method);
        }

        public void Free()
        {
            if (Kodnet.current.VFP != null && !this.disposed)
            {
                Kodnet.current.VFP.ProxyUnref(this.id);
                disposed = true;
            }
        }

        public void Dispose()
        {
            Free();
        }

        public object GetProperty(string name)
        {
            return Kodnet.current.VFP.Proxy_GetProperty(id.ToString(), name);
        }

        public object SetProperty(string name, object value)
        {
            return Kodnet.current.VFP.Proxy_SetProperty(id.ToString(), name, TypeDescriptor.GetWrappedIfRequired(value));
        }
    }


    public class DelegateProxy : Proxy
    {
        public string method;

        public DelegateProxy(int id, string method) : base(id)
        {
            this.method = method;
        }
        public object Invoke(object[] args)
        {
            return this.InvokeMethod(method, args);
        }

    }


    /// <summary>
    /// Description of DelegateWrapper.
    /// </summary>
    public class DelegateWrapperStatic
    {
        internal Type type;
        internal MethodInfo method;

        //private static ModuleBuilder m_module;


        public DelegateWrapperStatic(Type t)
        {
            type = t;
            method = t.GetMethod("Invoke");
        }

        Func<object, object[], object> _Invoker_p;

        public Func<object, object[], object> Invoker
        {
            get
            {
                if (_Invoker_p == null)
                {
                    _Invoker_p = new Func<object, object[], object>(__Construct);
                }
                return _Invoker_p;
            }
        }


        public object __Construct(object target, object[] args)
        {
            object target1 = null;
            string method = "";


            if (args.Length > 0)
            {
                target1 = args[0];
                var delegateproxy = target1 as DelegateProxy;
                if (delegateproxy != null)
                {
                    return Create(delegateproxy);
                }
            }
            if (args.Length > 1)
            {
                method = args[1].ToString();
            }

            return Create(target1, method);
        }

        public DelegateWrapper Create(object target, string method)
        {

            var w = new DelegateWrapper(this);
            w.target = target;
            w.method = method;
            w.CreateDelegateType();
            return w;
        }


        public DelegateWrapper Create(DelegateProxy ptr)
        {

            var w = new DelegateWrapper(this);
            w.proxy = ptr;
            w.CreateDelegateType();
            return w;
        }

        /*
         private string GetUniqueName(string nameBase)
        {
            int number = 2;
            string name = nameBase;
            while (m_module.GetType(name) != null)
                name = nameBase + number++;
            return name;
        }
             
        
        public object Compile(){
            
            string name = GetUniqueName(type.ToString().Replace('.', '-'));

            var typeBuilder = m_module.DefineType(
                name, TypeAttributes.Public, typeof(DelegateWrapper));
            
            
            
            List<Type> list = new List<Type>();
            var parameters = method.GetParameters();
            foreach(var par in parameters){
                list.Add(par.ParameterType);
            }
            var methodBuilder = typeBuilder.DefineMethod("DelegateInvoke", MethodAttributes.Public, method.ReturnType, list.ToArray()); 
        }
        */

    }

    public class DelegateWrapper
    {

        DelegateWrapperStatic staticWrapper;
        internal object target;
        internal string method;
        internal DelegateProxy proxy;
        public static MethodInfo invoker = typeof(DelegateWrapper).GetMethod("Invoke");
        public Delegate function;

        public DelegateWrapper()
        {

        }

        public DelegateWrapper(DelegateWrapperStatic t)
        {
            staticWrapper = t;
        }

        public object Invoke(object[] args)
        {

            try
            {

                object result = null;
                if (proxy != null)
                {
                    result = proxy.Invoke(args);
                }
                else
                {
                    result = CallSiteInvoker.invokeMethod(target, method, args);
                }

                return TypeDescriptor.GetWrappedIfRequired(result);

            }
            catch (Exception e)
            {
                if (Kodnet.current != null)
                {
                    Kodnet.current.exception = e;

                    if (Kodnet.current.VFP != null)
                    {
                        //vfp.lastException  = e;
                        //vfp.throwError(TypeDescriptor.GetWrappedIfRequired(e));
                        Kodnet.current.VFP.ThrowError(TypeDescriptor.GetWrappedIfRequired(e));
                    }
                    return null;
                }


                throw e;
            }
        }

        public void Free()
        {
            target = null;
            method = "";
            if (proxy != null)
            {
                proxy.Free();
            }
        }


        public void CreateDelegateType()
        {
            Type delegateType = staticWrapper.type;
            MethodInfo method = staticWrapper.method;
            var parameters = method.GetParameters();
            List<ParameterExpression> params1 = new List<ParameterExpression>();
            List<Expression> params2 = new List<Expression>();
            foreach (var par in parameters)
            {
                var p = Expression.Parameter(par.ParameterType);
                params1.Add(p);
                params2.Add(Expression.Convert(p, typeof(object)));

            }

            var objectArgs = Expression.NewArrayInit(typeof(object), params2.ToArray());
            var thisValue = Expression.Constant(this);
            var call = Expression.Call(thisValue, invoker, objectArgs);

            Expression end = call;
            if (method.ReturnType != typeof(void))
            {


                //LabelTarget label = Expression.Label(method.ReturnType);
                //var labelexp = Expression.Label(label, method.ReturnType);

                var callc = Expression.Convert(call, method.ReturnType);
                var label = Expression.Label(method.ReturnType);
                var labelexp = Expression.Label(label, callc);
                end = labelexp;
            }

            var block = Expression.Block(method.ReturnType, objectArgs, end);
            var expr = Expression.Lambda(delegateType, block, params1.ToArray());

            function = expr.Compile();
        }

    }
}
