using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using NHibernate;

namespace BusinessLogic.Repo
{
    public class ExpertsRepository : BaseRepository<DBAdviser>
    {
        public ExpertsRepository()
        {
        }

        public List<Adviser> GetAdvisers()
        {
            List<Adviser> results = new List<Adviser>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var advisers = Session.Query<DBAdviser>();
                foreach (var dbadv in advisers)
                {
                    Adviser adv = new Adviser();
                    toDTO(dbadv, ref adv);
                    results.Add(adv);
                }
            }
            return results;
        }

        public List<ExpertsCluster> GetClusters()
        {
            List<ExpertsCluster> results = new List<ExpertsCluster>();
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                var clusters = Session.Query<DBExpertcluster>();
                foreach (var dbCluster in clusters)
                {
                    ExpertsCluster cluster = toDTO(dbCluster);
                    var advisers = Session.Query<DBAdviser>().Where(x=> (x.Cluster != null) && (x.Cluster.Id == cluster.Id) );
                    foreach (var dbAdviser in advisers)
                    {
                        Adviser adv = new Adviser();
                        toDTO(dbAdviser, ref adv);
                        cluster.Advisers.Add(adv);
                    }
                    results.Add(cluster);
                }
            }
            return results;
        }

        public  bool UpdateAdviser(Adviser adv)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                DBAdviser adviser = Session.Get<DBAdviser>(adv.Id);
                if (adviser == null)
                    return false;
                using (ITransaction Transaction = Session.BeginTransaction())
                {
                    if (!String.IsNullOrEmpty(adv.State))
                        adviser.State = adv.State;
                    Session.Update(adviser);
                    Transaction.Commit();
                    return true;
                }
            }
        }

        public static bool UpdateAdviser(DBAdviser adv)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                using (ITransaction Transaction = Session.BeginTransaction())
                {
                    Session.Update(adv);
                    Transaction.Commit();
                    return true;
                }
            }
        }

        public static void toDTO(DBAdviser adv, ref Adviser result)
        {
            result.Id = adv.Id;
            result.Name = adv.Name;
            result.Running = adv.Running;
            if (adv.Closereason!= null)
                result.CloseReason = adv.Closereason.Value;
            if (adv.Terminal != null)
            {
                result.TerminalId = adv.Terminal.Id;
                result.CodeBase = adv.Terminal.Codebase;
                result.Broker = adv.Terminal.Broker;
                result.FullPath = adv.Terminal.Fullpath;
                if (adv.Terminal.Accountnumber != null) 
                    result.AccountNumber = adv.Terminal.Accountnumber.Value;
            }
            result.LastUpdate = adv.Lastupdate.Value;
            result.Disabled = adv.Disabled;
            if (adv.Symbol != null)
            {
                result.Symbol = adv.Symbol.Name;
                result.SymbolId = adv.Symbol.Id;
            }
            if (adv.Cluster != null)
            {
                result.ClusterId = adv.Cluster.Id;
                if (adv.Cluster.Metasymbol != null)
                    result.MetaSymbol = adv.Cluster.Metasymbol.Name;
                else
                    result.MetaSymbol = adv.Symbol.Name;
            }
            result.Timeframe = adv.Timeframe;
            result.State = adv.State;
            
        }

        public static ExpertsCluster toDTO(DBExpertcluster cluster)
        {
            ExpertsCluster result = new ExpertsCluster();
            result.Id = cluster.Id;
            result.Name = cluster.Name;
            result.Retired = cluster.Retired;
            if (cluster.Typ!= null)
                result.Typ = cluster.Typ.Value;
            if (cluster.Metasymbol != null)
            {
                result.MetaSymbol = cluster.Metasymbol.Name;
            }

            if (cluster.Adviser != null)
            {
                result.MasterAdviserId = cluster.Adviser.Id;
            }

            result.Advisers = new List<Adviser>();
            return result;
        }
    }
}
