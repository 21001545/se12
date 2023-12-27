using System;

namespace Festa.Client.Module
{
	public class Void { }

	public class FutureT<T> : AsyncResult<T> 
	{
		private bool _success;
		private T _result;
		private Exception _cause;

		public override bool failed()
		{
			return _success == false;
		}

		public override bool succeeded()
		{
			return _success;
		}

		public override T result()
		{
			return _result;
		}

		public override System.Exception cause()
		{
			return _cause;
		}

		public FutureT(bool success, T result, Exception cause)
		{
			_success = success;
			_result = result;
			_cause = cause;
		}

	}

	public class Future
	{
		public static FutureT<typeT> succeededFuture<typeT>(typeT result)
		{
			return new FutureT<typeT>(true, result, null);
		}


		public static FutureT<typeT> failedFuture<typeT>(Exception cause)
		{
			return new FutureT<typeT>(false, default(typeT), cause);
		}

		public static FutureT<typeT> failedFuture<typeT>(String message)
		{
			return new FutureT<typeT>(false, default(typeT), new Exception(message));
		}

		public static FutureT<Void> succeededFuture()
		{
			return new FutureT<Void>(true, null, null);
		}

		public static FutureT<Void> failedFuture(Exception cause)
		{
			return new FutureT<Void>(false, null, cause);
		}

		public static FutureT<Void> failedFuture(String message)
		{
			return new FutureT<Void>(false, null, new Exception(message));
		}
	}
}
