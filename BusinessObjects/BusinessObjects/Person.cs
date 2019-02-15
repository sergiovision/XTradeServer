using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Person
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public DateTime Created { get; set; }
        public int Languageid { get; set; }
        public string Credential { get; set; }
        public string Regip { get; set; }
        public string Mail { get; set; }
        public string Privilege { get; set; }
        public string Uuid { get; set; }
        public string Activated { get; set; }
        public bool Retired { get; set; }
    }
}