using System;
using System.Buffers;

namespace MediaFrontJapan.SCIP;

internal static class SequenceHelpers
{
	public static bool PositionOf<T>(ReadOnlySequence<T> sequence, ReadOnlySpan<T> match, out SequencePosition position) where T : IEquatable<T>
	{
		if (match.IsEmpty)
		{
			position = sequence.Start;
			return true;
		}
		T value = match[0];
		while (true)
		{
			SequencePosition? sequencePosition = sequence.PositionOf(value);
			if (!sequencePosition.HasValue)
			{
				break;
			}
			SequencePosition valueOrDefault = sequencePosition.GetValueOrDefault();
			if (sequence.Slice(valueOrDefault).Length < match.Length)
			{
				position = valueOrDefault;
				return false;
			}
			ReadOnlySequence<T> source = sequence.Slice(valueOrDefault, match.Length);
			if (source.IsSingleSegment)
			{
				if (source.First.Span.SequenceEqual(match))
				{
					position = source.Start;
					return true;
				}
				sequence = sequence.Slice(source.Slice(1L).Start);
				continue;
			}
			T[] array = ArrayPool<T>.Shared.Rent(match.Length);
			try
			{
				BuffersExtensions.CopyTo(in source, array);
				if (array.AsSpan(0, match.Length).SequenceEqual(match))
				{
					position = source.Start;
					return true;
				}
				sequence = sequence.Slice(source.Slice(1L).Start);
			}
			finally
			{
				ArrayPool<T>.Shared.Return(array);
			}
		}
		position = sequence.End;
		return false;
	}
}
