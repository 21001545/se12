using System;
using System.Runtime.Serialization;

namespace Festa.Client
{
	[Serializable]
	internal class Excetion : Exception
	{
		public Excetion()
		{
		}

		public Excetion(string message) : base(message)
		{
		}

		public Excetion(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected Excetion(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}