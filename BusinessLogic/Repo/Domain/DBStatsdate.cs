using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBStatsdate : BaseEntity<DBStatsdate>
    {
        public virtual int Id { get; set; }
        public virtual int Typ { get; set; }
        public virtual string Dailyvalue { get; set; }
        public virtual string Weeklyvalue { get; set; }
        public virtual DateTime? Lastupdate { get; set; }
        public virtual string Monthly { get; set; }
    }
}