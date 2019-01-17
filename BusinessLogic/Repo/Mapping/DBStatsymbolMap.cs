using System; 
using System.Collections.Generic; 
using System.Text; 
using FluentNHibernate.Mapping;
using BusinessLogic.Repo; 

namespace BusinessLogic.Repo {
    
    
    public class DBStatsymbolMap : ClassMap<DBStatsymbol> {
        
        public DBStatsymbolMap() {
			Table("statsymbol");
			LazyLoad();
			Id(x => x.Id).GeneratedBy.Identity().Column("Id");
			References(x => x.Metasymbol).Column("MetasymbolId");
			Map(x => x.Averagevalue).Column("AverageValue");
			Map(x => x.Lastupdate).Column("Lastupdate");
        }
    }
}
