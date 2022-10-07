/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 29/9/2022
 * Time: 02:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;

namespace FoxShell
{
	/// <summary>
	/// Description of FieldDescriptor.
	/// </summary>
	public class FieldDescriptor
	{
		
		public FieldInfo fieldInfo;
		public string name;
		
		Func<object, object[], object> _Getter_p;
		Func<object, object[], object> _Setter_p;
		
		
		public Func<object, object[], object> Getter{
			get{
				if(_Getter_p == null){
					_Getter_p = new Func<object, object[], object>(__Getter);
				}
				return _Getter_p;
			}
		}
		
		public Func<object, object[], object> Setter{
			get{
				if(_Setter_p == null){
					_Setter_p = new Func<object, object[], object>(__Setter);
				}
				return _Setter_p;
			}
		}
		
		
		public object __Getter(object target, object[] args){
			return this.GetValue(target);
		}
		
		public object __Setter(object target, object[] args){
			this.SetValue(target, args[0]);
			return null;
		}
		
		
		public object GetValue(object o){
			if(o != null && Kodnet.current.UseCallSiteOnProps){
				return CallSiteInvoker.GetInvokerparam0P(name).invoke(o);
			}else{
				return this.fieldInfo.GetValue(o);
			}
		}
		
		public void SetValue(object o, object value){
			if(o != null && Kodnet.current.UseCallSiteOnProps){
				CallSiteInvoker.GetInvokerparam0P(name).setProperty(o, value);
			}
			else{
				this.fieldInfo.SetValue(o, value);
			}
		}
		
	}
}
