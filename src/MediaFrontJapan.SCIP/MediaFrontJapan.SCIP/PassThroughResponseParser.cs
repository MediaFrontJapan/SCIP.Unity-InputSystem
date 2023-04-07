using System.Runtime.InteropServices;

namespace MediaFrontJapan.SCIP;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct PassThroughResponseParser : IResponseParser<Response>
{
	public Response Parse(Response response)
	{
		return response;
	}
}
