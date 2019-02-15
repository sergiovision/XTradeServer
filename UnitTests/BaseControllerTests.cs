using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Moq;
using XTrade.MainServer;
using Autofac;
using UnitTests.WeBAPITest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTests
{
    public class BaseControllerTests
    {
        public static TestData data = new TestData();

        public static HttpWebApi httpWebApi;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            var result = httpWebApi.Login(TestData.UserName, TestData.Password);
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(!string.IsNullOrEmpty(result.Result.access_token));
        }
    }
}