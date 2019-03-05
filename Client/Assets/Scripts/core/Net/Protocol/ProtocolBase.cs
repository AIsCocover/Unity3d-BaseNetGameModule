using System;

public class ProtocolBase
{
	/// <summary>
	/// Decode the readBuff from start to start+length. Package readBuff as a protocol and return it.
	/// </summary>
	/// <param name="readBuff">Read buff.</param>
	/// <param name="start">Type: int, decode begin from start.</param>
	/// <param name="length">Type: int, decode from begin to start+length.</param>
	public virtual ProtocolBase Decode(byte[] readBuff, int start, int length)
	{
		return new ProtocolBase ();
	}

	/// <summary>
	/// Encode the protocol's content to binary array.
	/// </summary>
	public virtual byte[] Encode()
	{
		return new byte[] { };
	}

	/// <summary>
	/// Return the protocol's name.
	/// </summary>
	/// <returns>The name.</returns>
	public virtual string GetName()
	{
		return "";
	}

	/// <summary>
	/// Return the protocol's description.
	/// </summary>
	/// <returns>The desc.</returns>
	public virtual string GetDesc()
	{
		return "";
	}
}
