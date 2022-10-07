/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 29/9/2022
 * Time: 02:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FoxShell
{
	/// <summary>
	/// Description of FieldDescriptor.
	/// </summary>
	public class PropertyDescriptor
	{
		
		public List<PropertyInfo> properties = new List<PropertyInfo>();
		//public int maxParameterCount;
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
			return this.GetValue(target, args);
		}
		
		public object __Setter(object target, object[] args){
			var value = args[args.Length -1];
			var nargs = new object[args.Length - 1];
			Array.Copy(args, 0, nargs, 0, args.Length -1);
			this.SetValue(target, value, nargs);
			return null;
		}
		
		
		public object GetValue(object target, object[] args){
			
			
			if((target != null)  && Kodnet.current.UseCallSiteOnProps){
				return CallSiteInvoker.invokeProperty(target, name, args);
			}
			else{
				object result = null;
				var prop = GetPropertyForParameters(args);
				if(prop != null){
					result = prop.GetValue(target, args);
				}
				return result; 
			}
			
			
		}
		
		public void SetValue(object target, object value, object[] args){
			
			
			if((target != null) && Kodnet.current.UseCallSiteOnProps){
				CallSiteInvoker.invokePropertySet(target, name, value, args);
			}
			else{
				var prop = GetPropertyForParameters(args);
				if(prop != null){
					prop.SetValue(target, value, args);
				} 
			}
			
			
		}
		
		public PropertyInfo GetPropertyForParameters(object[] parameters)
		{
			
			List<PropertyInfo> list = new List<PropertyInfo>(0);
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PropertyInfo property in this.properties)
			{
				stringBuilder.Length = 0;
				ParameterInfo[] indexParameters = property.GetIndexParameters();
				if ((int)indexParameters.Length != (int)parameters.Length)
				{
					continue;
				}
				bool flag2 = true;
				bool flag3 = false;
				for (int i = 0; i < (int)indexParameters.Length; i++)
				{
					ParameterInfo parameterInfo = indexParameters[i];
					if (parameters[i] == null)
					{
						flag3 = true;
					}
					else if (!parameterInfo.ParameterType.IsAssignableFrom(parameters[i].GetType()))
					{
						stringBuilder.Append(",").Append(parameterInfo.ParameterType.ToString());
						flag2 = false;
					}
				}
				if (!flag2)
				{
					continue;
				}
				list.Add(property);
				flag = (flag ? true : flag3 & flag2);
			}
			if (flag && list.Count > 0 || list.Count == 0)
			{
				string str = "";
				if (stringBuilder.Length > 0)
				{
					str = string.Concat("Una de las sobrecargas admite estos tipos es: ", stringBuilder.ToString());
				}
				throw new Exception(string.Concat("No se puede determinar la mejor coincidencia para la ejecución de la propiedad. ", str));
			}
			return list[0];
		}
		
	}
}
