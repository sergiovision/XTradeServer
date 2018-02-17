using DevExpress.Xpo;

namespace FXBusinessLogic.fx_mind
{
    public partial class DBNewsEvent
    {
        public DBNewsEvent(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
    }
}