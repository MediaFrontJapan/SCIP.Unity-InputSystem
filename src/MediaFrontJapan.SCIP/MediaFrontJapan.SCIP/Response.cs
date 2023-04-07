using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaFrontJapan.SCIP;

public struct Response : IDisposable
{
	public const byte Delimiter = 10;

	public ArraySegment<byte> Command;

	internal ulong? ID;

	public Status Status;

	public ArraySegment<byte> Data;

	public void Dispose()
	{
		if (Command.Array != null)
		{
			ArrayPool<byte>.Shared.Return(Command.Array);
			Command = (Data = default(ArraySegment<byte>));
		}
	}

	public override string ToString()
	{
		return string.Format("{0} : {1}\n{2} : {3}\n{4} : {5}\n{6} : {7}", "Command", Encoding.ASCII.GetString(Command.Array, Command.Offset, Command.Count), "ID", ID, "Status", Status, "Data", Encoding.ASCII.GetString(Data.Array, Data.Offset, Data.Count));
	}

	public static Response? TryRead(ReadOnlySequence<byte> packetSequence)
	{
		int num = (int)packetSequence.Length;
		byte[] array = ArrayPool<byte>.Shared.Rent(num);
		Span<byte> span = array.AsSpan(0, num);
		packetSequence.CopyTo(span);
		int num2 = span.IndexOf<byte>(10);
		if (num2 != -1)
		{
			ArraySegment<byte> commandEchoSegment = new ArraySegment<byte>(array, 0, num2);
			int num3 = num2 + 1;
			if (num3 <= span.Length)
			{
				Span<byte> span2 = span.Slice(num3);
				Span<byte> span3 = span2.Slice(0, 2);
				byte checkCode = span2[2];
				if (CheckSum.IsValidData(span3, checkCode))
				{
					Status status = MemoryMarshal.Read<Status>(span3);
					int num4 = num3 + 2 + 2;
					ArraySegment<byte> dataSegment = new ArraySegment<byte>(array, num4, num - num4);
					return Create(commandEchoSegment, status, dataSegment);
				}
			}
		}
		ArrayPool<byte>.Shared.Return(array);
		return null;
	}

	private static Response Create(ArraySegment<byte> commandEchoSegment, Status status, ArraySegment<byte> dataSegment)
	{
		Response result = new Response
		{
			Data = dataSegment,
			Status = status
		};
		Span<byte> span = commandEchoSegment.AsSpan();
		int num = span.IndexOf<byte>(59);
		if (num == -1)
		{
			result.Command = commandEchoSegment;
			result.ID = null;
		}
		else
		{
			result.Command = new ArraySegment<byte>(commandEchoSegment.Array, commandEchoSegment.Offset, num);
			if (Utf8Parser.TryParse((ReadOnlySpan<byte>)span.Slice(num + 1), out ulong value, out int _, 'X'))
			{
				result.ID = value;
			}
			else
			{
				result.ID = null;
			}
		}
		return result;
	}

	public static async ValueTask<Response?> ReadAsync(PipeReader input, CancellationToken cancellationToken = default(CancellationToken))
	{
		ReadOnlySequence<byte> buffer = (await input.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Buffer;
		SequencePosition pos = buffer.Start;
		byte[] packetDelimiter = new byte[2] { 10, 10 };
		while (true)
		{
			ReadOnlySequence<byte> sequence = buffer.Slice(pos);
			if (SequenceHelpers.PositionOf(sequence, packetDelimiter, out pos))
			{
				break;
			}
			input.AdvanceTo(buffer.Start, buffer.End);
			buffer = (await input.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Buffer;
		}
		SequencePosition end = buffer.Slice(pos, 2).End;
		Response? result = TryRead(buffer.Slice(buffer.Start, end));
		input.AdvanceTo(end);
		return result;
	}
}
