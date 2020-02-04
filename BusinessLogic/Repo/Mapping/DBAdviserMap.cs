using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBAdviserMap : ClassMap<DBAdviser>
    {
        public DBAdviserMap()
        {
            Table("adviser");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            References(x => x.Terminal).Column("TerminalId");
            References(x => x.Symbol).Column("SymbolId");
            References(x => x.Cluster).Column("ClusterId");
            Map(x => x.Name).Column("Name");
            Map(x => x.Timeframe).Column("Timeframe");
            Map(x => x.Running).Column("Running");
            Map(x => x.Disabled).Column("Disabled");
            // Map(x => x.State).Column("State");
            Map(x => x.Lastupdate).Column("Lastupdate");
            Map(x => x.Closereason).Column("Closereason");
        }
    }
}