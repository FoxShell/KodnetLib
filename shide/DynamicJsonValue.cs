/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 30/9/2022
 * Time: 06:02
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using FoxShell;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
namespace Shide
{
	/// <summary>
	/// Description of DynamicJsonValue.
	/// </summary>
	/// 
	
	public class DynamicJsonProps: DynamicProps
	{
		
		JToken data;
		Dictionary<string, object> dict = new Dictionary<string, object>();
		
		public DynamicJsonProps(): this(JToken.Parse("{}")){
			
		}
		
		public override List<string> Names(){
			var list = new List<string>();
			foreach(var item in dict.Keys){
				list.Add(item);
			}
			return list;

		}

		public DynamicJsonProps(JToken data)
		{
			this.data = data;
			if(data.Type == JTokenType.Array){
				JArray j = data as JArray;
				for(var i=0;i<j.Count;i++){					
					dict[i.ToString()] = data[i]; //FromJtoken(data[j], null, null);
				}
			}
			
			
			if(data.Type == JTokenType.Object){
				JObject j = data as JObject;
				foreach(var item in j.Properties()){
					dict[item.Name] =  item.Value; //FromJtoken(item.Value, null, null);
				}
			}


			// method Item 
			//dict["Item"] = new Func<object, object[], object>(Method_Item);

		}


		

		
		public int Count{
			get{

				if(data.Type == JTokenType.Array){
					JArray j = data as JArray;
					return j.Count;
				}
				return dict.Count;
			}
		}
		
		public static object FromJtoken(JToken  value){
			return FromJtoken(value, null, null);
		}
		public static object FromJtoken(JToken  value, Dictionary<string, object> dict, string index){
			
			if(value == null) return null;
			if((value.Type == JTokenType.String)|| (value.Type == JTokenType.Guid) || (value.Type == JTokenType.Date)){
				return value.ToObject<string>();
		 	}
			if((value.Type == JTokenType.Integer)){
				return value.ToObject<long>();
			}
			if(value.Type == JTokenType.Float){
				return value.ToObject<float>();
			}
			if(value.Type == JTokenType.Boolean){
				return value.ToObject<bool>();
			}
			if((value.Type == JTokenType.Undefined) || (value.Type == JTokenType.Null)){
				return null;
			}
			
			if(dict != null){
				if(!dict.ContainsKey(index)){
					dict[index] = new DynamicJsonProps(value);
				}
				
				return dict[index];
			}
			
			return new DynamicJsonProps(value);
		}
		
		public override object Get(string name)
		{
			object value = null;
			dict.TryGetValue(name, out value);
			return value; 
		}
		
		public override void Set(string name, object value)
		{
			dict[name] = value;
			
			var rvalue = value as DynamicJsonValue;
			if(rvalue != null){
				value = rvalue.props.data;
			}
			else{
				var rvalue1 = value as DynamicJsonProps;
				if(rvalue1 != null){
					value = rvalue1.data;
				}
				else{
					value = JToken.FromObject(value);
				}
			}

			
			if(data.Type == JTokenType.Array){
				JArray j = data as JArray;
				int index = -1;
				if(int.TryParse(name, out index)){
					j[index] = (JToken)value;
				}
			}


			if(data.Type == JTokenType.Object){
				JObject j = data as JObject;
				j[name] = (JToken)value;
				
			}
			
		}
		
	}

	[ComVisible(true)]	
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class DynamicJsonValue: DynamicValue
	{
		internal DynamicJsonProps props;
		
		public DynamicJsonValue(): this(new DynamicJsonProps())
		{
			
		}
		
		public DynamicJsonValue(DynamicJsonProps props): base(props)
		{
			this.props = props;


			props.Set("Item", new Func<object, object[], object>(Method_Item));
			props.Set("Count", new Func<object>(Method_Count));
			props.Set(".set.Item", new Func<object, object[], object>(Method_Item_set));
		}


		public object Method_Item(object target, object[] args){
			if(args[0] is int){
				return this[(int) args[0]];
			}
			else{
				return this[args[0].ToString()];
			}
		}

		public object Method_Count(){
			return this.Count;
		}

		public object Method_Item_set(object target, object[] args){
			if(args[0] is int){
				this[(int) args[0]] = args[1];
			}
			else{
				this[args[0].ToString()] = args[1];
			}
			return null;
		}


		
		public static object FromJtoken(JToken value){
			var obj = DynamicJsonProps.FromJtoken(value);
			var props = obj as DynamicJsonProps;
			if(props != null){
				return new DynamicJsonValue(props);
			}
			
			return obj;
		}
		
		public int Count{
			get{
				return props.Count;
			}
		}
		
		public override object Wrap(object value){
			
			if(value is JToken){
				var jtok = value as JToken;
				var props1 = DynamicJsonProps.FromJtoken(jtok);
				var dynprops = props1 as DynamicJsonProps;
				if(dynprops != null){
					return new DynamicJsonValue(dynprops);
				}
				return props1;
			}
			else{
				return base.Wrap(value);
			}
	    }
		
		
		public object this[int index]{
			get{
				var value = this.props.Get(index.ToString());
				if(value == null) return null;
				return Wrap(value);
			}
			set{
				this.props.Set(index.ToString(), value);
			}
		}
		
		public object this[string name]{
			get{

				var value = this.props.Get(name);
				if(value == null) return null;
				return Wrap(value);

			}
			set{
				this.props.Set(name, value);
			}
		}
		
		
	}
}
