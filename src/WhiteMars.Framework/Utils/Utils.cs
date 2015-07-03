using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WhiteMars.Framework
{
    public static class Utils
    {
        public static string XmlSerializeToString(this object obj)
        {
            var xml = XmlSerializeToXElement(obj);
            return xml == null ? null : xml.ToString();
        }

        public static XElement XmlSerializeToXElement(this object obj)
        {
            if (obj is IDictionary<string, object>)
            {
                XElement result;

                if (obj is DynamicDictionary)
                {
                    result = new XElement("DynamicDictionary");
                }
                else
                {
                    result = new XElement("Dictionary");
                }

                foreach (var kv in (IDictionary<string, object>)obj)
                {
                    var value = kv.Value;
                    if (value != null)
                    {
                        var keyNode = new XElement(kv.Key);
                        var xmlValue = XmlSerializeToXElement(kv.Value);
                        if (xmlValue.HasElements)
                        {
                            foreach (var n in xmlValue.Elements())
                            {
                                keyNode.Add(n);
                            }
                        }
                        else
                        {
                            keyNode.Value = xmlValue.Value;
                        }
                        result.Add(keyNode);
                    }
                    else
                    {
                        result.Add(new XElement(kv.Key, kv.Value));
                    }

                }

                return result;
            }
            else if (obj is IList<object>)
            {
                var result = new XElement("List");

                foreach (var o in (IList<object>)obj)
                {
                    var valueNode = XmlSerializeToXElement(o);
                    result.Add(valueNode);
                }

                return result;
            }
            else
            {
                XmlSerializer ser = new XmlSerializer(obj.GetType());
                var sb = new StringBuilder();
                using (var writer = XmlWriter.Create(sb))
                {
                    ser.Serialize(writer, obj);
                    return XElement.Parse(sb.ToString());
                }
            }
        }
        
        public static T XmlDeserialize<T>(string xmlString)
        {
            return (T)XmlDeserialize(xmlString, typeof(T));
        }

        public static object XmlDeserialize(string xmlString, Type outputType)
        {
            XmlSerializer ser = new XmlSerializer(outputType);
            using (var reader = new StringReader(xmlString))
            {
                return ser.Deserialize(reader);
            }
        }

        public static T XmlDeserialize<T>(XElement xml)
        {
            return (T)XmlDeserialize(xml, typeof(T));
        }

        public static object XmlDeserialize(XElement xml, Type outputType)
        {
            XmlSerializer ser = new XmlSerializer(outputType);
            using (var reader = xml.CreateReader())
            {
                return ser.Deserialize(reader);
            }
        }


        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        /// <summary>
        /// This method combines paths with '\'
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="subPaths"></param>
        /// <returns></returns>
        public static string CombinePath(this string basePath, params string[] subPaths)
        {
            if (subPaths != null && subPaths.Length > 0)
            {
                var args = new List<string>();
                args.Add(basePath);
                args.AddRange(subPaths);
                return System.IO.Path.Combine(args.ToArray());
            }
            else
            {
                return basePath;
            }
        }

        /// <summary>
        /// This method combines paths with '/'
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="subPaths"></param>
        /// <returns></returns>
        public static string CombineUrl(this string baseUrl, params string[] subPaths)
        {
            if (baseUrl == null) return null;

            var SEP = '/';
            if (subPaths != null && subPaths.Length > 0)
            {
                // string += will be faster for 4 to 8 strings
                // http://stackoverflow.com/questions/1612797/string-concatenation-vs-string-builder-performance
                // so 5 is using here
                if (subPaths.Length > 5)
                {
                    var result = new StringBuilder(baseUrl.TrimEnd(SEP));
                    foreach (var s in subPaths)
                    {
                        result.Append(SEP);
                        result.Append(s);
                    }
                    return result.ToString();
                }
                else
                {
                    var result = baseUrl.TrimEnd(SEP);
                    foreach (var s in subPaths)
                    {
                        result += SEP + s;
                    }
                    return result;
                }
            }
            else
            {
                return baseUrl;
            }
        }
    }
}
