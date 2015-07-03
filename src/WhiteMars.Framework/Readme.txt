WhiteMars.Framework
-----------------------------------------------

Configuration entires
===========================
1. <add key="TenantMetaLocation" value="/var/www/whitemars/TenantMetaXml.xml"/>
Specifies the tenant info location.
With different ITenantMetaProvider, the location will be different.
The default ITenantMetaProvider is WhiteMars.Framework.XmlFileTenantMetaProvider.
The TenantMetaLocation is the TenantMetaXml.xml file full path


2. <add key="CommonadTimeout" value="120"/>
Specifies the database command timeout default value. The value is in seconds.


3. TenantMeta.xml



