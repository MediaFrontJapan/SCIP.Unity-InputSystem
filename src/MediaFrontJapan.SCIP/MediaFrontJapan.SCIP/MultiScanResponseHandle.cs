using System;
using System.Threading.Channels;
using Microsoft.Extensions.ObjectPool;

namespace MediaFrontJapan.SCIP;

internal abstract class MultiScanResponseHandle : ResponseHandle
{
	protected static UnboundedChannelOptions ChannelOptions { get; } = new UnboundedChannelOptions
	{
		SingleWriter = false,
		SingleReader = true,
		AllowSynchronousContinuations = false
	};
}
internal sealed class MultiScanResponseHandle<TResponse, TParser> : MultiScanResponseHandle where TParser : unmanaged, IResponseParser<TResponse>
{
	private sealed class PooledPolicy : PooledObjectPolicy<MultiScanResponseHandle<TResponse, TParser>>
	{
		public override MultiScanResponseHandle<TResponse, TParser> Create()
		{
			return new MultiScanResponseHandle<TResponse, TParser>();
		}

		public override bool Return(MultiScanResponseHandle<TResponse, TParser> obj)
		{
			obj.Writer = null;
			return true;
		}
	}

	private static DefaultObjectPool<MultiScanResponseHandle<TResponse, TParser>> Pool { get; } = new DefaultObjectPool<MultiScanResponseHandle<TResponse, TParser>>(new PooledPolicy());

	private ChannelWriter<TResponse> Writer { get; set; }

	public static (MultiScanResponseHandle<TResponse, TParser>, ChannelReader<TResponse>) GetResponseHandle()
	{
		MultiScanResponseHandle<TResponse, TParser> multiScanResponseHandle = Pool.Get();
		Channel<TResponse> channel = Channel.CreateUnbounded<TResponse>(MultiScanResponseHandle.ChannelOptions);
		multiScanResponseHandle.Writer = channel.Writer;
		return (multiScanResponseHandle, channel.Reader);
	}

	public override bool SetResult(Response response)
	{
		TResponse item;
		try
		{
			item = default(TParser).Parse(response);
		}
		catch (Exception exceptionOrComplete)
		{
			SetExceptionOrComplete(exceptionOrComplete);
			return false;
		}
		Writer.WriteAsync(item);
		return true;
	}

	public sealed override void SetExceptionOrComplete(Exception? error)
	{
		try
		{
			Writer.Complete(error);
		}
		finally
		{
			Pool.Return(this);
		}
	}
}
