using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace MediaFrontJapan.SCIP;

public struct SingleScanParam
{
	public const int Chars = 10;

	private int start;

	private int end;

	private int grouping;

	public int Start
	{
		get
		{
			return start;
		}
		set
		{
			if ((uint)value >= 10000u)
			{
				ThrowOutOfRange();
			}
			start = value;
		}
	}

	public int End
	{
		get
		{
			return end;
		}
		set
		{
			if ((uint)value >= 10000u)
			{
				ThrowOutOfRange();
			}
			end = value;
		}
	}

	public int Grouping
	{
		get
		{
			return grouping;
		}
		set
		{
			if ((uint)value >= 100u)
			{
				ThrowOutOfRange();
			}
			grouping = value;
		}
	}

	private static void ThrowOutOfRange()
	{
		throw new ArgumentOutOfRangeException();
	}

	public bool Write(Span<byte> destination)
	{
		if (!Utf8Formatter.TryFormat(Start, destination, out var bytesWritten, new StandardFormat('d', 4)))
		{
			return false;
		}
		destination = destination.Slice(4);
		if (!Utf8Formatter.TryFormat(End, destination, out bytesWritten, new StandardFormat('d', 4)))
		{
			return false;
		}
		destination = destination.Slice(4);
		if (!Utf8Formatter.TryFormat(Grouping, destination, out bytesWritten, new StandardFormat('d', 2)))
		{
			return false;
		}
		return true;
	}

	public unsafe override string ToString()
	{
		byte* ptr = stackalloc byte[10];
		Write(new Span<byte>(ptr, 10));
		return Encoding.ASCII.GetString(ptr, 10);
	}
}
