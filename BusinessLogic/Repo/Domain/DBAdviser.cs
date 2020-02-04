using System;
using System.Text;
using System.Collections.Generic;

namespace BusinessLogic.Repo
{
    public class DBAdviser : BaseEntity<DBAdviser>
    {
        public virtual int Id { get; set; }
        public virtual DBTerminal Terminal { get; set; }
        public virtual DBSymbol Symbol { get; set; }
        public virtual string Name { get; set; }
        public virtual string Timeframe { get; set; }
        public virtual bool Running { get; set; }
        public virtual bool Disabled { get; set; }
        // public virtual string State { get; set; }
        public virtual string SaveOrders { get; set; }
        public virtual DateTime? Lastupdate { get; set; }
        public virtual int? Closereason { get; set; }
        public virtual DBExpertcluster Cluster { get; set; }
    }
}