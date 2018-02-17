using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FXBusinessLogic.eToro
{
    public class EToroJsonInstrumentParseProvider : JSONParserProvider<InstrumentRate>
    {
        public override InstrumentRate Parse(string data)
        {
            JObject vdata = JObject.Parse(data);
            return JsonConvert.DeserializeObject<InstrumentRate>(vdata.ToString());
        }
    }
}