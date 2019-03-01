using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Rates
    {
        public virtual int Id { get; set; }
        public string MetaSymbol { get; set; }
        public string C1 { get; set; }
        public string C2 { get; set; }
        public virtual decimal Ratebid { get; set; }
        public virtual decimal Rateask { get; set; }
        public virtual DateTime Lastupdate { get; set; }
        public virtual bool Retired { get; set; }
    }
}