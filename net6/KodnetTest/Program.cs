using System;
using FoxShell;

namespace KodnetTestCore // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var kodnet = new FoxShell.net6.KodnetCOM();
            var wrapper = kodnet.InvokeMember("GetStaticWrapper", System.Reflection.BindingFlags.InvokeMethod, null, null, new object[]{"System.Security.Cryptography.X509Certificates.X509Store"}, null, null, null);
            //var type = kodnet.GetTypeFromString("System.Security.Cryptography.X509Certificates.X509Store");
            //var wrapper = kodnet.GetStaticWrapper("System.Security.Cryptography.X509Certificates.X509Store");
            //FoxShell.TypeDescriptor td = TypeDescriptor.Get(type);
            

            //Console.WriteLine(bytes.GetType().ToString());
            Console.WriteLine("Hello World!");
        }
    }
}