

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Drawing;

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
        public string value;
    }

    public class TDynamicProperty<T>
    {
        public string type;
        public T value;
    }

    public class DefaultProperties {
        public static Dictionary<string, DynamicProperty> fillProperties(ref Dictionary<string, DynamicProperty> result, EntitiesEnum etype, int id, int objid, string value)
        {
            DynamicProperty p1 = new DynamicProperty()
            {
                type = "integer",
                value = id.ToString()
            };
            if (!result.ContainsKey("ID"))
                result.Add("ID", p1);
            else
                result["ID"] = p1;
            DynamicProperty p2 = new DynamicProperty()
            {
                type = "integer",
                value = objid.ToString()
            };
            if (!result.ContainsKey("ObjectID"))
                result.Add("ObjectID", p2);
            else
                result["ObjectID"] = p2;
            DynamicProperty p3 = new DynamicProperty()
            {
                type = "integer",
            };
            return result;
        }

        public static int RGBtoInt(int r, int g, int b)
        {
            return (r << 0) | (g << 8) | (b << 16);
        }

        public static Dictionary<string, object> transformProperties(Dictionary<string, DynamicProperty> dbProps)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if ((dbProps == null)||(dbProps.Count == 0))
                return result;
            foreach( var prop in dbProps)
            {
                switch(prop.Value.type)
                {
                    case "integer":
                        result.Add(prop.Key, int.Parse(prop.Value.value));
                        break;
                    case "hexinteger":
                        {
                           string hexValue = prop.Value.value;
                           if (hexValue.StartsWith("#"))
                                hexValue = hexValue.Substring(1);
                           int value = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                           Color c = Color.FromArgb(value);
                           value = RGBtoInt(c.R, c.G, c.B);
                           result.Add(prop.Key, value);
                        }
                        break;
                    case "double":
                        result.Add(prop.Key, double.Parse(prop.Value.value));
                        break;
                    case "boolean":
                        result.Add(prop.Key, bool.Parse(prop.Value.value));
                        break;
                    default:
                        result.Add(prop.Key, prop.Value.value);
                        break;
                }
            }
            return result;

        }

    }

}
