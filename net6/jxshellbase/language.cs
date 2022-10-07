using System;
using System.Collections.Generic;
using System.Reflection;

namespace jxshell
{
	public class language
	{
		public static Dictionary<string, languageEngine> languages;

		public static languageEngine defaultLanguage;

		public virtual string LanguageName
		{
			get
			{
				return "c#";
			}
		}

		static language()
		{
			language.languages = new Dictionary<string, languageEngine>();
			language.defaultLanguage = new csharplanguageEngine();
			language.languages["c#"] = language.defaultLanguage;
			language.languages["csharp"] = language.defaultLanguage;
		}

		public language()
		{
		}

		public virtual Assembly GetCompiledAssembly()
		{
			return null;
		}

		public virtual void LoadClass(string file)
		{
		}

		public virtual void RunFile(string file)
		{
		}

		public virtual void RunScript(string script)
		{
		}
		
		public virtual void RunScriptWithId(string script, string id)
		{
		}
	}
}