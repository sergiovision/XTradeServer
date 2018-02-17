using DevExpress.Xpo;

namespace FXBusinessLogic.fx_mind
{
    public partial class DBSite
    {
        public DBSite(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
    }
}