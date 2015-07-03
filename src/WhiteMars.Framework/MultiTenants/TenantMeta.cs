using System;
using System.Xml.Serialization;

namespace WhiteMars.Framework
{
	/// <summary>
	/// The entity class for TenantMeta
	/// </summary>
    [Serializable]
    [XmlRoot("TenantMeta", Namespace = "http://www.whitemars.com/xmlnamespaces/WhiteMars.Framework/TenantMeta")]
	public class TenantMeta
	{
		public TenantMeta()
		{
			// do nothing
		}

		/// <summary>
		/// Gets or sets the unique URL.
		/// </summary>
		/// <value>The unique URL.</value>
        [XmlElement]
		public string UniqueUrl { get; set; }

		/// <summary>
		/// Gets or sets the db connection string.
		/// </summary>
		/// <value>The db connection string.</value>
        [XmlElement]
		public string DbConnectionString { get; set; }

		/// <summary>
		/// Gets or sets the db provider.
		/// </summary>
		/// <value>The db provider.</value>
        [XmlElement]
		public string DbProvider { get; set; }

		/// <summary>
		/// Gets or sets the command timeout.
		/// </summary>
		/// <value>The command timeout.</value>
        [XmlElement]
		public int? CommandTimeout { get; set; }

	}
}

