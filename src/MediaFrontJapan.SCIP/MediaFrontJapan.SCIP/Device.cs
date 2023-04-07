using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Collections.Extensions;
using ValueTaskSupplement;

namespace MediaFrontJapan.SCIP;

public class Device : IAsyncDisposable
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct CompletionUnit
	{
	}

	private long lastRequestID;

	private static readonly UnboundedChannelOptions SessionChannelOptions = new UnboundedChannelOptions
	{
		SingleReader = true,
		SingleWriter = true,
		AllowSynchronousContinuations = false
	};

	private static readonly UnboundedChannelOptions UnsentRequestSessionsChannelOptions = new UnboundedChannelOptions
	{
		SingleReader = true,
		SingleWriter = false,
		AllowSynchronousContinuations = false
	};

	private volatile DeviceState state;

	private ConnectionProvider ConnectionProvider { get; }

	private CancellationTokenSource DisposeCancellationTokenSource { get; } = new CancellationTokenSource();

	private TaskCompletionSource<CompletionUnit> DisposeCompletionSource { get; } = new TaskCompletionSource<CompletionUnit>();

	private Channel<SessionHandle> UnsentRequestSessions { get; } = Channel.CreateUnbounded<SessionHandle>(UnsentRequestSessionsChannelOptions);

	private DictionarySlim<ulong, SessionHandle> RunningSessions { get; } = new DictionarySlim<ulong, SessionHandle>();

	public ValueTask<Response> GD(SingleScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 71, 68 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 10];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestAsync(span, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> GS(SingleScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 71, 83 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 10];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestAsync(span, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> GE(SingleScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 71, 69 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 10];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestAsync(span, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> HD(SingleScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 72, 68 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 10];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestAsync(span, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> HE(SingleScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 72, 69 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 10];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestAsync(span, willCancelPendingMultiScan: true);
	}

	public ChannelReader<Response> MD(MultiScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 77, 68 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 13];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestMultiScanAsync(span);
	}

	public ChannelReader<Response> MS(MultiScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 77, 83 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 13];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestMultiScanAsync(span);
	}

	public ChannelReader<Response> ME(MultiScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 77, 69 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 13];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestMultiScanAsync(span);
	}

	public ChannelReader<Response> ND(MultiScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 78, 68 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 13];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestMultiScanAsync(span);
	}

	public ChannelReader<Response> NE(MultiScanParam scanParam)
	{
		ReadOnlySpan<byte> readOnlySpan = stackalloc byte[2] { 78, 69 };
		Span<byte> span = stackalloc byte[readOnlySpan.Length + 13];
		readOnlySpan.CopyTo(span);
		scanParam.Write(span.Slice(readOnlySpan.Length));
		return RequestMultiScanAsync(span);
	}

	public unsafe ValueTask<Response> ST()
	{
		ReadOnlySpan<byte> request = "%ST"u8;
		return RequestAsync(request, willCancelPendingMultiScan: false);
	}

	public ValueTask<Response> BM()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 66, 77 };
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> QT()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 81, 84 };
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	public unsafe ValueTask<Response> SL()
	{
		ReadOnlySpan<byte> request = "%SL"u8;
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> RS()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 82, 83 };
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> RT()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 82, 84 };
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> RB()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 82, 66 };
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	public ValueTask<Response> VV()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 86, 86 };
		return RequestAsync(request, willCancelPendingMultiScan: false);
	}

	public ValueTask<Response> PP()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 80, 80 };
		return RequestAsync(request, willCancelPendingMultiScan: false);
	}

	public ValueTask<Response> II()
	{
		ReadOnlySpan<byte> request = stackalloc byte[2] { 73, 73 };
		return RequestAsync(request, willCancelPendingMultiScan: false);
	}

	public Device(ConnectionProvider connectionProvider)
	{
		ConnectionProvider = connectionProvider;
	}

	private (CancellationTokenSource? linkedCts, CancellationToken cancellationToken) CombineDisposeCancellation(CancellationToken cancellationToken)
	{
		CancellationTokenSource cancellationTokenSource;
		if (cancellationToken.CanBeCanceled)
		{
			cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, DisposeCancellationTokenSource.Token);
			cancellationToken = cancellationTokenSource.Token;
		}
		else
		{
			cancellationTokenSource = null;
			cancellationToken = DisposeCancellationTokenSource.Token;
		}
		return (linkedCts: cancellationTokenSource, cancellationToken: cancellationToken);
	}

	private static async ValueTask EnsurePipesCompleted(PipeReader pipeReader, PipeWriter pipeWriter)
	{
		ValueTask valueTask = default(ValueTask);
		ValueTask task2 = default(ValueTask);
		try
		{
			valueTask = pipeReader.CompleteAsync();
		}
		catch (Exception)
		{
		}
		try
		{
			task2 = pipeWriter.CompleteAsync();
		}
		catch (Exception)
		{
		}
		try
		{
			await valueTask;
		}
		catch (Exception)
		{
		}
		try
		{
			await task2;
		}
		catch (Exception)
		{
		}
	}

	public async Task<Task> ConnectAndRunAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		switch (CompareExchangeState(DeviceState.Connecting, DeviceState.NotConnected))
		{
		case DeviceState.Connecting:
		case DeviceState.Connected:
		case DeviceState.Disposing:
			throw new InvalidOperationException();
		case DeviceState.Disposed:
			throw new ObjectDisposedException(ToString());
		default:
			throw new Exception();
		case DeviceState.NotConnected:
		{
			CancellationTokenSource linkedCts = null;
			try
			{
				try
				{
					if (cancellationToken.CanBeCanceled)
					{
						linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, DisposeCancellationTokenSource.Token);
						cancellationToken = linkedCts.Token;
					}
					else
					{
						cancellationToken = DisposeCancellationTokenSource.Token;
					}
				}
				catch (Exception)
				{
					switch (CompareExchangeState(DeviceState.NotConnected, DeviceState.Connecting))
					{
					case DeviceState.Connecting:
						throw;
					case DeviceState.Disposing:
						PublishDisposed();
						throw;
					default:
						throw new Exception();
					}
				}
				var (pipeReader, pipeWriter) = await ConnectionProvider.ConnectAsync(cancellationToken);
				switch (CompareExchangeState(DeviceState.Connected, DeviceState.Connecting))
				{
				case DeviceState.Disposing:
					await EnsurePipesCompleted(pipeReader, pipeWriter);
					throw new ObjectDisposedException(ToString());
				default:
					throw new Exception();
				case DeviceState.Connecting:
					return RunAsync(pipeReader, pipeWriter, cancellationToken, linkedCts);
				}
			}
			catch (Exception)
			{
				linkedCts?.Dispose();
				throw;
			}
		}
		}
	}

	private async Task RunAsync(PipeReader input, PipeWriter output, CancellationToken cancellationToken, CancellationTokenSource? linkedCts)
	{
		try
		{
			Exception exception = null;
			try
			{
				await Task.Yield();
				SessionHandle activeMultiScan = default(SessionHandle);
				ValueTask<Response?> readResponse = Response.ReadAsync(input, cancellationToken);
				ValueTask<bool> waitToReadRequest = UnsentRequestSessions.Reader.WaitToReadAsync(cancellationToken);
				while (true)
				{
					Response? response;
					if (readResponse.IsCompleted)
					{
						response = readResponse.Result;
						goto IL_0448;
					}
					if (waitToReadRequest.IsCompleted)
					{
						_ = waitToReadRequest.Result;
					}
					else
					{
						waitToReadRequest = waitToReadRequest.Preserve();
						readResponse = readResponse.Preserve();
						int num;
						(num, _, response) = await ValueTaskEx.WhenAny(waitToReadRequest, readResponse).ConfigureAwait(continueOnCapturedContext: false);
						if (num != 0 && num == 1)
						{
							goto IL_0448;
						}
					}
					if (UnsentRequestSessions.Reader.TryRead(out var request))
					{
						try
						{
							if ((await output.WriteAsync(request.Request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).IsCanceled)
							{
								await UnsentRequestSessions.Writer.WriteAsync(request).ConfigureAwait(continueOnCapturedContext: false);
								break;
							}
						}
						catch (Exception)
						{
							await UnsentRequestSessions.Writer.WriteAsync(request).ConfigureAwait(continueOnCapturedContext: false);
							throw;
						}
						RunningSessions.GetOrAddValueRef(request.ID) = request;
					}
					waitToReadRequest = UnsentRequestSessions.Reader.WaitToReadAsync(cancellationToken);
					request = default(SessionHandle);
					continue;
					IL_0448:
					if (response.HasValue)
					{
						Response valueOrDefault = response.GetValueOrDefault();
						ulong? iD = valueOrDefault.ID;
						if (iD.HasValue)
						{
							ulong valueOrDefault2 = iD.GetValueOrDefault();
							if (RunningSessions.TryGetValue(valueOrDefault2, out var value))
							{
								ResponseHandle handle = value.Handle;
								if (activeMultiScan.Handle != handle && value.WillCancelPendingMultiScan && activeMultiScan.Handle != null)
								{
									activeMultiScan.Handle.SetExceptionOrComplete();
									RunningSessions.Remove(activeMultiScan.ID);
									activeMultiScan = default(SessionHandle);
								}
								if (handle.SetResult(valueOrDefault))
								{
									activeMultiScan = value;
								}
								else
								{
									RunningSessions.Remove(valueOrDefault2);
								}
							}
						}
					}
					readResponse = Response.ReadAsync(input);
				}
			}
			catch (Exception ex2)
			{
				exception = ex2;
				throw;
			}
			finally
			{
				Exception exceptionOrComplete = state switch
				{
					DeviceState.Connected => exception ?? new OperationCanceledException(), 
					DeviceState.Disposing => new ObjectDisposedException(ToString()), 
					_ => throw new Exception(), 
				};
				foreach (KeyValuePair<ulong, SessionHandle> runningSession in RunningSessions)
				{
					runningSession.Value.Handle.SetExceptionOrComplete(exceptionOrComplete);
				}
				RunningSessions.Clear();
				switch (CompareExchangeState(DeviceState.NotConnected, DeviceState.Connected))
				{
				case DeviceState.Disposing:
					PublishDisposed();
					throw new ObjectDisposedException(ToString());
				default:
					throw new Exception();
				case DeviceState.Connected:
					break;
				}
			}
		}
		finally
		{
			if (cancellationToken.IsCancellationRequested)
			{
				await EnsurePipesCompleted(input, output);
			}
			linkedCts?.Dispose();
		}
	}

	public ValueTask DisposeAsync()
	{
		while (true)
		{
			switch (CompareExchangeState(DeviceState.Disposed, DeviceState.NotConnected))
			{
			case DeviceState.NotConnected:
				PublishDisposed();
				return default(ValueTask);
			case DeviceState.Disposing:
				return new ValueTask(DisposeCompletionSource.Task);
			case DeviceState.Disposed:
				return default(ValueTask);
			case DeviceState.Connecting:
				goto IL_0053;
			case DeviceState.Connected:
				goto IL_00b2;
				IL_0053:
				switch (CompareExchangeState(DeviceState.Disposing, DeviceState.Connecting))
				{
				case DeviceState.NotConnected:
					break;
				case DeviceState.Connecting:
					DisposeCancellationTokenSource.Cancel();
					return new ValueTask(DisposeCompletionSource.Task);
				case DeviceState.Disposing:
					return new ValueTask(DisposeCompletionSource.Task);
				case DeviceState.Disposed:
					return default(ValueTask);
				case DeviceState.Connected:
					goto IL_00b2;
				default:
					goto end_IL_000a;
				}
				continue;
				IL_00b2:
				switch (CompareExchangeState(DeviceState.Disposing, DeviceState.Connected))
				{
				case DeviceState.NotConnected:
					break;
				case DeviceState.Connecting:
					goto IL_0053;
				case DeviceState.Connected:
					DisposeCancellationTokenSource.Cancel();
					goto case DeviceState.Disposing;
				case DeviceState.Disposing:
					return new ValueTask(DisposeCompletionSource.Task);
				case DeviceState.Disposed:
					return default(ValueTask);
				default:
					goto end_IL_000a;
				}
				continue;
				end_IL_000a:
				break;
			}
			break;
		}
		throw new Exception();
	}

	private void PublishDisposed()
	{
		DisposeCancellationTokenSource.Dispose();
		DisposeCompletionSource.SetResult(default(CompletionUnit));
		state = DeviceState.Disposed;
	}

	private DeviceState CompareExchangeState(DeviceState value, DeviceState comparand)
	{
		return (DeviceState)Interlocked.CompareExchange(ref Unsafe.As<DeviceState, int>(ref state), (int)value, (int)comparand);
	}

	private DeviceState ExchangeState(DeviceState value)
	{
		return (DeviceState)Interlocked.Exchange(ref Unsafe.As<DeviceState, int>(ref state), (int)value);
	}

	public ValueTask<Response> RequestAsync(ReadOnlySpan<byte> request, bool willCancelPendingMultiScan)
	{
		return RequestAsync<Response, PassThroughResponseParser>(request, willCancelPendingMultiScan);
	}

	public ValueTask<TResponse> RequestAsync<TResponse, TResponseParser>(ReadOnlySpan<byte> request, bool willCancelPendingMultiScan) where TResponseParser : unmanaged, IResponseParser<TResponse>
	{
		ulong num = (ulong)Interlocked.Increment(ref lastRequestID);
		ArraySegment<byte> request2 = BuildRequestBinary(request, num);
		ResponseHandle<TResponse, TResponseParser> responseHandle = ResponseHandle<TResponse, TResponseParser>.Pool.Get();
		SessionHandle item = new SessionHandle
		{
			Handle = responseHandle,
			ID = num,
			Request = request2,
			WillCancelPendingMultiScan = willCancelPendingMultiScan
		};
		UnsentRequestSessions.Writer.WriteAsync(item);
		return new ValueTask<TResponse>(responseHandle, responseHandle.Version);
	}

	public unsafe ValueTask<Response> RequestAsync(string request, bool willCancelPendingMultiScan)
	{
		byte* ptr = stackalloc byte[(int)(uint)request.Length];
		fixed (char* chars = request)
		{
			if (Encoding.ASCII.GetBytes(chars, request.Length, ptr, request.Length) != request.Length)
			{
				throw new FormatException("Could not encode request to ascii.");
			}
		}
		Span<byte> span = new Span<byte>(ptr, request.Length);
		return RequestAsync(span, willCancelPendingMultiScan);
	}

	public unsafe ChannelReader<Response> RequestMultiScanAsync(string request)
	{
		byte* ptr = stackalloc byte[(int)(uint)request.Length];
		fixed (char* chars = request)
		{
			if (Encoding.ASCII.GetBytes(chars, request.Length, ptr, request.Length) != request.Length)
			{
				throw new FormatException("Could not encode request to ascii.");
			}
		}
		Span<byte> span = new Span<byte>(ptr, request.Length);
		return RequestMultiScanAsync(span);
	}

	public ChannelReader<Response> RequestMultiScanAsync(ReadOnlySpan<byte> request)
	{
		return RequestMultiScanAsync<Response, PassThroughResponseParser>(request);
	}

	public ChannelReader<TResponse> RequestMultiScanAsync<TResponse, TResponseParser>(ReadOnlySpan<byte> request) where TResponseParser : unmanaged, IResponseParser<TResponse>
	{
		ulong num = (ulong)Interlocked.Increment(ref lastRequestID);
		ArraySegment<byte> request2 = BuildRequestBinary(request, num);
		(MultiScanResponseHandle<TResponse, TResponseParser>, ChannelReader<TResponse>) responseHandle = MultiScanResponseHandle<TResponse, TResponseParser>.GetResponseHandle();
		MultiScanResponseHandle<TResponse, TResponseParser> item = responseHandle.Item1;
		ChannelReader<TResponse> item2 = responseHandle.Item2;
		SessionHandle item3 = new SessionHandle
		{
			Handle = item,
			ID = num,
			Request = request2,
			WillCancelPendingMultiScan = true
		};
		UnsentRequestSessions.Writer.WriteAsync(item3);
		return item2;
	}

	public ValueTask<Response> TM(TMControlCode controlCode)
	{
		ReadOnlySpan<byte> request = stackalloc byte[3]
		{
			84,
			77,
			(byte)controlCode
		};
		return RequestAsync(request, willCancelPendingMultiScan: true);
	}

	private static ArraySegment<byte> BuildRequestBinary(ReadOnlySpan<byte> request, ulong id)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(request.Length + 18);
		Span<byte> span = array.AsSpan();
		request.CopyTo(span);
		span = span.Slice(request.Length);
		MemoryMarshal.GetReference(span) = 59;
		span = span.Slice(1);
		Utf8Formatter.TryFormat(id, span, out var bytesWritten, new StandardFormat('X', 16));
		span = span.Slice(bytesWritten);
		MemoryMarshal.GetReference(span) = 10;
		return new ArraySegment<byte>(array, 0, request.Length + bytesWritten + 2);
	}
}
