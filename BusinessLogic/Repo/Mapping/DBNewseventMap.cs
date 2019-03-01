using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBNewseventMap : ClassMap<DBNewsevent>
    {
        public DBNewseventMap()
        {
            Table("newsevent");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("ID");
            References(x => x.Currency).Column("CurrencyId");
            Map(x => x.Happentime).Column("HappenTime").Not.Nullable();
            Map(x => x.Name).Column("Name").Not.Nullable();
            Map(x => x.Importance).Column("Importance").Not.Nullable();
            Map(x => x.Actualval).Column("ActualVal");
            Map(x => x.Forecastval).Column("ForecastVal");
            Map(x => x.Previousval).Column("PreviousVal");
            Map(x => x.Parsetime).Column("ParseTime").Not.Nullable();
            Map(x => x.Raised).Column("Raised");
        }
    }
}