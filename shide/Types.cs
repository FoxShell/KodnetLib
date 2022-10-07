/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 30/9/2022
 * Time: 05:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoxShell;

namespace Shide
{
	/// <summary>
	/// Description of Types.
	/// </summary>
	public class Exception: System.Exception{
		public string code;
		public Exception(string message): base(message){}
	}
	
	
	public class InvokeResult{
		public Exception Exception;
		public object Result;
	}
	
	public class Module: DynamicValue{
			
		internal Dictionary<string, object> funcs;
		public Module(){
			var dyn = this.Properties as DynamicPropsDictionary;
			funcs = dyn.Dictionary;

			funcs["Execute"] = new Func<object, object[], object>(InvokeMethod);
		}

		public object InvokeMethod(object target, object[] args){
			return CallSiteInvoker.invokeMethod(this, "Execute", args);
		}
		
		public  Func<object, object[], object> GetMethod(string method){
			return funcs[method] as Func<object, object[], object>;
		}		
		
		public Task<object> Execute(string method, params object[] parameters){
			return (Task<object>)this.GetMethod(method).Invoke(null, parameters);
		}
		
		public Task<object> Execute(string method){
			return (Task<object>)this.GetMethod(method).Invoke(null, new object[]{});
		}
		
	}
}
