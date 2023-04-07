using System;
using System.Buffers.Text;
namespace MediaFrontJapan.SCIP;

public struct PPResponse
{
	public ArraySegment<byte> MODL;

	public int DMIN;

	public int DMAX;

	public int ARES;

	public int AMIN;

	public int AMAX;

	public int AFRT;

	public int SCAN;

	public ArraySegment<byte> Others;

	public unsafe static PPResponse? FromResponse(Response response)
	{
		PPResponse value = default(PPResponse);
		DataBlockEnumerator dataBlockEnumerator = new DataBlockEnumerator(response.Data);
		ReadOnlySpan<byte> tag = "MODL:"u8;
		ReadOnlySpan<byte> tag2 = "DMIN:"u8;
		ReadOnlySpan<byte> tag3 = "DMAX:"u8;
		ReadOnlySpan<byte> tag4 = "ARES:"u8;
		ReadOnlySpan<byte> tag5 = "AMIN:"u8;
		ReadOnlySpan<byte> tag6 = "AMAX:"u8;
		ReadOnlySpan<byte> tag7 = "AFRT:"u8;
		ReadOnlySpan<byte> tag8 = "SCAN:"u8;
		if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag, out var parameter))
		{
			value.MODL = parameter;
			if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag2, out parameter))
			{
				if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value2, out int bytesConsumed, 'd'))
				{
					value.DMIN = value2;
					if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag3, out parameter))
					{
						if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value3, out bytesConsumed, 'd'))
						{
							value.DMAX = value3;
							if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag4, out parameter))
							{
								if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value4, out bytesConsumed, 'd'))
								{
									value.ARES = value4;
									if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag5, out parameter))
									{
										if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value5, out bytesConsumed, 'd'))
										{
											value.AMIN = value5;
											if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag6, out parameter))
											{
												if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value6, out bytesConsumed, 'd'))
												{
													value.AMAX = value6;
													if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag7, out parameter))
													{
														if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value7, out bytesConsumed, 'd'))
														{
															value.AFRT = value7;
															if (dataBlockEnumerator.MoveNext() && VVPPIIParser.TryGetParameter(dataBlockEnumerator.Current, tag8, out parameter))
															{
																if (Utf8Parser.TryParse((ReadOnlySpan<byte>)parameter.AsSpan(), out int value8, out bytesConsumed, 'd'))
																{
																	value.SCAN = value8;
																	if (dataBlockEnumerator.MoveNext())
																	{
																		value.Others = dataBlockEnumerator.Remaining;
																	}
																	else
																	{
																		value.Others = new ArraySegment<byte>(response.Data.Array, response.Data.Offset + response.Data.Count, 0);
																	}
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

	public PPResponse(ArraySegment<byte> mODL, int dMIN, int dMAX, int aRES, int aMIN, int aMAX, int aFRT, int sCAN, ArraySegment<byte> others = default(ArraySegment<byte>))
	{
		MODL = mODL;
		DMIN = dMIN;
		DMAX = dMAX;
		ARES = aRES;
		AMIN = aMIN;
		AMAX = aMAX;
		AFRT = aFRT;
		SCAN = sCAN;
		Others = others;
	}
}
