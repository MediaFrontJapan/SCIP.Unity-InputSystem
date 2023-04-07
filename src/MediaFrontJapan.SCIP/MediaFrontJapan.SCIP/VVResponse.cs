using System;
namespace MediaFrontJapan.SCIP;

public struct VVResponse
{
	public ArraySegment<byte> VEND;

	public ArraySegment<byte> PROD;

	public ArraySegment<byte> FIRM;

	public ArraySegment<byte> PROT;

	public ArraySegment<byte> SERI;

	public unsafe static VVResponse? FromResponse(Response response)
	{
		VVResponse value = default(VVResponse);
		DataBlockEnumerator dataBlockEnumerator = new DataBlockEnumerator(response.Data);
		ReadOnlySpan<byte> tag = "VEND:"u8;
		ReadOnlySpan<byte> tag2 = "PROD:"u8;
		ReadOnlySpan<byte> tag3 = "FIRM:"u8;
		ReadOnlySpan<byte> tag4 = "PROT:"u8;
		ReadOnlySpan<byte> tag5 = "SERI:"u8;
		if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag, out var parameter))
		{
			value.VEND = parameter;
			if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag2, out parameter))
			{
				value.PROD = parameter;
				if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag3, out parameter))
				{
					value.FIRM = parameter;
					if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag4, out parameter))
					{
						value.PROT = parameter;
						if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag5, out parameter))
						{
							value.SERI = parameter;
							return value;
						}
						return null;
					}
					return null;
				}
				return null;
			}
			return null;
		}
		return null;
	}

	public VVResponse(ArraySegment<byte> vEND, ArraySegment<byte> pROD, ArraySegment<byte> fIRM, ArraySegment<byte> pROT, ArraySegment<byte> sERI)
	{
		VEND = vEND;
		PROD = pROD;
		FIRM = fIRM;
		PROT = pROT;
		SERI = sERI;
	}
}
