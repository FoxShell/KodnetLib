/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 30/9/2022
 * Time: 05:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Shide
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	public class Executor{
		Process p;
		TaskCompletionSource<bool> started;
		Dictionary<string, TaskCompletionSource<object>> tasks = new Dictionary<string, TaskCompletionSource<object>>();
		Dictionary<string, object> modules = new Dictionary<string,object>();
		Module def; 
		
		public void Start(string file){
			Start(file, 12000);
		}
		
	
		public object WaitTask(Task<object> task){
			task.Wait();
			if(task.Exception != null) throw task.Exception;
			
			return task.Result;
		}
		
		public static bool IsLinux
		{
			get
			{
				int p = (int) Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}
		
		public void Start(string file, int timeout){
			p = new Process();
			
			var home = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
			var kwrunpath = System.IO.Path.Combine(home, "KwRuntime", "bin", "kwrun");

			if(!IsLinux){
				kwrunpath += ".exe";
			}

			p.StartInfo.FileName = kwrunpath;
			p.StartInfo.CreateNoWindow =true;
			p.StartInfo.Arguments = file; //"\\\\tsclient\\_home_james\\projects\\scripts\\shide\\program.ts";
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.UseShellExecute = false;
			p.Start();
			
			p.OutputDataReceived += StdoutReceived;			
			p.ErrorDataReceived += StdoutReceived;
			p.Exited += delegate { Console.WriteLine("Exited"); };
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();			
			started = new TaskCompletionSource<bool>();
			
			var completed = Task.WaitAny(started.Task, Task.Delay(timeout));
			if(started.Task.Exception != null){
				throw started.Task.Exception;
			}
			if(completed == 1){
				throw new Exception("Timeout waiting for KwRuntime");
			}
			
			var task = this.CreateModule("default", "export var id = 0;");
			task.Wait();
			if(task.Exception != null){
				throw task.Exception;
			}
			def = (Module)task.Result;
			
		}
		
		
		public string generateID()
		{
		    return Guid.NewGuid().ToString("N");
		}
		
		public object GetModule(string name){
			if(this.modules.ContainsKey(name)){
				return this.modules[name];
			}
			return null;
		}
		
		
		public Task<object> Evaluate(string code, object param){
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict["code"] = code ;
			dict["params"] = param; 
			
			return def.Execute("$evaluate",  dict);
		}
		
		
		public Task<object> EvaluateAsyncAction(string code, object param){
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict["code"] = code ;
			dict["params"] = param; 
			
			return  def.Execute("$evaluateAsync",  dict);
		}
		
		
		
		
		public Task<Module> CreateModule(string name, string code){	
			return 	CreateModule(name, code, null);
		}
		
		
		public async Task<Module> CreateModule(string name, string code, List<string> methods){
			
			dynamic e = new ExpandoObject();
			e.name = name;
			e.type = "module";
			e.code = code;
			dynamic result = await this.SendCommand(e);			
			
			if(methods == null){
				methods = new List<string>();
			}
			
			
			if(result["methods"] != null){
				var methods1 = result["methods"];
				var json = methods1 as DynamicJsonValue;
				for(var i=0;i< json.Count;i++){
					var item = json[i].ToString();
					methods.Add(item);
				}
			}
			
			if(methods.Count <= 0){
				methods.Add("Invoke");
			}
			
			var module = new Module();
			foreach(var methodName in methods){
				this._CreateFunc2(module.funcs, name, methodName);
			}
			
			modules[name] = module;
			return module; 
			
		}
		
		
		public void _CreateFunc2(Dictionary<string, object> dict, string moduleName,  string name){
			
			
			
			Func<object, object[], object> methodAsync = (target, parameters) => {
				//var task = Task.Run(new Func<Task<object>>(async () => {
					dynamic cmd = new ExpandoObject();
					cmd.name = moduleName;
					cmd.method = name; 
					cmd.type = "task";
					//cmd.param = param;	
					cmd.parameters = parameters;
					
					return this.SendCommand(cmd);
				//}));
				//return task; 
			};
			
			dict[name] = methodAsync;
		}
		
		
		
		
		
		public async Task<object> SendCommand(dynamic obj){
			//var dict = (IDictionary<string, object>)obj;
			
			var uid = generateID();
			obj.uid = uid;
			tasks[uid] = new TaskCompletionSource<object>();
			
			var str = JsonConvert.SerializeObject(obj);
			p.StandardInput.WriteLine("##" + str);
			p.StandardInput.Flush();
			
			
			var result = await tasks[uid].Task;
			
			
			
			
			return DynamicJsonValue.FromJtoken(result as JToken);
		}
		
		
		
		public void StdoutReceived(object sender, DataReceivedEventArgs e){
			Console.WriteLine(e.Data);
			
			if(null != e.Data && e.Data.StartsWith("##")){
				
				var content = e.Data.Substring(2);
				
				
				dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
				if(obj.type == "event"){
					if(obj.name == "started"){
						started.SetResult(true);
					}
					
					if(obj.name == "error"){
						if(started != null){
							var ex = new Exception(obj.value.message);
							ex.code = obj.value.code;
							started.SetException(ex);
						}
					}
				}

				
				if(obj.type == "task"){
					var uid = obj.uid.ToString();
					if(tasks.ContainsKey(uid)){
						var task = tasks[uid];
						if(task != null){
							tasks.Remove(uid);
							if(obj.status == "error"){
								var ex = new Exception(obj.error.message.ToString());
								ex.code = obj.error.code;
								task.SetException(ex);
							}
							else{
								task.SetResult(obj.value);
							}
						}
					}
				}
				
			}
		}
		
	}
}