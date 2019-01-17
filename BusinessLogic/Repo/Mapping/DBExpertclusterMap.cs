using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using BusinessLogic.Repo; 

namespace BusinessLogic.Repo {
    
    
    public class DBExpertclusterMap : ClassMap<DBExpertcluster> {
        
        public DBExpertclusterMap() {
			Table("expertcluster");
			LazyLoad();
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			References(x => x.Metasymbol).Column("MetasymbolId");
			References(x => x.Adviser).Column("AdviserId");
			Map(x => x.Name).Column("Name").Not.Nullable();
			Map(x => x.Description).Column("Description");
			Map(x => x.Typ).Column("Typ");
			Map(x => x.Retired).Column("Retired");
        }
    }
}
