using Newtonsoft.Json;

namespace FXBusinessLogic
{
    public class JSON
    {
        public static T Parse<T>(string data) where T : IJSONObject
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}