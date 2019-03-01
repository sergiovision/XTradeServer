using Newtonsoft.Json;

namespace BusinessLogic
{
    public class JSON
    {
        public static T Parse<T>(string data) where T : IJSONObject
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}