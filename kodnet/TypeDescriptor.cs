/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 29/9/2022
 * Time: 02:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoxShell
{
	/// <summary>
	/// Description of TypeDescriptor.
	/// </summary>
	public class TypeDescriptor
	{
		Type type;
		internal DelegateWrapperStatic ws;
		
		public Dictionary<string, object> instance = new Dictionary<string, object>();
		public Dictionary<string, object> noninstance = new Dictionary<string, object>();

		public List<FieldDescriptor> Fields = new List<FieldDescriptor>();
		public List<MethodDescriptor> Methods = new List<MethodDescriptor>();
		public ConstructorDescriptor Constructor;
		public List<FieldDescriptor> StaticFields = new List<FieldDescriptor>();
		public List<MethodDescriptor> StaticMethods = new List<MethodDescriptor>();


		static Dictionary<Type, TypeDescriptor> types = new Dictionary<Type, TypeDescriptor>();
		
		public TypeDescriptor(Type type)
		{
			this.type = type;
			this.Load();
		}
		
		
		public DynamicValue GetInstanceWrapper(object o){
			var props = new DynamicPropsDictionary(instance);
			var dyn = new DynamicValue(props);
			dyn.value = o;
			return dyn;
		}
		public static object ConvertNumber(object value)
		{
			if(value is sbyte
					|| value is byte
					|| value is short
					|| value is ushort
					|| value is int){

				return System.Convert.ChangeType(value, typeof(int));
			}

			if(value is long ){
				var l = (long)value;
				if(l <= System.Int32.MaxValue && l >= System.Int32.MinValue)
					return System.Convert.ChangeType(value, typeof(int));
			}

			if(value is uint
					|| value is long
					|| value is ulong
					|| value is float
					|| value is double
					|| value is decimal){

				return System.Convert.ChangeType(value, typeof(double));

			}
			return null;
		}

		public static object GetWrappedIfRequired(object o){
			//System.Text.Encoding.
			
			if(o == null || o is System.DBNull){
				return null;
			}	

			if(o is DynamicValue){	
				return o;
			}

			var number = ConvertNumber(o);
			if(number != null) return number;
			
			if(o.GetType().IsPrimitive || (o is string)){
				return o;
			}
			
			var dw = o as DelegateWrapper;
			if(dw != null){
				var dyn = Get(dw.function.GetType()).GetInstanceWrapper(dw.function);
				dyn.meta = dw;
				return dyn;
			}
			return Get(o.GetType()).GetInstanceWrapper(o);
		}
		
		public DynamicValue GetStaticWrapper(){
			var props = new DynamicPropsDictionary(noninstance);
			var dyn = new DynamicValue(props);
			dyn.value = null;
			return dyn;
		}
		
		
		public static TypeDescriptor Get(Type t){
			TypeDescriptor value = null;
			if(!types.TryGetValue(t, out value)){
				value = new TypeDescriptor(t);
				types[t] = value; 
			}
			return value; 
		}
		
		
		public void Load(){
			
			
			instance["$TypeDescriptor"] = this;
			noninstance["$TypeDescriptor"] = this;
			
			
			
			//instance["item"] = true;
			
			
			
			//noninstance["item"] = true; 
			
			
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.SetField);
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
			
			
			ConstructorInfo[] ctors = type.GetConstructors();
			
			
			ConstructorInfo[] constructors = type.GetConstructors();
			PropertyInfo[] sproperties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			FieldInfo[] sfields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.SetField);
			MethodInfo[] smethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod);
			
			
			
			/* FIELDS */
			foreach(var field in fields){
				var fd = new FieldDescriptor();
				fd.fieldInfo = field; 
				fd.name = field.Name;
				instance[fd.name] = fd.Getter;
				instance[".set." + fd.name] = fd.Setter;

				Fields.Add(fd);
			}
			
			foreach(var field in sfields){
				var fd = new FieldDescriptor();
				fd.fieldInfo = field; 
				fd.name = field.Name;
				noninstance[fd.name] = fd.Getter;
				noninstance[".set." + fd.name] = fd.Setter;

				StaticFields.Add(fd);
			}
			/* FIELDS */
			
			/* METHODS */
			Dictionary<string, List<MethodInfo>> methods1 = new Dictionary<string, List<MethodInfo>>();
			foreach(var prop in methods){
				List<MethodInfo> list = null;
				string name = prop.Name;
				if(!prop.IsPublic){
					name = "hidden_" + name;
				}
				//Console.WriteLine(name);
				if(!methods1.ContainsKey(name)){
					list = new List<MethodInfo>();
					methods1[name] = list;
				}else{
					list = methods1[name];
				}
				list.Add(prop);
			}
			foreach(var item in methods1){
				var list = item.Value;
				var pd = new MethodDescriptor();
				pd.methods = list;
				pd.name = item.Key;
				pd.type = type;
				if(pd.name.StartsWith("hidden_")){
					pd.hidden = true;
				}
				pd.Init();
				instance[pd.name] = pd.Invoker;
				Methods.Add(pd);
			}
			
			
			
			methods1 = new Dictionary<string, List<MethodInfo>>();
			foreach(var prop in smethods){
				List<MethodInfo> list = null;
				string name = prop.Name;
				if(!prop.IsPublic){
					name = "hidden_" + name;
				}
				if(!methods1.ContainsKey(name)){
					list = new List<MethodInfo>();
					methods1[name] = list;
				}else{
					list = methods1[name];
				}
				list.Add(prop);
			}
			foreach(var item in methods1){
				var list = item.Value;
				var pd = new MethodDescriptor();
				pd.methods = list;
				pd.name = item.Key;
				pd.type = type;
				if(pd.name.StartsWith("hidden_")){
					pd.hidden = true;
				}
				pd.Init();
				noninstance[pd.name] = pd.Invoker;
				StaticMethods.Add(pd);
			}
			/* METHODS */
			
			/* CONSTRUCTORES */
			noninstance["$ctors"] = ctors;
			if(typeof(MulticastDelegate).IsAssignableFrom(type)){
				
				//var value11 = TypeDescriptor.Get(typeof(List<ConstructorInfo>)).GetInstanceWrapper(ctors);
				ws = new DelegateWrapperStatic(type);
				noninstance["construct"] = ws.Invoker;
				noninstance[".ctor"] = ws.Invoker;
				
			}
			else{			
				
				var ctorList = new  List<ConstructorInfo>(ctors);
				var cd = new ConstructorDescriptor();
				cd.methods = ctorList;
				cd.name = ".ctor";
				noninstance[cd.name] = cd.Invoker;
				noninstance["construct"] = cd.Invoker;

				Constructor = cd;
			}
			/* CONSTRUCTORES */
			
			
			/* GET PROPERTIES DIRECTLY FROM METHODS */
			var useProperties = (Kodnet.current != null) && (Kodnet.current.UseCallSiteOnMethods);			
			if(!useProperties){
				var names = new List<string>();
				foreach(var prop in properties){
					if(!names.Contains(prop.Name)){
						names.Add(prop.Name);
					}
				}
				foreach(var name in names){
					var fget = "get_" + name;
					var fset = "set_" + name;				
					if(instance.ContainsKey(fget)){
						instance[name] = instance[fget];
					}
					if(instance.ContainsKey(fset)){
						instance[".set." + name] = instance[fset];
					}
				}
				
				names = new List<string>();
				foreach(var prop in sproperties){
					if(!names.Contains(prop.Name)){
						names.Add(prop.Name);
					}
				}
				foreach(var name in names){
					var fget = "get_" + name;
					var fset = "set_" + name;				
					if(noninstance.ContainsKey(fget)){
						noninstance[name] = noninstance[fget];
					}
					if(noninstance.ContainsKey(fset)){
						noninstance[".set." + name] = noninstance[fset];
					}
				}
			}
			/* GET PROPERTIES DIRECTLY FROM METHODS */
			
			
			if(useProperties){ //  BY DEFAULT THIS IS FALSE
				Dictionary<string, List<PropertyInfo>> props = new Dictionary<string, List<PropertyInfo>>();
				foreach(var prop in properties){
					List<PropertyInfo> list = null;
					if(!props.ContainsKey(prop.Name)){
						list = new List<PropertyInfo>();
						props[prop.Name] = list;
					}else{
						list = props[prop.Name];
					}
					list.Add(prop);
				}
				foreach(var list in props.Values){
					var pd = new PropertyDescriptor();
					pd.properties = list;
					pd.name = list[0].Name;
					instance[pd.name] = pd.Getter;
					instance[".set." + pd.name] = pd.Setter;
				}
				
				
				props = new Dictionary<string, List<PropertyInfo>>();
				foreach(var prop in sproperties){
					List<PropertyInfo> list = null;				
					
					if(!props.ContainsKey(prop.Name)){
						list = new List<PropertyInfo>();
						props[prop.Name] = list;
					}else{
						list = props[prop.Name];
					}
					list.Add(prop);
				}
				foreach(var list in props.Values){
					var pd = new PropertyDescriptor();
					pd.properties = list;
					pd.name = list[0].Name;
					noninstance[pd.name] = pd.Getter;
					noninstance[".set." + pd.name] = pd.Setter;
				}
			}


            if (type.IsEnum)
            {
                instance["BitAnd"] = new Func<object, object[], object>(BitAnd);
                instance["BitOr"] = new Func<object, object[], object>(BitOr);
            }


			if(instance.ContainsKey("ToString") && !instance.ContainsKey("ToString_2")){
				// this is for compatibility with kodnet1 
				// some methods has special ToString method, and is sent to client as ToString_2
				instance["ToString_2"] = instance["ToString"];
			}
			
			// Additional Methods
			if(instance.ContainsKey("Dispose")){
				instance[".dispose"] = instance["Dispose"];
			}
			instance["Dispose"] = new Func<DynamicValue, object[], object>(this.Item_Dispose);
			instance["Internal_Free"] = new Func<DynamicValue, object[], object>(this.Item_Free);
			
		}
		
		internal object Item_Dispose(DynamicValue target, object[] args){
			object disp = null;
			object result = null;
			if(instance.TryGetValue(".dispose", out disp)){
				var dispose = disp as Func<object, object[], object>;
				if(dispose !=null){
					result = dispose(target.value, args);
				}
			}
			
			this.Item_Free(target, null);
			return result;
		}
		
		internal object Item_Free(DynamicValue target, object[] args){
			
			if(target.meta != null){
				var dw = target.meta as DelegateWrapper;
				if(dw != null){
					dw.target = null;
				}
				target.meta = null;
			}			
			target.value = null;			
			return null;
		}

        // This is For Enum types
        public object BitAnd(object target, object[] args)
        {
            Enum enum1 = (Enum)target;
            if (args.Length == 0)
            {
                throw new ArgumentException("Invalid arguments. Enum value is required");
            }
            Enum value = (Enum)args[0];

            object value2 = Convert.ChangeType(enum1, enum1.GetTypeCode());
            object value3 = Convert.ChangeType(value, enum1.GetTypeCode());
            long num = (long)Convert.ChangeType(value2, TypeCode.Int64) & (long)Convert.ChangeType(value3, TypeCode.Int64);
            object value4 = Convert.ChangeType(num, enum1.GetTypeCode());
            return Enum.ToObject(type, value4);
        }

        public object BitOr(object target, object[] args)
        {
            Enum enum1 = (Enum)target;
            if(args.Length == 0)
            {
                throw new ArgumentException("Invalid arguments. Enum value is required");
            }
            Enum value = (Enum)args[0];

            object value2 = Convert.ChangeType(enum1, enum1.GetTypeCode());
            object value3 = Convert.ChangeType(value, enum1.GetTypeCode());
            long num = (long)Convert.ChangeType(value2, TypeCode.Int64) | (long)Convert.ChangeType(value3, TypeCode.Int64);
            object value4 = Convert.ChangeType(num, enum1.GetTypeCode());
            return Enum.ToObject(type, value4);
        }

    }







}
