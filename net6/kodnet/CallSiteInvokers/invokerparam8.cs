using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FoxShell
{
	public class invokerparam8 
	{
		private string method = "";

		protected CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>> invoker;

		protected CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>> invoker_p;

		protected CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>> invoker_v;

		private bool isProperty;

		public invokerparam8(string met)
		{
			this.method = met;
		}

		public invokerparam8(string met, bool isProperty)
		{
			this.method = met;
			this.isProperty = isProperty;
		}

		private void ensureInvoker()
		{
			if (this.invoker == null)
			{
				if (!this.isProperty)
				{
					this.invoker = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, this.method, null, typeof(CallSiteInvoker), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
					return;
				}
				this.invoker = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(Binder.GetIndex(CSharpBinderFlags.None, typeof(CallSiteInvoker), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
			}
		}

		private void ensureInvokerP()
		{
			if (this.invoker_p == null)
			{
				this.invoker_p = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>>.Create(Binder.SetIndex(CSharpBinderFlags.None, typeof(CallSiteInvoker), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
			}
		}

		private void ensureInvokerVoid()
		{
			if (this.invoker_v == null)
			{
				this.invoker_v = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, this.method, null, typeof(CallSiteInvoker), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
			}
		}

		public object invoke(object obj, object arg, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			this.ensureInvoker();
			object obj2 = this.invoker.Target(this.invoker, obj, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			if (obj2 == null)
			{
				return null;
			}
			return obj2;
		}

		public void invokeasVoid(object obj, object arg, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			this.ensureInvokerVoid();
			this.invoker_v.Target(this.invoker_v, obj, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		public object setProperty(object obj, object arg, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object value)
		{
			this.ensureInvokerP();
			return this.invoker_p.Target(this.invoker_p, obj, arg, arg2, arg3, arg4, arg5, arg6, arg7, arg8, value);
		}
	}
}