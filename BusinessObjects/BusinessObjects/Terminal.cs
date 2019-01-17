using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Terminal
    {
        public int Id { get; set; }
        public long AccountNumber { get; set; }

        public string Broker { get; set; }

        public string FullPath { get; set; }
        public string CodeBase { get; set; }
        public bool Disabled { get; set; }
        public bool Demo { get; set; }
        public bool Stopped { get; set; }
    }
}