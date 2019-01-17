using Autofac;
using XTrade.MainServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.WeBAPITest;

namespace UnitTests
{
    [TestClass]
    public static class InitTestsClass
    {
        private static bool bInited = false;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            if (bInited)
                Assert.Fail("AssemblyInit should be called only ONCE!!!");
            Program.RegisterContainer();
            var server = Program.Container.Resolve<QuartzServer>();
            server.Initialize(TestData.testPort);
            server.Start();
            BaseControllerTests.httpWebApi = new HttpWebApi(TestData.TestURL);
            bInited = true;
        }
    }
}
