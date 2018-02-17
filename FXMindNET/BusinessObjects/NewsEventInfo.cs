using System;

namespace BusinessObjects
{
    public class NewsEventInfo
    {
        public string Currency { get; set; }
        public string Name { get; set; }
        public byte Importance { get; set; }
        public DateTime RaiseDateTime { get; set; }
    }
}