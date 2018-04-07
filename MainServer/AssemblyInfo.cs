using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

//#if NET_40

[assembly: AssemblyConfiguration("net-4.6.win64; Release")]

[assembly: AssemblyProduct("FXMind.MainServer")]
[assembly: AssemblyDescription("FXMind Main Server")]
[assembly: AssemblyCompany("http://facebook.com/sergei.zhuravlev")]
[assembly: AssemblyCopyright("Copyright 2013-2018 Sergei Zhuravlev")]
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