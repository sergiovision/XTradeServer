    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public interface IDataService
    {
        List<CurrencyInfo> GetCurrencies();
        List<Terminal> GetTerminals();
        List<Adviser> GetAdvisers();
        List<ExpertsCluster> GetClusters();
        List<Account> GetAccounts();
        List<DealInfo> GetDeals();
        List<Wallet> GetWalletsState(DateTime forDate);
        string GetGlobalProp(string name);
        void SetGlobalProp(string name, string value);
        Person LoginPerson(string mail, string password);
        bool UpdateTerminals(Terminal t);
        bool UpdateAdviser(Adviser adviser);
        void UpdateBalance(int TerminalId, decimal Balance, decimal Equity);
        void SaveDeals(List<DealInfo> deals);
        decimal ConvertToUSD(decimal value, string valueCurrency);

    }
}

