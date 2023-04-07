using System;
using System.Collections;
using System.Collections.Generic;

namespace MediaFrontJapan.SCIP;

internal struct DataBlockEnumerator : IEnumerator<ArraySegment<byte>>, IEnumerator, IDisposable
{
	public ArraySegment<byte> Remaining;

	private int currentLength;

	public ArraySegment<byte> Current => new ArraySegment<byte>(Remaining.Array, Remaining.Offset, currentLength);

	object IEnumerator.Current => Current;

	public DataBlockEnumerator(ArraySegment<byte> responseData)
	{
		this = default(DataBlockEnumerator);
		Remaining = responseData;
	}

	public void Dispose()
	{
		Remaining = default(ArraySegment<byte>);
	}

	public bool MoveNext()
	{
		Remaining = new ArraySegment<byte>(Remaining.Array, Remaining.Offset + currentLength, Remaining.Count - currentLength);
		int num = Remaining.AsSpan().IndexOf<byte>(10);
		if (num > 0)
		{
			currentLength = num + 1;
			return true;
		}
		return false;
	}

	public void Reset()
	{
		throw new NotImplementedException();
	}
}
