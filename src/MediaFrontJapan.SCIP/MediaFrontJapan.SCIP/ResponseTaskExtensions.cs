using System.Threading.Tasks;

namespace MediaFrontJapan.SCIP;

public static class ResponseTaskExtensions
{
	public static ParsedResponseAwaitable<Response, ThrowExceptionForStatusResponseParser> ThrowIfExceptionalStatus(this in ValueTask<Response> task)
	{
		return new ParsedResponseAwaitable<Response, ThrowExceptionForStatusResponseParser>(in task);
	}
}
