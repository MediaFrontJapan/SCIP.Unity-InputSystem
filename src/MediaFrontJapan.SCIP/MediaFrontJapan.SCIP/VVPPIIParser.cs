using System;

namespace MediaFrontJapan.SCIP;

public static class VVPPIIParser
{
	public static bool TryGetParameter(ArraySegment<byte> segment, ReadOnlySpan<byte> tag, out ArraySegment<byte> parameter)
	{
		Span<byte> span = segment.AsSpan();
		Span<byte> span2 = span.Slice(0, span.Length - 3);
		if (CheckSum.IsValidData(span2, span[span.Length - 2]) && span2.StartsWith(tag))
		{
			parameter = new ArraySegment<byte>(segment.Array, segment.Offset + tag.Length, tag.Length);
			return true;
		}
		parameter = default(ArraySegment<byte>);
		return false;
	}
}
