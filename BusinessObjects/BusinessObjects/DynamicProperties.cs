

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace BusinessObjects
{
    public enum EntitiesEnum
    {
        Undefined,
        Account,
        Settings,
        Adviser,
        MetaSymbol,
        Symbol,
        Terminal,
        Deals,
        Jobs,
        Country,
        Currency,
        ExpertCluster,
        NewsEvent,
        Person,
        Rates,
        Site,
        Wallet
    }

    public class DynamicProperties
    {
        public int ID { get; set; }
        public int objId { get; set; }
        public short entityType { get; set; }
        public string Vals { get; set; }
        public DateTime? updated { get; set; }
    }

    public class DynamicProperty
    {
        public string type;
        public string name;
        public string group;
        public string value;
    }

    public class DefaultProperties {
        public static Dictionary<string, DynamicProperty> fillProperties(ref Dictionary<string, DynamicProperty> result, EntitiesEnum etype, int id, int objid, string value)
        {
            DynamicProperty p1 = new DynamicProperty()
            {
                type = "integer",
                name = "ID",
                group = "System",
                value = id.ToString()
            };
            if (!result.ContainsKey(p1.name))
                result.Add(p1.name, p1);
            else
                result[p1.name] = p1;
            DynamicProperty p2 = new DynamicProperty()
            {
                type = "integer",
                name = "ObjectID",
                group = "System",
                value = objid.ToString()
            };
            if (!result.ContainsKey(p2.name))
                result.Add(p2.name, p2);
            else
                result[p2.name] = p2;
            DynamicProperty p3 = new DynamicProperty()
            {
                type = "integer",
                group = "Specification"
            };
            switch (etype)
            {
                case EntitiesEnum.MetaSymbol:
                    p3.name = "Levels";
                    p3.value = value;
                    if (!result.ContainsKey(p3.name))
                        result.Add(p3.name, p3);
                    else
                        result[p3.name] = p3;
                    break;
                case EntitiesEnum.Terminal:
                    p3.name = "Risk Per Day";
                    p3.value = value;
                    if (!result.ContainsKey(p3.name))
                        result.Add(p3.name, p3);
                    else
                        result[p3.name] = p3;
                    break;
            }
            return result;
        }

    }

}
