using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ValueTaskSupplement;

namespace MediaFrontJapan.SCIP;

public sealed class EthernetConnectionProvider : ConnectionProvider
{
	public string HostName { get; }

	public int PortNumber { get; }

	public int TimeoutMilliSeconds { get; }

	public EthernetConnectionProvider(string hostName, int portNumber, int timeoutMilliSeconds = -1)
	{
		HostName = hostName;
		PortNumber = portNumber;
		TimeoutMilliSeconds = timeoutMilliSeconds;
	}

	protected internal override async ValueTask<(PipeReader Input, PipeWriter Output)> ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		TcpClient client = new TcpClient();
		try
		{
			Task right = Task.Delay(TimeoutMilliSeconds, cancellationToken);
			if (await ValueTaskEx.WhenAny(client.ConnectAsync(HostName, PortNumber).AsValueTask(), right) != 0)
			{
				throw new TimeoutException();
			}
			NetworkStream stream = client.GetStream();
			PipeReader item = PipeReader.Create(stream);
			PipeWriter item2 = PipeWriter.Create(stream);
			return (item, item2);
		}
		catch (Exception)
		{
			client.Dispose();
			throw;
		}
	}
}
