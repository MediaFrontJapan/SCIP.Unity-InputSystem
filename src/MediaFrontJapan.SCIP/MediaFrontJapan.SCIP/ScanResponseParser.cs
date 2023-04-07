using System;
using System.Runtime.InteropServices;

namespace MediaFrontJapan.SCIP;

public static class ScanResponseParser
{
	public static (int Time, ArraySegment<byte> EncodedDataBlocks) Parse(Response scanResponse)
	{
		Span<byte> span = scanResponse.Data.AsSpan();
		int num = span.IndexOf<byte>(10);
		switch (num)
		{
		case -1:
			throw new FormatException("Invalid response.");
		case 0:
			throw new FormatException("Response's data block was empty. Not a scan response.");
		default:
		{
			Span<byte> span2 = span.Slice(0, num - 1);
			if (!CheckSum.IsValidData(checkCode: span[num - 1], data: span2))
			{
				throw new FormatException("CheckSum mismatch.");
			}
			uint item = CharEncoding.Decode4(MemoryMarshal.Read<uint>(span2));
			ArraySegment<byte> item2 = new ArraySegment<byte>(scanResponse.Data.Array, scanResponse.Data.Offset + num + 1, scanResponse.Data.Count - (num + 1));
			return (Time: (int)item, EncodedDataBlocks: item2);
		}
		}
	}

	public static int GetDecodedValueAt(ReadOnlySpan<byte> encodedDataBlocks, int index, int encodingChars)
	{
		int num = encodingChars * index;
		int num2 = num & 0x3F;
		int num3 = num >> 6;
		int start = num + (num3 << 1);
		int num4 = 64 - num2;
		bool num5 = num4 < encodingChars;
		if (num5)
		{
			byte[] bytes = new byte[encodingChars];
			encodedDataBlocks.Slice(start, num4).CopyTo(bytes);
			encodedDataBlocks.Slice((num3 + 1) * 66, encodingChars - num4).CopyTo(bytes.AsSpan(num4));
			return (int)CharEncoding.Decode(bytes);
		}
		return (int)CharEncoding.Decode(encodedDataBlocks.Slice(start, encodingChars));
	}
}
