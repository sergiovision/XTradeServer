using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo {
    
    public class DBExpertcluster : BaseEntity<DBExpertcluster> {
        public virtual int Id { get; set; }
        public virtual DBMetasymbol Metasymbol { get; set; }
        public virtual DBAdviser Adviser { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int? Typ { get; set; }
        public virtual bool Retired { get; set; }
    }
}
