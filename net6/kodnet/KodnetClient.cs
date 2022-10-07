
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FoxShell
{
    public class KodnetClient
    {

        public object value;

        static invokerparam2 setVar = CallSiteInvoker.GetInvokerparam2("SetVar");
        static invokerparam2 getProperty = CallSiteInvoker.GetInvokerparam2("Proxy_GetProperty");
        static invokerparam3 setProperty = CallSiteInvoker.GetInvokerparam3("Proxy_SetProperty");
        static invokerparam3 execute = CallSiteInvoker.GetInvokerparam3("Execute");
        static invokerparam1 getRealReference = CallSiteInvoker.GetInvokerparam1("GetRealReference");

        static invokerparam1 proxyUnref = CallSiteInvoker.GetInvokerparam1("ProxyUnref");
        static invokerparam0 processId = CallSiteInvoker.GetInvokerparam0("ProcessId");
        static invokerparam1 doCmd = CallSiteInvoker.GetInvokerparam1("DoCmd");
        static invokerparam1 throwError = CallSiteInvoker.GetInvokerparam1("ThrowError");

        public IntPtr iunk;
        public KodnetClient(object vfp)
        {

            //int num = (int)getRealReference.invoke(vfp, vfp);
            //iunk = new IntPtr(num);
            //value = Marshal.GetObjectForIUnknown(iunk);           
            iunk = Marshal.GetIDispatchForObject(vfp);
            value = vfp;

        }

        public KodnetClient(int vfp)
        {

            iunk = new IntPtr(vfp);
            value = Marshal.GetObjectForIUnknown(iunk);
        }


        public void SetVar(string name, object value)
        {
            //value = Marshal.GetObjectForIUnknown(iunk);   
            setVar.invoke(this.value, name, value);
        }
        public int ProcessId()
        {
            //value = Marshal.GetObjectForIUnknown(iunk);   
            return (int)processId.invoke(value);
        }
        public object DoCmd(string cmd)
        {
            //value = Marshal.GetObjectForIUnknown(iunk);   
            return doCmd.invoke(value, cmd);
        }
        public void ThrowError(object e)
        {

            //value = Marshal.GetObjectForIUnknown(iunk);       
            throwError.invoke(value, e);
        }

        public void ProxyUnref(string id)
        {
            //value = Marshal.GetObjectForIUnknown(iunk);       
            proxyUnref.invoke(value, id);
        }

        public void ProxyUnref(int id)
        {
            //value = Marshal.GetObjectForIUnknown(iunk);       
            proxyUnref.invoke(value, id.ToString());
        }


        public object Execute(string target, string method, object args)
        {

            //value = Marshal.GetObjectForIUnknown(iunk);   
            return execute.invoke(value, target, method, args);
        }

        public object Proxy_GetProperty(string target, string name)
        {

            //value = Marshal.GetObjectForIUnknown(iunk);   
            return getProperty.invoke(value, target, name);
        }

        public object Proxy_SetProperty(string target, string name, object value)
        {

            //value = Marshal.GetObjectForIUnknown(iunk);   
            return setProperty.invoke(this.value, target, name, value);
        }
    }
}