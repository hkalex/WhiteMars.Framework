using NUnit.Framework;
using System;
using System.Linq;

namespace WhiteMars.Framework.UnitTest
{
    [TestFixture]
    public class WhiteMarsApplicationTest
    {

        [Test]
        public void TenantMetaProvider()
        {
            var provider = WhiteMarsApplication.TenantMetaProvider;
            Assert.IsNotNull(provider);
            var tenantMetas = provider.GetTenantMetaCollection();
            Assert.IsNotNull(tenantMetas);

            Assert.IsTrue(tenantMetas.Contains("solar.whitemars.com.au"));
            Assert.IsTrue(tenantMetas.Contains("lunar.whitemars.com.au"));

        }

        [Test]
        public void ServiceProvider()
        {

        }


    }
}

