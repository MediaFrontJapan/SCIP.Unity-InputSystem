using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MediaFrontJapan.SCIP;

public readonly struct ParsedResponseAwaitable<TResult, TParser> where TParser : unmanaged, IResponseParser<TResult>
{
	public readonly struct ParsedResponseAwaiter : ICriticalNotifyCompletion, INotifyCompletion
	{
		private readonly ValueTaskAwaiter<Response> ValueTaskAwaiter;

		public bool IsCompleted => ValueTaskAwaiter.IsCompleted;

		public ParsedResponseAwaiter(in ValueTaskAwaiter<Response> valueTaskAwaiter)
		{
			ValueTaskAwaiter = valueTaskAwaiter;
		}

		public TResult GetResult()
		{
			return default(TParser).Parse(ValueTaskAwaiter.GetResult());
		}

		public void OnCompleted(Action continuation)
		{
			ValueTaskAwaiter.OnCompleted(continuation);
		}

		public void UnsafeOnCompleted(Action continuation)
		{
			ValueTaskAwaiter.UnsafeOnCompleted(continuation);
		}
	}

	private readonly ValueTask<Response> task;

	public ParsedResponseAwaitable(in ValueTask<Response> task)
	{
		this.task = task;
	}

	public ParsedResponseAwaiter GetAwaiter()
	{
		return new ParsedResponseAwaiter(task.GetAwaiter());
	}
}
