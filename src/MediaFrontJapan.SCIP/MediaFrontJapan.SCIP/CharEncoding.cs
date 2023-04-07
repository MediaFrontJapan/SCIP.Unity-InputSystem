using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MediaFrontJapan.SCIP;

public static class CharEncoding
{
	public static uint Decode4(uint char4)
	{
		uint num = char4;
		num -= 808464432;
		return ((num & 0xFF) << 18) | ((num & 0xFF00) << 4) | ((num & 0xFF0000) >> 10) | ((num & 0xFF000000u) >> 24);
	}

	public static uint Decode3(uint char3)
	{
		uint num = char3;
		num -= 3158064;
		return ((num & 0xFF) << 12) | ((num & 0xFF00) >> 2) | ((num & 0xFF0000) >> 16);
	}

	public static uint Decode2(ushort char2)
	{
		uint num = char2;
		num -= 12336;
		return ((num & 0xFF) << 6) | ((num & 0xFF00) >> 8);
	}

	public static uint Decode(ReadOnlySpan<byte> chars)
	{
		switch (chars.Length)
		{
		case 2:
			return Decode2(Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(chars)));
		case 3:
		{
			ref byte reference = ref MemoryMarshal.GetReference(chars);
			return Decode3((uint)(Unsafe.ReadUnaligned<ushort>(ref reference) | (Unsafe.AddByteOffset(ref reference, (IntPtr)2) << 16)));
		}
		case 4:
			return Decode4(Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(chars)));
		default:
			return Fallback(chars);
		}
		static uint Fallback(ReadOnlySpan<byte> readOnlySpan)
		{
			uint num = 0u;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				num <<= 6;
				num |= (uint)(readOnlySpan[i] - 48);
			}
			return num;
		}
	}
}
