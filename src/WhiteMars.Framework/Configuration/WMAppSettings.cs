using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;


namespace WhiteMars.Framework.Configuration
{
    public class WMAppSettings : ConfigurationElement, IReadOnlyDictionary<string, string>
    {
        private Dictionary<string, string> data = new Dictionary<string, string>();

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            var outxml = reader.ReadOuterXml();

            var ele = XElement.Parse(outxml);

            foreach (var e in ele.Elements())
            {
                if (e.Name == "add")
                {
                    var keyAttr = e.Attribute("key");
                    var valueAttr = e.Attribute("value");

                    string key = null, value = null;

                    if (keyAttr != null) key = keyAttr.Value;
                    if (valueAttr != null) value = valueAttr.Value;

                    if (key != null) this.data.Add(key, value);
                }
                else if (e.Name == "remove")
                {
                    string key = null;
                    var keyAttr = e.Attribute("key");
                    if (keyAttr != null) key = keyAttr.Value;

                    if (key != null) this.data.Remove(key);
                }
            }
        }

        #region IReadOnlyDictionary implementation

        public bool ContainsKey(string key)
        {
            return this.data.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return this.data.TryGetValue(key, out value);
        }

        public new string this [string indexer]
        {
            get { return this.data[indexer]; }
        }


        public IEnumerable<string> Keys
        {
            get
            {
                return this.data.Keys;
            }
        }

        public IEnumerable<string> Values
        {
            get
            {
                return this.data.Values;
            }
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        #endregion

        #region IReadOnlyCollection implementation

        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }

        #endregion
    }
}

