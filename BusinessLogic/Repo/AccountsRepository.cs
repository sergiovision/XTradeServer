using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using NHibernate;

namespace BusinessLogic.Repo
{
    public class AccountsRepository : BaseRepository<DBAccount>
    {
        private readonly static object lockObject = new object();

        public AccountsRepository()
        {
        }

        public List<Account> GetAccounts()
        {
            List<Account> results = new List<Account>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var accounts = Session.Query<DBAccount>();
                foreach (var dbacc in accounts)
                {
                    results.Add(toDTO(dbacc));
                }
            }
            return results;
        }

        public List<Terminal> GetTerminals()
        {
            List<Terminal> results = new List<Terminal>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var terminals = Session.Query<DBTerminal>().OrderBy(x => x.Disabled);
                foreach (var dbt in terminals)
                {
                    results.Add(toDTO(dbt));
                }
            }
            return results;
        }

        public bool UpdateTerminals(Terminal t)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                DBTerminal terminal = Session.Get<DBTerminal>(t.Id);
                if (terminal == null)
                    return false;
                using (ITransaction Transaction = Session.BeginTransaction())
                {
                    terminal.Disabled = t.Disabled;
                    terminal.Stopped = t.Stopped;
                    Session.Update(terminal);
                    Transaction.Commit();
                    return true;
                }
            }
        }

        public void UpdateBalance(int AccountNumber, decimal Balance, decimal Equity)
        {
            lock (lockObject)
            {

                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var terms = Session.Query<DBTerminal>().Where(x => (x.Accountnumber == AccountNumber));
                    if ((terms == null) || (terms.Count() <= 0))
                        return;
                    DBTerminal terminal = terms.FirstOrDefault();
                    if (terminal == null)
                        return;
                    if (terminal.Account == null)
                        return;
                    using (ITransaction Transaction = Session.BeginTransaction())
                    {
                        terminal.Account.Balance = Balance;
                        terminal.Account.Equity = Equity;
                        terminal.Account.Lastupdate = DateTime.UtcNow;
                        Session.Update(terminal);
                        Transaction.Commit();
                    }
                    var acc = Session.Query<DBAccountstate>().Where(x => (x.Account.Id == terminal.Account.Id)).OrderByDescending(x => x.Date);
                    using (ITransaction Transaction = Session.BeginTransaction())
                    {
                        if (acc.Any())
                        {
                            DBAccountstate state = null;
                            state = acc.FirstOrDefault();
                            if ((state == null) || (state.Date.DayOfYear != DateTime.Today.DayOfYear))
                            {
                                var newstate = new DBAccountstate();
                                if (state == null)
                                    newstate.Account = terminal.Account;
                                else
                                    newstate.Account = state.Account;
                                newstate.Balance = Balance;
                                newstate.Comment = "Autoupdate";
                                newstate.Date = DateTime.UtcNow;
                                Session.Save(newstate);
                            }
                            else
                            {
                                state.Balance = Balance;
                                state.Comment = "Autoupdate";
                                state.Date = DateTime.UtcNow;
                                Session.Update(state);
                            }
                            Transaction.Commit();
                        }
                    }
                }
            }
        }

        public static Account toDTO(DBAccount a)
        {
            Account result = new Account();
            result.Id = a.Id;
            result.Number = a.Number;
            result.Balance = a.Balance;
            if (a.Terminal != null)
                result.TerminalId = a.Terminal.Id;
            result.Lastupdate = a.Lastupdate;
            result.Description = a.Description;
            result.Equity = a.Equity;
            if (a.Person != null)
                result.PersonId = a.Person.Id;
            if (a.Currency != null)
                result.CurrencyStr = a.Currency.Name;
            result.Retired = a.Retired;
            if (a.Wallet != null)
                result.WalletId = a.Wallet.Id;
            return result;
        }
        public Terminal toDTO(DBTerminal t)
        {
            Terminal result = new Terminal();
            result.AccountNumber = t.Accountnumber.Value;
            result.Broker = t.Broker;
            result.CodeBase = t.Codebase;
            result.Disabled = t.Disabled;
            result.FullPath = t.Fullpath;
            result.Demo = t.Demo;
            result.Stopped = t.Stopped;
            result.Id = t.Id;
            if (t.Account != null)
                result.Currency = t.Account.Currency.Name;
            return result;
        }

    }
}
