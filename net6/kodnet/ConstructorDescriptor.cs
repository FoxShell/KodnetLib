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
	/// 
	
	
	public class ConstructorDescriptor
	{
		
		public List<ConstructorInfo> methods = new List<ConstructorInfo>();
		//public int maxParameterCount;
		public string name;
		
		Func<object, object[], object> _Invoker_p;
		
		
		public Func<object, object[], object> Invoker{
			get{
				if(_Invoker_p == null){
					_Invoker_p = new Func<object, object[], object>(__Invoker);
				}
				return _Invoker_p;
			}
		}
		
		
		public object __Invoker(object target, object[] args){
			return this.Invoke(args);
		}	
		
		
		
		public object Invoke(object[] args){
			object result = null;
			var method = GetMethodForParameters(args);
			if(method != null){
				result = method.Invoke(args);
			} 
			return result; 
		}
		
		
		public ConstructorInfo GetMethodForParameters(object[] parameters)
		{
			
			List<ConstructorInfo> list = new List<ConstructorInfo>(0);
			bool flag = false;
			for (int i = 0; i < this.methods.Count; i++)
			{
				ConstructorInfo methodBase = this.methods[i];
			
				ParameterInfo[] parameters2 = methodBase.GetParameters();
				
				if ((int)parameters2.Length == (int)parameters.Length)
				{
					bool flag3 = true;
					bool flag4 = false;
					for (int j = 0; j < (int)parameters2.Length; j++)
					{
						ParameterInfo parameterInfo = parameters2[j];
						
						if (parameters[j] == null)
						{
							flag4 = true;
						}
						else
						{
							Type type = parameters[j].GetType();
							
							if (!parameterInfo.ParameterType.IsAssignableFrom(type))
							{
								flag3 = false;
							}
						}
					}
					if (flag3)
					{
						list.Add(methodBase);
						i = 99999;
						flag = (flag ? true : flag4 & flag3);
					}
				}
				
			}
			if (flag && list.Count > 1 || list.Count == 0)
			{
				throw new Exception("No se puede determinar la mejor coincidencia para la ejecución del método.");
			}
			return list[0];
		}
		
	}
}
