using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using BusinessLogic.Repo;

namespace BusinessLogic.Repo
{
    public class DBStatsdateMap : ClassMap<DBStatsdate>
    {
        public DBStatsdateMap()
        {
            Table("statsdate");
            LazyLoad();
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Typ).Column("Typ").Not.Nullable();
            Map(x => x.Dailyvalue).Column("DailyValue");
            Map(x => x.Weeklyvalue).Column("WeeklyValue");
            Map(x => x.Lastupdate).Column("Lastupdate");
            Map(x => x.Monthly).Column("Monthly");
        }
    }
}