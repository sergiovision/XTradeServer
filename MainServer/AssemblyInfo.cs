using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

//#if NET_40

[assembly: AssemblyConfiguration("net-4.5.win32; Release")]
//#else
//[assembly: AssemblyConfiguration("net-3.5.win32; Release")]
//#endif

[assembly: AssemblyProduct("FXMind.MainServer")]
[assembly: AssemblyDescription("FXMind Main Server")]
[assembly: AssemblyCompany("http://quartznet.sourceforge.net/")]
[assembly: AssemblyCopyright("Copyright 2007-2013 Marko Lahma")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
//[assembly: CLSCompliant(true)]

[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.0.0")]


#if STRONG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("FXMind.MainServer.Net.snk")]
[assembly:AllowPartiallyTrustedCallers]
#endif

[assembly: AssemblyTitle("FXMind.MainServer")]