using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Migrations
{
    [Migration(20190103121800)]
    public class Migration1 : Migration
    {
        public override void Up()
        {
            Execute.Sql("UPDATE Terminal SET Broker = 'FXPRO_DEMO_USD' WHERE AccountNumber=5187039");
            Execute.Sql("UPDATE Terminal SET Broker = 'FXPRO_USD' WHERE AccountNumber=5187041");
            Execute.Sql("UPDATE Terminal SET Broker = 'BCS_RUR' WHERE AccountNumber=994741");
            Execute.Sql("UPDATE Terminal SET Broker = 'ALPARI_USD' WHERE AccountNumber=15020825");
            Execute.Sql("UPDATE Terminal SET Broker = 'QUIK_RUR' WHERE AccountNumber=417402");
        }

        public override void Down()
        {
            // Delete.Table("Log");
        }
    }
}
