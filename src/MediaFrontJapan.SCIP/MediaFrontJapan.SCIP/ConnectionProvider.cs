using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace MediaFrontJapan.SCIP;

public abstract class ConnectionProvider
{
	protected internal abstract ValueTask<(PipeReader Input, PipeWriter Output)> ConnectAsync(CancellationToken cancellationToken = default(CancellationToken));
}
