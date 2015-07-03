using System;
using System.Xml;
using System.Xml.Linq;

namespace WhiteMars.Framework
{
    /// <summary>
    /// The tenant meta provider in xml file
    /// The class requires AppSettings["TenantMetaLocation"]
    /// </summary>
    public class StaticXmlFileTenantMetaProvider : ITenantMetaProvider
    {
        #region ITenantMetaProvider implementation

        public TenantMetaCollection GetTenantMetaCollection()
        {
            return metas;
        }

        #endregion

        static TenantMetaCollection metas = new TenantMetaCollection();
        static readonly string xmlFilePath;

        static StaticXmlFileTenantMetaProvider()
        {
            xmlFilePath = WhiteMarsApplication.ConfigSection.AppSettings["TenantMetaLocation"];

            if (!IOHelper.FileExists(xmlFilePath)) throw new WhiteMarsException(string.Format("TenantMetaLocation '{0}' not found.", xmlFilePath));

            var xml = IOHelper.ReadAllText(xmlFilePath);

            var xdoc = XDocument.Parse(xml);

            foreach (var nodes in xdoc.Root.Elements("{http://www.whitemars.com/xmlnamespaces/WhiteMars.Framework/TenantMeta}TenantMeta"))
            {
                var x = nodes.ToString();
                var meta = Utils.XmlDeserialize<TenantMeta>(x);
                metas.Add(meta);
            }

        }

        /// <summary>
        /// Create a new instance of XmlFileTenantMetaProvider
        /// </summary>
        public StaticXmlFileTenantMetaProvider()
        {
            // do nothing
        }


    }
}

