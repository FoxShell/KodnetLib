/*
 * Created by SharpDevelop.
 * User: james
 * Date: 27/09/2022
 * Time: 4:34 p. m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;


namespace FoxShell
{
	/// <summary>
	/// Description of Wrap.
	/// </summary>

	public class FieldSpec : FieldInfo{
		public string name;
		RuntimeFieldHandle handle = typeof(FieldSpec).GetField("name").FieldHandle;
		
		public FieldSpec(string name){
			this.name= name;
			
		}
		
		
		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return true;
		}
		
		public override object[] GetCustomAttributes(bool inherit)
		{
			return new object[]{};
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return new object[]{};
		}
		
		public override Type ReflectedType {
			get {
				return typeof(object);
			}
		}
		
		public override Type DeclaringType {
			get {
				return typeof(object);
			}
		}
		
		public override string Name {
			get {
				return this.name;
			}
		}
		
		public override FieldAttributes Attributes {
			get {
				return default(FieldAttributes);
			}
		}
		
		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, System.Globalization.CultureInfo culture)
		{
			
		}
		
		public override object GetValue(object obj)
		{
			return null;
		}
		
		public override RuntimeFieldHandle FieldHandle {
			get {
				return handle;
			}
		}

		public override Type FieldType {
			get {
				return typeof(object);
			}
		}
	}
	
	
	
	public class MethodSpec : MethodInfo{
		
		string name;
		static ParameterInfo[] parameters;
		static RuntimeMethodHandle handle = new RuntimeMethodHandle();
		
		public MethodSpec(string name){
			this.name= name;
		}
		
		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return true;
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return new object[]{};
		}
		public override object[] GetCustomAttributes(bool inherit)
		{
			return new object[]{};
		}
		
		public override Type ReflectedType {
			get {
				return typeof(object);
			}
		}
		
		public override Type DeclaringType{
			get {
				return typeof(object);
			}
		}
		
		public override string Name {
			get {
				return this.name;
			}
		}
		
		public override MethodAttributes Attributes {
			get {
				return default(MethodAttributes);
			}
		}
		
		public override RuntimeMethodHandle MethodHandle {
			get {
				return handle;
			}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			throw new NotImplementedException();
		}
		
		
		public object __example(object a0, object a1, object a2, object a3, object a4, object a5, object a6, object a7, object a8, object a9,
		                       object a10, object a11, object a12, object a13, object a14, object a15, object a16, object a17, object a18, object a19, object a20){
			
			return null; 
			
		}
		
		public override ParameterInfo[] GetParameters()
		{
			if(parameters == null){
				parameters = typeof(DynamicValue).GetMethod("__example").GetParameters();
			}
			return parameters;
		}
		
		
		public override MethodInfo GetBaseDefinition()
		{
			throw new NotImplementedException();
		}
		
		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {
				throw new NotImplementedException();
			}
		}
		
		
		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, System.Globalization.CultureInfo culture)
		{
			
			return null; 
		}
		
	}
	
	
	
	public class DynamicProps{
			
		
		public virtual List<string> Names(){
			throw new NotImplementedException();
		}
		
		public virtual object Get(string name){
			throw new NotImplementedException();
		}
		
		
		public virtual void Set(string name, object value){
			throw new NotImplementedException();
		}
		
	}
	
	
	public class DynamicPropsDictionary:DynamicProps{
		
		internal Dictionary<string, object> dict;
		
		public DynamicPropsDictionary(): this(new Dictionary<string, object>()){
			
		}
		
		public DynamicPropsDictionary(Dictionary<string, object> dict){
			this.dict = dict;
		}
		
		
		public Dictionary<string, object> Dictionary{
			get{
				return dict; 
			}
		}
		
		public override List<string> Names(){
			var list = new List<string>();
			foreach( var key in dict.Keys){
				list.Add(key);
			}
			return list;
		}
		
		public override object Get(string name){
			object value = null;
			if(!dict.TryGetValue(name, out value)){
				throw new MissingMemberException("Member with name " + name + " not available");
			}
			return value; 
		}
		
		
		
		
		
		public override void Set(string name, object value){
			dict[name] = value;
		}
		
	}
	
	
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[Guid("491DFDA1-0B34-4422-845B-510DF6F9B741")]
	//[ProgId("FoxShell.NetCore.DynamicValue")]
	[ComVisible(true)]
	public class DynamicValue: IReflect{
		
		
		internal DynamicProps props;
		internal object value;
		internal object meta; 
		
		MemberInfo[] memberCache;
		FieldInfo[] fieldCache;
		
		public Type UnderlyingSystemType
		{
			get {
				return typeof(DynamicValue);
			}
		}
		
		
		public DynamicValue(): this(new DynamicPropsDictionary()){
			
		}	
		
		public DynamicValue(DynamicProps props){
			this.props = props;
		}	
		
		public DynamicProps Properties{
			get{
				return this.props;
			}
		}
		
		public System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
		{
			return new FieldSpec(name);
		}
	
		/* SNIP other IReflect methods */
		
		public MemberInfo[] GetMembers(BindingFlags bindingAttr){
			
			if(memberCache != null){
				return memberCache;
			}
			
			var fields = this.GetFields(bindingAttr);
			var methods = this.GetMethods(bindingAttr);	    	
			var members = new List<MemberInfo>();
			foreach(var field in fields){
				members.Add(field);
			}
			
			foreach(var method in methods){
				members.Add(method);
			}
			
			return memberCache = members.ToArray();
		}
	
		public MemberInfo[] GetMember(string name, BindingFlags bindingAttr){
			var obj = props.Get(name);
			if((obj is Func<object, object>) || (obj is Func<object[], object>) || (obj is Func<object, object[], object>)){
				return new MemberInfo[]{this.GetMethod(name, bindingAttr)};
			}
			return new MemberInfo[]{this.GetField(name, bindingAttr)};
		}
	
		
		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers){
			return this.GetMethod(name, bindingAttr);
		}
		
		public MethodInfo GetMethod(string name, BindingFlags bindingAttr){
			return new MethodSpec(name);
		}
		
		public MethodInfo[] GetMethods(BindingFlags bindingAttr){
			
			return new MethodInfo[]{};
			
			
		}
		
		public FieldInfo[] GetFields(BindingFlags bindingAttr){
			
			
			if(fieldCache != null) return fieldCache;
			
			var list = new List<FieldInfo>();
			var names = this.props.Names();
			
			if(!names.Contains("Internal_Get"))
				names.Add("Internal_Get");
			
			if(!names.Contains("Internal_Value"))
				names.Add("Internal_Value");
			
			
			
			foreach(var name in names){
				var f = new FieldSpec(name);
				list.Add(f);
			}
				
			return fieldCache = list.ToArray();
			
		}
		
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr){
			return null;
		}
		
		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers){
			return null;
		}
		
		public PropertyInfo[] GetProperties(BindingFlags bindingAttr){
			return new PropertyInfo[]{};
		}
			
		
		public static object[] ConvertArguments(object[] parameters){
			var realParameters = new List<object>();
			foreach(var par in parameters){
				if(par is System.DBNull){
					realParameters.Add(par);
				}
				else if(par is System.Reflection.Missing){
					
				}
				else if(par is DynamicValue){
					var w = par as DynamicValue;
					realParameters.Add(w.value);
				}
				else{
					realParameters.Add(par);
				}
			}
			return realParameters.ToArray();
		}
		
		
		
		public object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] oargs, System.Reflection.ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
		{
			
			
			try{
				var args = ConvertArguments(oargs);
				bool isMethod = invokeAttr.HasFlag(BindingFlags.InvokeMethod);
				
				
				if(invokeAttr.HasFlag(BindingFlags.SetField) || invokeAttr.HasFlag(BindingFlags.SetProperty) || invokeAttr.HasFlag(BindingFlags.PutDispProperty)){
				
					
					if(args.Length == 1){
						name = ".set." + name;
						isMethod = true; 
					}
					
					if(args.Length > 1){
						
						if(name == "item"){
							name = args[0].ToString();
							name = ".set." + name;
							
							object[] nargs = new object[args.Length -1];
							Array.Copy(args, 1, nargs, 0, args.Length-1);
							args = nargs; 
						}
						else{
							name = ".set." + name;
							isMethod = true; 
						}
						
					}
					
				}
				
				
				if(isMethod){
					
					bool internalm = false;
					if(name == "Internal_Value"){
						return value;
					}
					
					if(name == "Internal_Get"){
						if(args.Length >= 1){
							name = args[0].ToString();
							
							for(var i=0;i<args.Length-1;i++){
								args[i] = args[i+1];
							}
							Array.Resize(ref args, args.Length-1);
							internalm = true; 
						}
					}
					
					
					
					
					var value1 = props.Get(name);
					if(!internalm){
					
					
						
						
						var func = value1 as Func<object, object[], object>;
						if(func != null){
							value1 = func(this.value, args);
						}
						else{
							var func3 = value1 as Func<DynamicValue, object[], object>;
							if(func3 != null){
								value1 = func3(this, args);
							}
							else{
								var func1 = value1 as Func<object, object>;
								if(func1 != null){
									value1 = func1(this.value);
								}
								else{
									var func2 = value1 as Func<object>;
									if(func2 != null){
										value1 = func2();
									}
								}
							}
						}
						
						if(value1 == this.value){
							return value; 
						}
						
					}
					
					return Wrap(value1);
				}
				
			}
			catch(Exception e){
				
				Kodnet.current.exception = e; 
				if(Kodnet.current.VFP != null){
					//CallSiteInvoker.GetInvokerparam1("ThrowError").invoke(Kodnet.current.VFP, TypeDescriptor.GetWrappedIfRequired(e));

					Kodnet.current.VFP.ThrowError(TypeDescriptor.GetWrappedIfRequired(e));
				}
				else{
					throw e;
				}
				
			}
			
			return null; 
		}
		
		
		public virtual object Wrap(object value){
			return TypeDescriptor.GetWrappedIfRequired(value);
		}
		
		
	}
	
}
