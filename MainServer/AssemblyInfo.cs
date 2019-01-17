using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

//#if NET_40

[assembly: AssemblyConfiguration("net-4.7.win64; Release")]

[assembly: AssemblyProduct("XTrade.MainServer")]
[assembly: AssemblyDescription("XTrade Main Server")]
[assembly: AssemblyCompany("https://www.sergego.com")]
[assembly: AssemblyCopyright("Copyright 2013-2018 Sergei Zhuravlev")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
//[assembly: CLSCompliant(true)]

[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.0.0")]

#if STRONG
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("XTrade.MainServer.Net.snk")]
[assembly:AllowPartiallyTrustedCallers]
#endif

[assembly: AssemblyTitle("XTrade.MainServer")]
[assembly: XmlConfigurator(Watch = true)]
