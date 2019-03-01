using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo
{
    public class DBStatsymbol : BaseEntity<DBStatsymbol>
    {
        public virtual int Id { get; set; }
        public virtual DBMetasymbol Metasymbol { get; set; }
        public virtual string Averagevalue { get; set; }
        public virtual DateTime? Lastupdate { get; set; }
    }
}