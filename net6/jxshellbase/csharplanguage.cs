using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using System.Runtime.Loader;
using System.Reflection;
using System.Text;

namespace jxshell
{

	public class CompileException : Exception{

		public CompileException(string message): base(message){}
		public CompileException(string message, Exception innerException): base(message,innerException){}

	}

	public class csharplanguage : language
	{
		//private CSharpCodeProvider cp;
		//private CompilerParameters p = new CompilerParameters();


		public List<Diagnostic> CompilationErrors = new List<Diagnostic>();
		public Assembly compiled = null;

		private string sourceDefault = "";
		private static Dictionary<string, int> compilations;
		private static Dictionary<string, Assembly> compileds;




		public override string LanguageName
		{
			get
			{
				return "c#";
			}
		}

		static csharplanguage()
		{
			csharplanguage.compilations = new Dictionary<string, int>(0);
			csharplanguage.compileds = new Dictionary<string, Assembly>(0);
		}

		public csharplanguage()
		{
			
		}

		public void compileString(string script, string file)
		{
			string[] location = new string[environment.assemblies.Count];
			PortableExecutableReference[] references = new PortableExecutableReference[environment.assemblies.Count];
			for (int i = 0; i < environment.assemblies.Count; i++)
			{
				if (environment.assemblies[i] == null)
				{
					throw new Exception("No se pudo cargar uno o más ensamblados.");
				}
				var location1 = environment.assemblies[i].Location;				
				references[i] = MetadataReference.CreateFromFile(location1);
				location[i] = location1;
			}

			

			var tree = SyntaxFactory.ParseSyntaxTree(script);
			var compilation = CSharpCompilation.Create(System.IO.Path.GetFileName(file))
				.WithOptions(
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
				.AddReferences(references)
				.AddSyntaxTrees(tree);

			EmitResult compilationResult = compilation.Emit(file);
			if(compilationResult.Success){
				Assembly asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
				environment.loadAssembly(asm, true);
				compiled = asm;
			}
			else{
				CompilationErrors.Clear();
				StringBuilder sb = new StringBuilder();
				foreach (Diagnostic codeIssue in compilationResult.Diagnostics){
					CompilationErrors.Add(codeIssue);
					sb.AppendLine( $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: {codeIssue.Location.GetLineSpan()}, Severity: {codeIssue.Severity}");
				}
				throw new CompileException(sb.ToString());
			}

		}

		public override Assembly GetCompiledAssembly()
		{
			return this.compiled;
		}

		
		public void LoadScript(string code, string id)
		{
			
			string file  = environment.getCompilationFile(id);
			var f = new FileInfo(file);
			bool compile= true;
			if(f.Exists){
				try{
					this.compiled = Assembly.LoadFile(file);	
					compile = false;
				}
				catch(Exception){}
				
			}
			if(compile){
				this.compileString(code, file);
			}
			
		}

		public override void RunFile(string file)
		{

			var str = System.IO.File.ReadAllText(file);
			this.RunScript(str);
		}
	
		
		public static string GetSHA1(String texto)
		{
			

			string[] location = new string[environment.assemblies.Count];
			for (int i = 0; i < environment.assemblies.Count; i++)
			{
				var location1 = "";
				if (environment.assemblies[i] != null)
				{
					location1 = environment.assemblies[i].FullName;	
				}				
				location[i] = location1;
			}

			SHA1 sha1 = SHA1.Create();
			Byte[] textOriginal = Encoding.UTF8.GetBytes(texto + ">" + string.Join('-',location));
			Byte[] hash = sha1.ComputeHash(textOriginal);
			return  BitConverter.ToString(hash).Replace("-", string.Empty);
		}
		
		
		public override void RunScript(string script)
		{

			RunScriptWithId(script, "JIT-" + GetSHA1(script));
		}
		
		public override void RunScriptWithId(string script, string id)
		{
			this.LoadScript(script, id);
			Type type = compiled.GetType("program");
			if(type != null){
				MethodInfo method = type.GetMethod("main", new Type[0]);
				method.Invoke(null, new object[0]);	
			}
		}

		public void RunScript(string script, bool inMemory)
		{
			this.RunScript(script);
		}
	}
}