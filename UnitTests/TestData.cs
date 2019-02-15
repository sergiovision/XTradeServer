using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;

namespace UnitTests
{
    public class TestData
    {
        public static short testPort = 2014;

        public static string TestURL => $"http://localhost:{testPort}";

        public static string UserName => "admin@sergego.com";

        public static string Password => "654321X";

        #region Wallet

        public static AccountState getTestAccount()
        {
            AccountState aState = new AccountState();
            aState.AccountId = 100;
            aState.Balance = 1;
            aState.Date = DateTime.UtcNow;
            aState.Comment = "Test";
            return aState;
        }

        #endregion
    }
}