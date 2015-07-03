using System;
using System.Collections.Generic;
using System.Configuration;

namespace WhiteMars.Framework.Configuration
{
    /// <summary>
    /// WhiteMars Application Config
    /// </summary>
    public class WMApplicationDefault : ConfigurationElement
    {
        public Dictionary<string, string> Configs = new Dictionary<string, string>();

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            if (reader.HasAttributes && reader.MoveToFirstAttribute())
            {
                do
                {
                    var attrName = reader.LocalName;
                    var attrValue = reader.Value;

                    Configs[attrName] = attrValue;
                } while (reader.MoveToNextAttribute());
            }

            //base.DeserializeElement(reader, serializeCollectionKey);
        }
    }
}

