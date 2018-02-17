namespace FXBusinessLogic
{
    public abstract class JSONParserProvider<T> where T : IJSONObject
    {
        public abstract T Parse(string data);
    }
}