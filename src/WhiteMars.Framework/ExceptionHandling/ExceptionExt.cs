using System;
using System.Text;
using System.Linq;
using System.Collections;

namespace WhiteMars.Framework
{
	/// <summary>
	/// WhiteMarsException extensions
	/// </summary>
	public static class ExceptionExt
	{
		const string PREFIX_ACC = " > ";
		const string EXCEPTION_STRAT = "++++++++++Exception Start++++++++++";
		const string EXCEPTION_END = "++++++++++Exception End++++++++++";

		/// <summary>
		/// To the detail string.
		/// </summary>
		/// <returns>The detail string.</returns>
		/// <param name="exception">Exception.</param>
		public static string ToDetailString (this Exception exception)
		{
			var result = new StringBuilder ();

			result.AppendLine (EXCEPTION_STRAT);

			var ex = exception;
			var level = 0;
			var prefix = string.Empty;

			while (ex != null) {
				if (level > 0) {
					prefix += PREFIX_ACC;
				}

				foreach (var p in ex.GetType().GetProperties().Where(c=>c.Name != "Data")) {
					var value = p.GetValue (ex);
					result.AppendLine (string.Format ("{0}{1} : {2}", prefix, p.Name, value));
				}

				if (ex.Data != null) {
					foreach (DictionaryEntry de in ex.Data) {
						result.AppendLine (string.Format ("{0}Data[{1}]={2}", prefix, de.Key, de.Value));
					}
				}


				level++;
				ex = ex.InnerException;
			}

			result.AppendLine (EXCEPTION_END);

			return result.ToString ();
		}
	}
}

