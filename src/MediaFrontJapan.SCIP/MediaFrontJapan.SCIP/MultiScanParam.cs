using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace MediaFrontJapan.SCIP;

public struct MultiScanParam
{
	public const int Chars = 13;

	private int start;

	private int end;

	private int grouping;

	private int skips;

	private int scans;

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

	public int Skips
	{
		get
		{
			return skips;
		}
		set
		{
			if ((uint)value >= 10u)
			{
				ThrowOutOfRange();
			}
			skips = value;
		}
	}

	public int Scans
	{
		get
		{
			return scans;
		}
		set
		{
			if ((uint)value >= 100u)
			{
				ThrowOutOfRange();
			}
			scans = value;
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
		destination = destination.Slice(2);
		if (!Utf8Formatter.TryFormat(Skips, destination, out bytesWritten, new StandardFormat('d', 1)))
		{
			return false;
		}
		destination = destination.Slice(1);
		if (!Utf8Formatter.TryFormat(Scans, destination, out bytesWritten, new StandardFormat('d', 2)))
		{
			return false;
		}
		return true;
	}

	public unsafe override string ToString()
	{
		byte* ptr = stackalloc byte[13];
		Write(new Span<byte>(ptr, 13));
		return Encoding.ASCII.GetString(ptr, 13);
	}
}
