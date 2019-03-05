using System;

/// <summary>
/// String Protocol
/// Protocol Format:
/// 	"[protocol name],[data1],[data2],..."
/// 	protocol name: 	string
/// 	data:	string
/// </summary>
public class ProtocolStr : ProtocolBase
{
	public string str;

	public override ProtocolBase Decode (byte[] readBuff, int start, int length)
	{
		ProtocolStr proto = new ProtocolStr ();
		proto.str = System.Text.Encoding.UTF8.GetString (readBuff, start, length);
		return (ProtocolBase)proto;
	}

	public override byte[] Encode ()
	{
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes (str);
		return bytes;
	}

	public override string GetName ()
	{
		if (str.Length == 0)
			return "";
		return str.Split (',') [0];
	}

	public override string GetDesc ()
	{
		return str;
	}
}


