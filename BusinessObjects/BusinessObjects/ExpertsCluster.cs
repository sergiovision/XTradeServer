using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class ExpertsCluster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaSymbol { get; set; }
        public int MetaSymbolId { get; set; }
        public int MasterAdviserId { get; set; }
        public int Typ { get; set; }
        public bool Retired { get; set; }
        public List<Adviser> Advisers { get; set; }
    }
}