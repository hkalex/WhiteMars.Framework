using System;
using System.Text;

namespace WhiteMars.Framework
{
	
	[Serializable]
	public class WhiteMarsException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:WhiteMarsException"/> class
		/// </summary>
		public WhiteMarsException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WhiteMarsException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public WhiteMarsException (string message) : base (message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WhiteMarsException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public WhiteMarsException (string message, Exception inner) : base (message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:WhiteMarsException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected WhiteMarsException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
		{
		}

		private object stringValueLocker = new object ();
		private string stringValue;

		public override string ToString ()
		{
			if (stringValue == null) {
				lock (stringValueLocker) {
					if (stringValue == null) {
						this.stringValue = this.ToDetailString ();
					}
				}
			}
			return this.stringValue;
		}
	}
}

