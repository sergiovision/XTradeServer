using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;

namespace BusinessLogic.Repo {
    
    
    public class DBLaststateMap : ClassMap<DBLaststate> {
        
        public DBLaststateMap() {
			Table("laststate");
            ReadOnly();
			LazyLoad();
            Id(x => x.Name).GeneratedBy.Assigned();
            References(x => x.Wallet).Column("WALLET_ID");
            Map(x => x.Name).Column("NAME");
            Map(x => x.Date).Column("DATE").Not.Nullable();
			Map(x => x.Balance).Column("BALANCE");
        }
    }
}
