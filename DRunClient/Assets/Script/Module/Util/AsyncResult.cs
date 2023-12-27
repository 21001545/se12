namespace Festa.Client.Module
{
	public abstract class AsyncResult<T>
	{
		public abstract bool failed();
		public abstract bool succeeded();
		public abstract T result();
		public abstract System.Exception cause();
	}
}
