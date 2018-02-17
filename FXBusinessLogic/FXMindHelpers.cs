using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DevExpress.Xpo;
using FXBusinessLogic.fx_mind;

namespace FXBusinessLogic
{
    internal static class FXMindHelpers
    {
        #region CryptoHelpers

        public static string Encode(string value)
        {
            SHA1 hash = SHA1.Create();
            var encoder = new ASCIIEncoding();
            byte[] combined = encoder.GetBytes(value ?? "");
            return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
        }

        #endregion

        public static DBSymbol getSymbolByID(Session session, int SymbolID)
        {
            var symbolsQuery = new XPQuery<DBSymbol>(session);
            IQueryable<DBSymbol> symbols = from c in symbolsQuery
                where c.ID == SymbolID
                select c;
            if (symbols.Count() > 0)
            {
                return symbols.First();
            }
            return null;
        }

        public static DBSymbol getSymbolID(Session session, string SymbolStr)
        {
            var symbolsQuery = new XPQuery<DBSymbol>(session);
            IQueryable<DBSymbol> symbols = from c in symbolsQuery
                where c.Name == SymbolStr
                select c;
            if (symbols.Any())
            {
                return symbols.First();
            }
            return null;
        }

        public static DBCurrency getCurrencyID(Session session, string currencyStr)
        {
            var qCurrency = new XPQuery<DBCurrency>(session);
            IQueryable<DBCurrency> varQCurrency = from c in qCurrency
                where c.Name == currencyStr
                select c;
            if (varQCurrency.Any())
            {
                return varQCurrency.First();
            }
            return null;
        }

        public static IQueryable<DBSymbol> getTechSymbols(Session session)
        {
            var symbolsQuery = new XPQuery<DBSymbol>(session);
            IQueryable<DBSymbol> symbols = from c in symbolsQuery
                                           where c.Use4Tech
                                           select c;
            return symbols;
        }



        #region GlobalVars

        public static void SetGlobalVar(Session session, string name, string value)
        {
            var gvarsQuery = new XPQuery<DBSettings>(session);
            IQueryable<DBSettings> gvars = from c in gvarsQuery
                where c.PropertyName == name
                select c;
            if (gvars.Count() > 0)
            {
                DBSettings gvar = gvars.First();
                gvar.Value = value;
                session.Save(gvar);
            }
            else
            {
                var gvar = new DBSettings(session);
                gvar.PropertyName = name;
                gvar.Value = value;
                session.Save(gvar);
            }
        }

        public static string GetGlobalVar(Session session, string name)
        {
            var gvarsQuery = new XPQuery<DBSettings>(session);
            IQueryable<DBSettings> gvars = from c in gvarsQuery
                where c.PropertyName == name
                select c;
            if (gvars.Count() > 0)
            {
                DBSettings gvar = gvars.First();
                return gvar.Value;
            }
            return null;
        }

        #endregion

        #region Technicals

        // returns action ID
        public static short GetActionId(string actionName)
        {
            string action = actionName.ToUpper();
            switch (action)
            {
                case "STRONG BUY":
                    return 2;
                case "BUY":
                    return 1;
                case "NEUTRAL":
                    return 0;
                case "SELL":
                    return -1;
                case "STRONG SELL":
                    return -2;
                default:
                    return 0;
            }
        }


        // returns action ID
        public static decimal ActionIdToPercent(int action)
        {
            switch (action)
            {
                case 2:
                    return new decimal(100.0);
                case 1:
                    return new decimal(50.0);
                case 0:
                    return new decimal(0.0);
                case -1:
                    return new decimal(-50.0);
                case -2:
                    return new decimal(-100.0);
                default:
                    return new decimal(0.0);
            }
        }

        #endregion
    }
}