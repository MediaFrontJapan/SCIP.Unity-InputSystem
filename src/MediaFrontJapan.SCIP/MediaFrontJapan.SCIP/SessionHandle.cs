using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaFrontJapan.SCIP;

[StructLayout(LayoutKind.Auto)]
internal struct SessionHandle
{
	public ArraySegment<byte> Request;

	public bool WillCancelPendingMultiScan;

	public ulong ID;

	public ResponseHandle Handle;

	public override string ToString()
	{
		return Encoding.ASCII.GetString(Request.Array, Request.Offset, Request.Count);
	}
}
