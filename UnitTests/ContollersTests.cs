using System;
using System.Collections.Generic;
using BusinessObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class ContollersTests : BaseControllerTests
    {
        [TestMethod]
        public void TestJobs()
        {
            var result = httpWebApi.Get("/api/jobs");
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(!string.IsNullOrEmpty(result.Result));
            List<ScheduledJobView> list = JsonConvert.DeserializeObject<List<ScheduledJobView>>(result.Result);
            Assert.IsTrue(list.Count > 0);
            var result2 = httpWebApi.Get("/api/jobs/GetRunning");
            Assert.IsNotNull(result2.Result);
            List<ScheduledJobView> list2 = JsonConvert.DeserializeObject<List<ScheduledJobView>>(result2.Result);
            Assert.IsTrue(list2.Count == 0);
        }

        [TestMethod]
        public void TestExperts()
        {
            var result2 = httpWebApi.Get("/api/experts/GetTerminals");
            Assert.IsNotNull(result2.Result);
            List<Terminal> list2 = JsonConvert.DeserializeObject<List<Terminal>>(result2.Result);
            Assert.IsTrue(list2.Count > 0);

            var result = httpWebApi.Get("/api/experts");
            Assert.IsNotNull(result.Result);
            Assert.IsTrue(!string.IsNullOrEmpty(result.Result));
            List<Adviser> list = JsonConvert.DeserializeObject<List<Adviser>>(result.Result);
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void TestWallets()
        {
            var result2 = httpWebApi.Get("/api/wallets");
            Assert.IsNotNull(result2.Result);
            List<Wallet> list2 = JsonConvert.DeserializeObject<List<Wallet>>(result2.Result);
            Assert.IsTrue(list2.Count > 0);
        }

        [TestMethod]
        public void TestWalletsRange()
        {
            DateTime fromDate = new DateTime(2018, 1, 1); // DateTime.UtcNow.AddMonths(-3);
            DateTime toDate = new DateTime(2018, 1, 5); // DateTime.UtcNow;
            int walletId = 0;
            var result = httpWebApi.Get($"/api/wallets/GetRange?id={walletId}&fromDate={fromDate}&toDate={toDate}");
            Assert.IsNotNull(result.Result);
            List<Wallet> list = JsonConvert.DeserializeObject<List<Wallet>>(result.Result);
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void UpdateWallet()
        {
            var aState = TestData.getTestAccount();
            string jsonString = JsonConvert.SerializeObject(aState);
            var result2 = httpWebApi.PutJson("/api/wallets/Put", jsonString);
            Assert.IsTrue(string.IsNullOrEmpty(result2.Result));
        }

        [TestMethod]
        public void TestNews()
        {
            DateTime now = DateTime.UtcNow;
            now = now.AddDays(-3);
            string query = $"?datetime={now.ToShortDateString()}&symbol=EURUSD&importance=0&timezoneoffset=-3";
            var result2 = httpWebApi.Get("/api/news" + query);
            Assert.IsNotNull(result2.Result);
            List<NewsCalendarEvent> list2 = JsonConvert.DeserializeObject<List<NewsCalendarEvent>>(result2.Result);
            Assert.IsTrue(list2.Count >= 0);
        }
    }
}