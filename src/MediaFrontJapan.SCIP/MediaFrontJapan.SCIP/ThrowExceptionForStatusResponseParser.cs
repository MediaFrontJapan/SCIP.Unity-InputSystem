using System.Runtime.InteropServices;

namespace MediaFrontJapan.SCIP;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ThrowExceptionForStatusResponseParser : IResponseParser<Response>
{
	public Response Parse(Response response)
	{
		response.Status.ThrowExceptionForStatus();
		return response;
	}
}
