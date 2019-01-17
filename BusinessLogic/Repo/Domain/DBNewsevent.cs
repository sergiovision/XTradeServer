using System;
using System.Text;
using System.Collections.Generic;


namespace BusinessLogic.Repo {
    
    public class DBNewsevent : BaseEntity<DBNewsevent> {
        public virtual int Id { get; set; }
        public virtual DBCurrency Currency { get; set; }
        public virtual DateTime Happentime { get; set; }
        public virtual string Name { get; set; }
        public virtual int Importance { get; set; }
        public virtual string Actualval { get; set; }
        public virtual string Forecastval { get; set; }
        public virtual string Previousval { get; set; }
        public virtual DateTime Parsetime { get; set; }
        public virtual int? Raised { get; set; }
    }
}
