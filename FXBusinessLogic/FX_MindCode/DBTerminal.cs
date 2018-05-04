using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;

namespace FXBusinessLogic.fx_mind
{

    public partial class DBTerminal
    {
        public DBTerminal(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
