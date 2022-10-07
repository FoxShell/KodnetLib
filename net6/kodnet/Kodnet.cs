/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 29/9/2022
 * Time: 02:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using jxshell;

namespace FoxShell
{
	
    namespace net6{

        

        [Guid("491DFDA1-0B34-4422-845B-510DF6F9B730")]
        //[ProgId("FoxShell.NetCore.KodnetCOM")]
        [ComVisibleAttribute(true)]
        //[ClassInterface(ClassInterfaceType.AutoDispatch)]
        [ClassInterface(ClassInterfaceType.AutoDual)]
        public class KodnetCOM: DynamicValue{
            public KodnetCOM(){
                
                this.props = new DynamicPropsDictionary(TypeDescriptor.Get(typeof(Kodnet)).instance);
                this.value = new Kodnet();
            }
        }

    }

	/*
	public interface KodnetVFPClient{
		object SetVar(object name, object value);
		object ProcessId();
		object DoCmd(object cmd);
		object ThrowError(object e);
		
	}
	*/

	



	
	
	public class KodnetUtils{
		Kodnet kodnet; 
		public object shide;
		bool _initedui = false;
		
		public KodnetUtils(Kodnet kodnet){
			this.kodnet= kodnet;
		}
		
		public object Dictionary(){
			return Dictionary(typeof(string), typeof(object));
		}
		
		public object Dictionary(Type keyType, Type valueType){
			
			return typeof(System.Collections.Generic.Dictionary<object, object>).GetGenericTypeDefinition().MakeGenericType(keyType, valueType).GetConstructor(new Type[]{}).Invoke(new object[]{});
		}
		
		public object Dictionary(string keyTypeStr, string valueTypeStr){
			Type keyType = kodnet.GetTypeFromString(keyTypeStr);
			Type valueType = kodnet.GetTypeFromString(valueTypeStr);
			
			return Dictionary(keyType,valueType);
		}
		
		
		public object CustomList(Type valueType, params object[] args){
			dynamic list = typeof(System.Collections.Generic.List<object>).GetGenericTypeDefinition().MakeGenericType(valueType).GetConstructor(new Type[]{}).Invoke(new object[]{});
			foreach(var arg in args)
				list.Add(arg);
			
			return list; 
		}
		
		public object CustomList(string valueTypeStr, params object[] args){
			Type valueType = kodnet.GetTypeFromString(valueTypeStr);
			return CustomList(valueType, args);
		}
		
		public object List(){
			return new List<object>();
		}
		
		public object List(params object[] args){
			return new List<object>(args);
		}
		
		public object ObjectArray(params object[] args){
			return this.Array(typeof(object), args);
		}
		
		public object Array(Type valueType, params object[] args){
			dynamic list = CustomList(valueType, args);
			return list.ToArray();
		}
		
		public object Array(string valueTypeStr, params object[] args){
			Type valueType = kodnet.GetTypeFromString(valueTypeStr);
			dynamic list = CustomList(valueType, args);
			return list.ToArray();
		}
		
		
		public object CreateEventHandler(object target, string method, string delegateType){
			if(string.IsNullOrEmpty(delegateType)){
				delegateType = "System.EventHandler" ;
			}
			var type = kodnet.GetTypeFromString(delegateType);
			return CreateEventHandler(target, method, type);
		}
		public object CreateEventHandler(object target, string method, Type delegateType){
			var td = TypeDescriptor.Get(delegateType);
			if(td.ws == null){
				throw new ArgumentException("Type is not delegate");
			}
			
			return td.ws.Create(target, method);
		}
		
		public object CreateEventHandler(object target, string method){
			return CreateEventHandler(target, method, "System.EventHandler");
		}
		
		
		public void InitUI(){
			if(!this._initedui){
				kodnet.LoadAssemblyPartialName("System.Drawing");
				kodnet.LoadAssemblyPartialName("System.Windows.Forms");
				
				var td = TypeDescriptor.Get(kodnet.GetTypeFromString("System.Windows.Forms.Application"));
				var func = td.noninstance["EnableVisualStyles"] as  Func<object, object[], object>;
				func(null, new object[]{});
				_initedui = true;
			}
		}
		
		public Kodnet GetCLR(){
			return kodnet;
		}
		
		public Kodnet GetKodnet(){
			return kodnet;
		}
		
		public int RgbToArgb(int rgb, int alpha){
			string rgbhex = rgb.ToString("X");
			string alphahex = alpha.ToString("X");
			
			return int.Parse(alphahex + rgbhex, System.Globalization.NumberStyles.HexNumber);
		}
		
		
		public object Await(Task task){
			task.Wait();
			if(task.IsFaulted){
				throw task.Exception;
			}
			
			var td = TypeDescriptor.Get(task.GetType());
			if(td.instance.ContainsKey("Result")){
				return CallSiteInvoker.GetInvokerparam0P("Result").invoke(task);
			}
			return null; 
		}
		
		public int VFPProcessId(){
			return (int)CallSiteInvoker.GetInvokerparam0("ProcessId").invoke(kodnet.VFP);
		}

		public object GetShide(){
			if(this.shide == null){
				this.shide = CreateShide();
			}
			return this.shide;
		}
		
		public object CreateShide(){
			//MessageBox.Show("here->");
			var path = Assembly.GetCallingAssembly().Location;
			var folder = Path.GetDirectoryName(path);
			var shideDll = Path.Combine(folder, "shide.dll");
			var shideScript = Path.Combine(folder, "shide.ts");
			if(!File.Exists(shideScript)){
				shideScript = Path.Combine(Path.GetDirectoryName(folder), "shide.ts");					
			}
			var pid = this.VFPProcessId();
			
			kodnet.LoadAssemblyFile(shideDll);
			var td = TypeDescriptor.Get(kodnet.GetTypeFromString("Shide.Executor"));
			var func = td.noninstance[".ctor"] as Func<object, object[], object>;
			var executor = func(null, new object[]{});
			//MessageBox.Show(executor != null ? executor.GetType().ToString(): "nada");
			func = td.instance["Start"] as Func<object, object[], object>;
			func(executor, new object[]{ shideScript + " " + pid.ToString()});
			return executor;			
		}
	}
	
	
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class Kodnet
	{
		
		Encoding win1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
		//Encoding.GetEncoding("WINDOWS1252");
		public Dictionary<string, Type> loadedTypes = new Dictionary<string, Type>();
		List<Assembly> assemblies = new List<Assembly>();
		public KodnetClient VFP = null;
		public object VFPCom = null;
		public static Kodnet current; 
		List<string> folders = new List<string>();
		public Exception exception;
		
		public KodnetUtils utils;
		
		
		
		public bool UseCompilerMethod = true;
		public bool UseCallSiteOnProps = true;
		
		// PREFERABLE DON'T ENABLE, BECAUSE NO MUCH DIFFERENCE IN PERFORMANCE, AND SOME DRAWBACKS
		public bool UseCallSiteOnMethods = false;
		public Dictionary<string, Assembly> fileAssemblies = new Dictionary<string, Assembly>();
		
		
		
		public Kodnet(){
			current = this; 
			utils = new KodnetUtils(this);
			this.LoadAssembly(typeof(string).Assembly);			
			this.LoadAssembly(typeof(Console).Assembly);
			this.LoadAssembly(typeof(WebClient).Assembly);
            this.LoadAssembly(typeof(TypeDescriptor).Assembly);
            this.LoadAssembly(typeof(jxshell.csharplanguage).Assembly);
            
            
            
            ResolveEventHandler value = (object sender, ResolveEventArgs args) => {
				Assembly value2 = null;
                AssemblyName an = new AssemblyName(args.Name);

				this.fileAssemblies.TryGetValue(args.Name, out value2);
                if(value2 == null)
                {
                    var path = Path.GetDirectoryName(args.RequestingAssembly.Location);
                    var file1 = Path.Combine(path, an.Name);
                    if (File.Exists(file1))
                    {
                        return Assembly.LoadFile(file1);
                    }
                    file1 += ".dll";
                    if (File.Exists(file1))
                    {
                        return Assembly.LoadFile(file1);
                    }

                    this.fileAssemblies.TryGetValue(an.Name, out value2);

                }
				return value2;
			};
			AppDomain.CurrentDomain.AssemblyResolve += value;
			environment.initEnvironment();
		}
		public void SetVFPClient(int vfp){

			
			
			VFP = new KodnetClient(vfp);
		}

		public void SetVFPClient(object vfp){

			
			
			VFP = new KodnetClient(vfp);
		}
		
		public void SetVFPCom(object vfp){
			VFPCom = vfp;
		}
		
		public Exception LastException{
			get{
				return this.exception;
			}
		}
		
		
		

		public byte[] GetBytesFromString(string s)
		{
			return win1252.GetBytes(s);
		}
		
		public object GetDefaultFor(Type type)
		{
			object o = null;
			if (type.IsValueType)
			{
				o = Activator.CreateInstance(type);
			}
			return o; 
		}
		
		
		
		public DynamicValue GetStaticWrapper(string type){
			Type t = this.GetTypeFromString(type);
			return TypeDescriptor.Get(t).GetStaticWrapper();
		}
		
		
		public DynamicValue GetWrapped(object o, Type t){
			if(o == null) return null;
			
			if(t == null){
				t = o.GetType();
			}
			return TypeDescriptor.Get(t).GetInstanceWrapper(o);
		}
		
		
		public DynamicValue GetWrapped(object o){
			return GetWrapped(o, null);
		}
		
		public DynamicValue GetWrappedConverted(object o, Type t){
			if(o == null) return null;
			
			object no = Convert.ChangeType(o, t);
			return TypeDescriptor.Get(t).GetInstanceWrapper(no);
		}
		
		
		
		
		
		public Type GetTypeFromString(string typeName)
		{
			Type value;
			string[] array = typeName.Split(new char[] { '<' });
			if ((int)array.Length <= 1)
			{
				if (typeName.IndexOf("[") <= 0)
				{
					if (!this.loadedTypes.TryGetValue(typeName, out value))
					{
						throw new Exception("El tipo especificado no se encontró. Revise si debe cargar un ensamblado.");
					}
					return value;
				}
				int num = typeName.IndexOf('[');
				string typeName3 = typeName.Substring(0, num);
				string text3 = typeName.Substring(num);
				Type type2 = this.GetTypeFromString(typeName3);
				char c = text3[0];
				int num2 = 0;
				int num3 = 1;
				while (num2 < text3.Length)
				{
					while (true)
					{
						if (c == ',')
						{
							num3++;
						}
						else if (c == ']')
						{
							break;
						}
						num2++;
						c = text3[num2];
					}
					num2++;
					type2 = type2.MakeArrayType(num3);
					num3 = 1;
				}
				return type2;
			}
			if (array[1].IndexOf(">") < 0)
			{
				throw new Exception("El nombre del tipo no es válido.");
			}
			int length = array[1].LastIndexOf('>');
			string text = array[1].Substring(0, length);
			string[] array2 = text.Split(new char[] { ',' });
			List<Type> list = new List<Type>();
			string[] strArrays = array2;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string text2 = strArrays[i];
				list.Add(this.GetTypeFromString(text2.Trim()));
			}
			Type[] array4 = list.ToArray();
			string str = array[0];
			int num1 = (int)array4.Length;
			Type typeOrGenericType = this.GetTypeFromString(string.Concat(str, "`", num1.ToString()));
			return typeOrGenericType.MakeGenericType(array4);
		}
		
		
		
		public void LoadTypes(string types)
		{
			string[] array = types.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder stringBuilder = new StringBuilder();
			
			//typeDescriptor.addUsingsStatements(stringBuilder);
			//Dictionary<Type, type_1> dictionary = new Dictionary<Type, type_1>();
			
			string[] strArrays = array;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string typeName = strArrays[i];
				Type typeOrGenericType = this.GetTypeFromString(typeName);
				//typeDescriptor typeDescriptor = new typeDescriptor(typeOrGenericType, typeName, false);
				TypeDescriptor.Get(typeOrGenericType);
			}
		}
		
		
		public void LoadAssembly(string name){
			this.LoadAssembly(Assembly.Load(name));
		}
		
		public void LoadAssembly(Assembly assembly){
			if(assemblies.Contains(assembly)){
				return; 
			}
			
			assemblies.Add(assembly);
			environment.loadAssembly(assembly, true);
			
			var file = assembly.Location;
			this.fileAssemblies[file] = assembly;
            this.fileAssemblies[assembly.FullName] = assembly;

			//file = Path.GetFileNameWithoutExtension(file);
			//this.fileAssemblies[file] = assembly;
			
			
			Type[] types = assembly.GetTypes();
			for (int i = 0; i < (int)types.Length; i++)
			{
				Type type = types[i];
				if (!type.IsGenericType)
				{
					string text3 = type.ToString();
					string text4 = text3.Replace("+", ".");
					this.loadedTypes[text3] = type;
					if (text4 != text3)
					{
						this.loadedTypes[text4] = type;
					}
				}
				else
				{
					string text = type.ToString();
					string text2 = text.Replace("+", ".");
					int length = text.IndexOf("[");
					this.loadedTypes[text.Substring(0, length)] = type;
					if (text2 != text)
					{
						this.loadedTypes[text2.Substring(0, length)] = type;
					}
				}
			}
		}
		
		public void LoadAssemblyFile(string path){
			var dir = Path.GetDirectoryName(path);
			if(!folders.Contains(dir)){
				folders.Add(dir);
			}
			
			Assembly assembly = Assembly.LoadFile(path);
			this.LoadAssembly(assembly);
		}
		
		
		public void LoadAssemblyPartialName(string name){
			this.LoadAssembly(Assembly.LoadWithPartialName(name));
		}
		
		
		
		
	}
}