using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using NHibernate;

namespace BusinessLogic.Repo
{
    public class WalletsRepository : BaseRepository<DBAccount>
    {
        private readonly DataService parent;

        public WalletsRepository(DataService p)
        {
            parent = p;
        }

        public List<Wallet> GetWallets()
        {
            List<Wallet> results = new List<Wallet>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var wallets = Session.Query<DBWallet>();
                foreach (var dbw in wallets) results.Add(toDTO(dbw));
            }

            return results;
        }

        public List<Wallet> GetWalletsState(DateTime forDate)
        {
            List<Wallet> results = new List<Wallet>();
            try
            {
                using (ISession Session = ConnectionHelper.CreateNewSession())
                {
                    var rateList = Session.Query<DBRates>().Where(x => x.Retired == false).ToList();
                    IQueryable<DBWallet> wallets = null;
                    if (forDate == DateTime.MaxValue)
                        wallets = Session.Query<DBWallet>()
                            .Where(x => x.Retired == false && !x.Name.Equals("test"));
                    else
                        wallets = Session.Query<DBWallet>().Where(x => !x.Name.Equals("test"));

                    foreach (var dbw in wallets)
                    {
                        Wallet wallet = toDTO(dbw);
                        decimal balance = 0;
                        IQueryable<DBAccount> accounts = null;
                        if (forDate == DateTime.MaxValue)
                            accounts = Session.Query<DBAccount>()
                                .Where(x => x.Wallet.Id == wallet.Id && x.Retired == false);
                        else
                            accounts = Session.Query<DBAccount>().Where(x => x.Wallet.Id == wallet.Id);

                        foreach (var acc in accounts)
                        {
                            var account = AccountsRepository.toDTO(acc);
                            DBAccountstate accState = null;
                            IQueryable<DBAccountstate> accResults = null;
                            if (forDate.Equals(DateTime.MaxValue))
                                accResults = Session.Query<DBAccountstate>()
                                    .Where(x => x.Account.Id == acc.Id)
                                    .OrderByDescending(x => x.Date);
                            else
                                accResults = Session.Query<DBAccountstate>()
                                    .Where(x => x.Account.Id == acc.Id && x.Date <= forDate)
                                    .OrderByDescending(x => x.Date);

                            if (accResults == null || accResults.Count() == 0)
                                continue;
                            // acc.Currency.Id
                            accState = accResults.FirstOrDefault();
                            if (accState != null)
                            {
                                account.Balance = accState.Balance;
                                decimal value = account.Balance;
                                if (acc.Currency != null)
                                    value = parent.ConvertToUSD(account.Balance, acc.Currency.Name);
                                balance += value;
                            }

                            wallet.Accounts.Add(account);
                        }

                        wallet.Balance = balance;
                        results.Add(wallet);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("GetWalletsState for Date: " + forDate.ToString(xtradeConstants.MTDATETIMEFORMAT) +
                          e);
            }

            return results;
        }

        public Wallet toDTO(DBWallet w)
        {
            Wallet result = new Wallet();
            result.Id = w.Id;
            result.Name = w.Name;
            if (w.Person != null)
                result.PersonId = w.Person.Id;
            result.Retired = w.Retired;
            result.Shortname = w.Shortname;
            if (w.Site != null)
                result.SiteId = w.Site.Id;
            result.Accounts = new List<Account>();
            return result;
        }
    }
}