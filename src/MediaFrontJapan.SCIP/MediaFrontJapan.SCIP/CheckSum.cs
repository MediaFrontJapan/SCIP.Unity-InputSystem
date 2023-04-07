using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MediaFrontJapan.SCIP;

public static class CheckSum
{
	public static bool IsValidData(ReadOnlySpan<byte> data, byte checkCode)
	{
		ref byte reference = ref MemoryMarshal.GetReference(data);
		int length = data.Length;
		return IsValidData(ref reference, length, checkCode);
	}

	public unsafe static bool IsValidData(byte* ptr, int length, byte checkCode)
	{
		return IsValidData(ref *ptr, length, checkCode);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValidData(ref byte dataRef, int length, byte checkCode)
	{
		int num = 0;
		while (true)
		{
			switch (length)
			{
			case 0:
				return (num & 0x3F) + 48 == checkCode;
			case 1:
				num += dataRef;
				length--;
				dataRef = ref Unsafe.AddByteOffset(ref dataRef, (IntPtr)1);
				break;
			case 2:
			case 3:
			{
				ushort num4 = Unsafe.ReadUnaligned<ushort>(ref dataRef);
				num += (num4 & 0xFF) + ((num4 & 0xFF00) >> 8);
				length -= 2;
				dataRef = ref Unsafe.AddByteOffset(ref dataRef, (IntPtr)2);
				break;
			}
			case 4:
			case 5:
			case 6:
			case 7:
			{
				uint num3 = Unsafe.ReadUnaligned<uint>(ref dataRef);
				num3 = (num3 & 0xFF00FF) + ((num3 & 0xFF00FF00u) >> 8);
				num3 = (num3 & 0xFFFF) + ((num3 & 0xFFFF0000u) >> 16);
				num += (int)num3;
				length -= 4;
				dataRef = ref Unsafe.AddByteOffset(ref dataRef, (IntPtr)4);
				break;
			}
			default:
			{
				ulong num2 = Unsafe.ReadUnaligned<ulong>(ref dataRef);
				num2 = (num2 & 0xFF00FF00FF00FFL) + ((num2 & 0xFF00FF00FF00FF00uL) >> 8);
				num2 = (num2 & 0xFFFF0000FFFFL) + ((num2 & 0xFFFF0000FFFF0000uL) >> 16);
				num2 = (num2 & 0xFFFFFFFFu) + ((num2 & 0xFFFFFFFF00000000uL) >> 32);
				num += (int)num2;
				length -= 8;
				dataRef = ref Unsafe.AddByteOffset(ref dataRef, (IntPtr)8);
				break;
			}
			}
		}
	}
}
