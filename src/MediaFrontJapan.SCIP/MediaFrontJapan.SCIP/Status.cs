using System;
using System.Text;

namespace MediaFrontJapan.SCIP;

public readonly struct Status : IEquatable<Status>
{
	private readonly ushort value;

	private static readonly Encoding ExceptionFallBackASCIIEncoding = Encoding.GetEncoding(20127, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

	public static Status Accepted => new Status(CommonStatusCode.Accepted);

	public static Status ErrorAbnormalState => new Status(CommonStatusCode.ErrorAbnormalState);

	public static Status ErrorUnstable => new Status(CommonStatusCode.ErrorUnstable);

	public static Status ErrorCommandNotDefined => new Status(CommonStatusCode.ErrorCommandNotDefined);

	public static Status ErrorCommandNotSupported => new Status(CommonStatusCode.ErrorCommandNotSupported);

	public static Status ErrorDenied => new Status(CommonStatusCode.ErrorDenied);

	public static Status ErrorUserStringLong => new Status(CommonStatusCode.ErrorUserStringLong);

	public static Status ErrorUserStringCharacter => new Status(CommonStatusCode.ErrorUserStringCharacter);

	public static Status ErrorCommandShort => new Status(CommonStatusCode.ErrorCommandShort);

	public static Status ErrorCommandLong => new Status(CommonStatusCode.ErrorCommandLong);

	public static Status ErrorParameter1 => new Status(CommonStatusCode.ErrorParameter1);

	public static Status ErrorParameter2 => new Status(CommonStatusCode.ErrorParameter2);

	public static Status ErrorParameter3 => new Status(CommonStatusCode.ErrorParameter3);

	public static Status ErrorParameter4 => new Status(CommonStatusCode.ErrorParameter4);

	public static Status ErrorParameter5 => new Status(CommonStatusCode.ErrorParameter5);

	public static Status ErrorParameter6 => new Status(CommonStatusCode.ErrorParameter6);

	public static Status ErrorParameter7 => new Status(CommonStatusCode.ErrorParameter7);

	public bool IsAbnormalState => value == 19504;

	public Status(CommonStatusCode commonStatusCode)
	{
		value = (ushort)commonStatusCode;
	}

	public Status(ushort value)
	{
		this.value = value;
	}

	public Status(byte char1, byte char2)
	{
		value = (ushort)(char1 | (char2 << 8));
	}

	public unsafe static Status Parse(string statusCode)
	{
		if (statusCode.Length != 2)
		{
			throw new FormatException("Status code must be 2char ASCII.");
		}
		try
		{
			fixed (char* chars = statusCode)
			{
				Status result = default(Status);
				ExceptionFallBackASCIIEncoding.GetBytes(chars, statusCode.Length, (byte*)(&result), sizeof(Status));
				return result;
			}
		}
		catch (Exception innerException)
		{
			throw new FormatException("Status code must be 2char ASCII.", innerException);
		}
	}

	public void ThrowExceptionForStatus()
	{
		Exception exceptionForStatus = GetExceptionForStatus();
		if (exceptionForStatus != null)
		{
			throw exceptionForStatus;
		}
	}

	public Exception? GetExceptionForStatus()
	{
		switch (value)
		{
		default:
			return null;
		case 19504:
			return new InvalidOperationException("Device is in abnormal state. ErrorCode:" + ToString());
		case 19760:
			return new InvalidOperationException("Device is unstable. ErrorCode:" + ToString());
		case 17712:
			return new FormatException("Undefined command. ErrorCode:" + ToString());
		case 17968:
			return new NotSupportedException("Not supported command. ErrorCode:" + ToString());
		case 12337:
			return new InvalidOperationException("Not compatible command for current state. ErrorCode:" + ToString());
		case 18224:
			return new FormatException("Too long user defined string. ErrorCode:" + ToString());
		case 18480:
			return new FormatException("Invalid format user defined string. ErrorCode:" + ToString());
		case 17200:
			return new FormatException("Too short command. ErrorCode:" + ToString());
		case 17456:
			return new FormatException("Too long command. ErrorCode:" + ToString());
		case 12592:
		case 12848:
		case 13104:
		case 13360:
		case 13616:
		case 13872:
		case 14128:
			return new ArgumentException("Invalid parameter. ErrorCode:" + ToString());
		}
	}

	public unsafe override string ToString()
	{
		fixed (Status* ptr = &this)
		{
			void* bytes = ptr;
			return Encoding.ASCII.GetString((byte*)bytes, sizeof(Status));
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is Status other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Status other)
	{
		return value == other.value;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public static bool operator ==(Status left, Status right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Status left, Status right)
	{
		return !(left == right);
	}
}
