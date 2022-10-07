/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 29/9/2022
 * Time: 22:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Reflection;
using FoxShell;

namespace KodnetTest
{
	public class Program
	{
		
		
		
		
		public int main(string a, string b){
			return a.Length;
		}
		
		public int main(int a, int b){
			return a+b;
		}
		
		public void Completed(object sender, AsyncCompletedEventArgs e){
			
		}
		
		public static void Main(string[] args)
		{

            var kodnet = new FoxShell.KodnetCOM();
			Console.ReadLine();
			                                                    
			                                                    
			//TypeDescriptor .Get(typeof(System.Text.StringBuilder)).GetStaticWrapper();
			
		}
	}
}