using System;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.ObjectPool;

namespace MediaFrontJapan.SCIP;

internal abstract class ResponseHandle
{
	public abstract bool SetResult(Response response);

	public abstract void SetExceptionOrComplete(Exception? error = null);
}
internal abstract class ResponseHandle<T> : ResponseHandle, IValueTaskSource<T>
{
	protected ManualResetValueTaskSourceCore<T> core;

	public short Version => core.Version;

	public abstract T GetResult(short token);

	public ValueTaskSourceStatus GetStatus(short token)
	{
		return core.GetStatus(token);
	}

	public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
	{
		core.OnCompleted(continuation, state, token, flags);
	}

	public sealed override void SetExceptionOrComplete(Exception? error)
	{
		core.SetException(error);
	}

	protected void Reset()
	{
		core.Reset();
	}
}
internal sealed class ResponseHandle<TResponse, TParser> : ResponseHandle<TResponse> where TParser : unmanaged, IResponseParser<TResponse>
{
	private sealed class PoolPolicy : PooledObjectPolicy<ResponseHandle<TResponse, TParser>>
	{
		public override ResponseHandle<TResponse, TParser> Create()
		{
			return new ResponseHandle<TResponse, TParser>();
		}

		public override bool Return(ResponseHandle<TResponse, TParser> obj)
		{
			obj.Reset();
			return true;
		}
	}

	internal static DefaultObjectPool<ResponseHandle<TResponse, TParser>> Pool { get; } = new DefaultObjectPool<ResponseHandle<TResponse, TParser>>(new PoolPolicy());

	public override TResponse GetResult(short token)
	{
		try
		{
			return core.GetResult(token);
		}
		finally
		{
			Pool.Return(this);
		}
	}

	public override bool SetResult(Response response)
	{
		TResponse result;
		try
		{
			result = default(TParser).Parse(response);
		}
		catch (Exception exception)
		{
			core.SetException(exception);
			return false;
		}
		core.SetResult(result);
		return false;
	}
}
