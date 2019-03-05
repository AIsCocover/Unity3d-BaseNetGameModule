using System;
using System.Linq;

/// <summary>
/// Bytes Protocol
/// Protocol Format:
/// 	[protocollength][protocol name][data1][data2]...
/// 	protocollength: Int32 -> byte
/// 	protocol name: 	byte
/// 	data:	byte
/// </summary>
public class ProtocolBytes : ProtocolBase
{
	public byte[] bytes;

	public override ProtocolBase Decode (byte[] readBuff, int start, int length)
	{
		ProtocolBytes proto = new ProtocolBytes ();
		proto.bytes = new byte[length];
		Array.Copy (readBuff, start, proto.bytes, 0, length);
		return (ProtocolBase)proto;
	}

	public override byte[] Encode ()
	{
		return bytes;
	}

	public override string GetName ()
	{
		return GetString (0);
	}

	public override string GetDesc ()
	{
		if (bytes == null)
			return "";
		string str = "";
		for (int i = 0; i < bytes.Length; i++) {
			int t = (int)bytes [i];
			str += t.ToString () + " ";
		}

		return str;
	}

	#region Help Function
	#region String Operation
	/// <summary>
	/// convert the str into bytes and connect it behind the protocol.
	/// </summary>
	/// <param name="str">the connected string</param>
	public void AddString(string str)
	{
		Int32 length = str.Length;
		byte[] lenBytes = BitConverter.GetBytes (length);
		byte[] strBytes = System.Text.Encoding.UTF8.GetBytes (str);
		if (bytes == null)
			bytes = lenBytes.Concat (strBytes).ToArray ();
		else
			bytes = bytes.Concat (lenBytes).Concat (strBytes).ToArray ();
	}
	/// <summary>
	/// Get a complete string from start to end.
	/// </summary>
	/// <returns>string</returns>
	/// <param name="start">start index</param>
	/// <param name="end">end index</param>
	public string GetString(int start, ref int end)
	{
		if (bytes == null)
			return "";
		if (bytes.Length < start + sizeof(Int32))
			return "";

		Int32 strLen = BitConverter.ToInt16 (bytes, start);
		if (bytes.Length < start + sizeof(Int32) + strLen)
			return "";

		string str = System.Text.Encoding.UTF8.GetString (bytes, start + sizeof(Int32), strLen);
		end = start + sizeof(Int32) + strLen;
		return str;
	}
	/// <summary>
	/// Get a complete string begin at start.
	/// </summary>
	/// <returns>string</returns>
	/// <param name="start">start index</param>
	public string GetString(int start)
	{
		int end = 0;
		return GetString (start, ref end);
	}
	#endregion

	#region Int32 Operation
	/// <summary>
	/// convert the Int32 into bytes and connect it behind the protocol.
	/// </summary>
	/// <param name="num">Number.</param>
	public void AddInt32(Int32 num)
	{
		byte[] numBytes = BitConverter.GetBytes (num);
		if (bytes == null)
			bytes = numBytes;
		else
			bytes = bytes.Concat (numBytes).ToArray ();
	}
	/// <summary>
	/// Get a complete Int32 num from protocol's start to end.
	/// </summary>
	/// <returns>Int32</returns>
	/// <param name="start">start index</param>
	/// <param name="end">end index</param>
	public Int32 GetInt(int start, ref int end)
	{
		if (bytes == null)
			return 0;
		if (bytes.Length < start + sizeof(Int32))
			return 0;
		end = start + sizeof(Int32);
		return BitConverter.ToInt32 (bytes, start);
	}
	/// <summary>
	/// Get a complete Int32 num from protocol's start.
	/// </summary>
	/// <returns>Int32.</returns>
	/// <param name="start">start index</param>
	public Int32 GetInt(int start)
	{
		int end = 0;
		return GetInt (start, ref end);
	}
	#endregion

	#region Float Operation
	/// <summary>
	/// convert the float into bytes and connect it behind the protocol.
	/// </summary>
	/// <param name="num">float number</param>
	public void AddFloat(float num)
	{
		byte[] numBytes = BitConverter.GetBytes (num);
		if (bytes == null)
			bytes = numBytes;
		else
			bytes = bytes.Concat (numBytes).ToArray ();
	}
	/// <summary>
	/// Get a complete float num from protocol's start to end.
	/// </summary>
	/// <returns>The float.</returns>
	/// <param name="start">start index</param>
	/// <param name="end">end index</param>
	public float GetFloat(int start, ref int end)
	{
		if (bytes == null)
			return 0;
		if (bytes.Length < start + sizeof(float))
			return 0;
		end = start + sizeof(float);
		return BitConverter.ToSingle (bytes, start);
	}
	/// <summary>
	/// Get a complete float num from protocol's start.
	/// </summary>
	/// <returns>The float.</returns>
	/// <param name="start">start index</param>
	public float GetFloat(int start)
	{
		int end = 0;
		return GetFloat (start, ref end);
	}
	#endregion
	#endregion
}
