using DevExpress.Xpo;

namespace FXBusinessLogic.fx_mind
{
    public partial class DBSettings
    {
        public DBSettings(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
    }
}