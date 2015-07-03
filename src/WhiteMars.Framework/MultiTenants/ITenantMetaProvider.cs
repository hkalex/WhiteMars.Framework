using System;
using System.Collections.Generic;

namespace WhiteMars.Framework
{
	/// <summary>
	/// The common interface  for all TenantMetaProvider
	/// </summary>
	public interface ITenantMetaProvider
	{
		/// <summary>
		/// Gets all tenant meta.
		/// </summary>
		/// <returns>All tenant meta.</returns>
		TenantMetaCollection GetTenantMetaCollection();
	}
}

