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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace FoxShell
{
	/// <summary>
	/// Description of FieldDescriptor.
	/// </summary>
	/// 
	
	public class MethodTypeArguments{
		
		internal List<Type> TypeArguments = new List<Type>();
		public MethodTypeArguments(IEnumerable<object> typeArguments){
			foreach(var arg in typeArguments){
				TypeArguments.Add((Type) arg);
			}
		}
		
	}
	
	
	public class MethodInformation{
		public MethodInfo method; 
		public bool hasRestParameter;
		public int index;
		
		public Func<object, object[], object> Invoker;
	}
	
	public class MethodDescriptor
	{
		
		public List<MethodInfo> methods = new List<MethodInfo>();
		List<Func<object, object[], object>> compiled = new List<Func<object, object[], object>>();
		public string name;
		public bool hidden = false;
		
		internal bool callsiteCanInvoke = false;
		internal bool callsiteVoid = false;
		internal Type type;		
		Func<object, object[], object> _Invoker_p;
		List<MethodInformation> methodsInfo;
		
		
		static MethodInfo CreateExceptionMethod = typeof(MethodDescriptor).GetMethod("CreateException");
		
		public Func<object, object[], object> Invoker{
			get{
				if(_Invoker_p == null){
					_Invoker_p = new Func<object, object[], object>(__Invoker);
				}
				return _Invoker_p;
			}
		}
		
		
		public object __Invoker(object target, object[] args){
			MethodTypeArguments mtype = null;
			Type[] arguments = new Type[]{};
			if(args.Length > 0){
				mtype = args[0] as MethodTypeArguments;
				if(mtype != null){
					arguments = mtype.TypeArguments.ToArray();					
					for(var i=0;i<args.Length -1;i++){
						args[i] = args[i+1];
					}
					Array.Resize(ref args, args.Length - 1 );
				}
			}
			return this.Invoke(target, args, arguments);
		}	
		
		
		public static Exception CreateException(params object[] message){
			List<string> smessages = new List<string>();
			foreach(var str in message){
				smessages.Add(str.ToString());
			}
			return new Exception(string.Join("", smessages));
		}
		
		
		public static Func<object, object[], object> CompileMethodInfo(MethodInfo method){
			
	    	var parameters = method.GetParameters();
	    	foreach(var param1 in parameters){
	    		if(param1.ParameterType.IsByRef  || param1.IsOut){
	    			return null; 
	    		}
	    		
	    		if(param1.ParameterType.IsGenericParameter){
	    			return null;
	    		}
	    	}
	    	
	    	
	    	List<ParameterExpression> params1 = new List<ParameterExpression>();
	    	var targetParam = Expression.Parameter(typeof(object));
	    	params1.Add(targetParam);
	    	
	    	var argsParams = Expression.Parameter(typeof(object[]));
	    	params1.Add(argsParams);
	    	
	    	var callParams = new List<Expression>();
	    	
	    	var comp = Expression.IfThen(Expression.NotEqual(Expression.ArrayLength(argsParams), Expression.Constant(parameters.Length)),
	    	                  Expression.Throw(Expression.Call(CreateExceptionMethod, Expression.Constant("Array with different dimension. Waited = " + parameters.Length.ToString() + ", Length = "), Expression.ArrayLength(argsParams) )));
	    	                  
	    	for(var i=0;i<parameters.Length;i++){
	    		
	    		var p = Expression.ArrayIndex(argsParams, Expression.Constant(i));
	    		var p1 = Expression.Convert(p, parameters[i].ParameterType);
	    		callParams.Add(p1);
	    	}
	    	
	    	Expression call;
	    	if(method.IsStatic){
	    		call = Expression.Call(method, callParams);
	    	}
	    	else{
	    		call = Expression.Call(Expression.Convert(targetParam, method.DeclaringType), method, callParams);
	    	}
	    	
	    	LabelTarget label = Expression.Label(typeof(object));
	    	BlockExpression block = null;
	    	var labele = Expression.Label(label, Expression.Constant(null));
	    	if(method.ReturnType != typeof(void)){	    
	    		Expression end = Expression.Return(label, Expression.Convert(call, typeof(object)));
	    		 
	    		block = Expression.Block(typeof(object), comp, end, labele);
	    	}
	    	else{
	    		block = Expression.Block(typeof(object), comp, call, labele);;
	    	}
	    	
	    	var expr = Expression.Lambda<Func<object, object[], object>>(block, params1.ToArray());
	    	
	    	return expr.Compile();
		}
		
		
		
		public void Init(){
			
			foreach(var method in methods){
				try{
					var func = CompileMethodInfo(method);
					compiled.Add(func);
				}catch(Exception){
					compiled.Add(null);
				}
			}
			
			methodsInfo  = new List<MethodInformation>();
			var i =0;
			int voids = 0;
			foreach(var method in methods){
				var params1 = method.GetParameters();
				var isParams = false;
				if(params1.Length > 0){
					var lpar = params1[params1.Length-1];
					isParams = lpar.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
				}
				
				var m = new MethodInformation();
				m.method = method;
				m.hasRestParameter = isParams;
				m.index = i++;
				m.Invoker = compiled[m.index];
				methodsInfo.Add(m);
				
				if(method.ReturnType == typeof(void)){
					voids++;
				}
			}
			
			if(voids == 0 || voids == methods.Count){
				callsiteCanInvoke = true;
				callsiteVoid = voids > 0;
			}
		
			
		}
		
		
		public object Invoke(object target, object[] args, Type[] arguments){
			object result = null;
			
			if(arguments.Length == 0 && target != null){
				// try callSite, is faster
				if(Kodnet.current.UseCallSiteOnMethods && callsiteCanInvoke){
					if(callsiteVoid){
						CallSiteInvoker.invokeMethodVoid(target, name, args);
						return null;
					}
					else{
						return CallSiteInvoker.invokeMethod(target, name, args);
					}
				}
			}
			
			var method = GetMethodForParameters(args, arguments);
			if(method != null){
				
				
				Func<object, object[], object> invoker = null;
				if(Kodnet.current.UseCompilerMethod)
					invoker = method.Invoker;
				
				if(!method.hasRestParameter){
					if(invoker != null){
						result = invoker.Invoke(target, args);
					}
					else{
						result = method.method.Invoke(target, args);	
					}
				}
				else{
					
					var args1 = new object[method.method.GetParameters().Length];
					Array.Copy(args, 0, args1, 0, args1.Length - 1);
					var args2 = new object[args.Length - args1.Length + 1];
					if(args2.Length > 0){
						Array.Copy(args, args1.Length - 1, args2, 0, args2.Length);
					}					
					args1[args1.Length - 1] = args2;
					
					
					if(invoker != null){
						result = invoker.Invoke(target, args1);
					}
					else{
						result = method.method.Invoke(target, args1);	
					}
				}
				
			} 
			return result; 
		}
		
		
		public MethodInformation GetMethodForParameters(object[] parameters, Type[] arguments)
		{
			
			if(methodsInfo.Count==1) return methodsInfo[0];
			
			List<MethodInformation> list = new List<MethodInformation>(0);
			
			
			bool flag = false;
			for (int i = 0; i < this.methodsInfo.Count; i++)
			{
				MethodInformation minfo = this.methodsInfo[i];
				MethodInfo methodBase = minfo.method;
				
				bool flag2 = true;
				if ((arguments != null) && (arguments.Length > 0))
				{
					if ((int)methodBase.GetGenericArguments().Length == (int)arguments.Length)
					{
						methodBase = ((MethodInfo)methodBase).MakeGenericMethod(arguments);
					}
					else
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					ParameterInfo[] parameters2 = methodBase.GetParameters();
					bool isParams = minfo.hasRestParameter;
					
					
					bool comp = parameters2.Length == parameters.Length;
					if(isParams){
						comp = parameters.Length >= (parameters2.Length -1 );
					}
					
					if (comp)
					{
						bool flag3 = true;
						bool flag4 = false;
						for (int j = 0; j < (int)parameters2.Length; j++)
						{
							ParameterInfo parameterInfo = parameters2[j];
							Type type2 = parameterInfo.ParameterType;
							Type type = null;
							
							if((j == parameters2.Length - 1) && isParams){
								
								if(parameters.Length == j){
									break;
								}
								
								if (parameters[j] == null)
								{
									flag4 = true;
								}
								else{
									type2 = type2.GetElementType();
									type = parameters[j].GetType();									
								}
								
								
							}
							else{
								if (parameters[j] == null)
								{
									flag4 = true;
								}
								else
								{
									type = parameters[j].GetType();
								}

							}
							
							if(type != null){
								if (!type2.IsAssignableFrom(type))
								{
									flag3 = false;
								}
							}
							
						}
						if (flag3)
						{
							
							list.Add(minfo);							
							i = 99999;
							flag = (flag ? true : flag4 & flag3);
						}
					}
				}
			}
			
			var count = list.Count ;
			if (flag && count > 1 || count == 0)
			{
				throw new Exception("No se puede determinar la mejor coincidencia para la ejecución del método.");
			}
			
			return list[0];
		}
		
	}
}
