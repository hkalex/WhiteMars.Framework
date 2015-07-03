using System;
using NUnit.Framework;

namespace WhiteMars.Framework.UnitTest
{
    [TestFixture()]
    public class WMConfigSectionTest
    {
        [Test()]
        public void LoadWhiteMarsConfigSection()
        {
            var obj = System.Configuration.ConfigurationManager.GetSection("WhiteMars");
            Assert.NotNull(obj);
            Assert.IsInstanceOf<WhiteMars.Framework.Configuration.WMConfigSection>(obj);

            var configSection = obj as Configuration.WMConfigSection;

            Assert.AreEqual("MultiTenants/TenantMeta.xml", configSection.AppSettings["TenantMetaLocation"]);
            Assert.AreEqual("TenantConfigs/", configSection.AppSettings["TenantConfigLocation"]);

            Assert.AreEqual("WhiteMars.Framework.StaticXmlFileTenantMetaProvider, WhiteMars.Framework", configSection.ApplicationConfig.Configs["TenantMetaProvider"]);
            Assert.AreEqual("WhiteMars.Framework.InMemoryCacherProvider, WhiteMars.Framework", configSection.ApplicationConfig.Configs["CacherProvider"]);
            Assert.AreEqual("WhiteMars.Framework.ServiceProvider, WhiteMars.Framework", configSection.ApplicationConfig.Configs["ServiceProvider"]);
        }
    }
}

