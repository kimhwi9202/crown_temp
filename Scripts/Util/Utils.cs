using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Collection of utility functions
/// </summary>
public class Utils {
    const string TAG = "Utils";

    public delegate void DelayCallFunction();

    /// <summary>Convert Color32 to Hex string</summary>
    /// <returns>Color value (Hex formatted)</returns>
    public static string colorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}
	
	/// <summary>Convert Hex string to Color32</summary>
	/// <returns>Color value (Color32 formatted)</returns>
	public static Color hexToColor(string hex)
	{
		hex = hex.Replace ("0x", "");	//in case the string is formatted 0xFFFFFF
		hex = hex.Replace ("#", "");	//in case the string is formatted #FFFFFF
		byte a = 255;					//assume fully visible unless specified in hex
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		
		//Only use alpha if the string has enough characters
		if(hex.Length == 8){
		a = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		}
		return new Color32(r,g,b,a);
	}
	
	private static List<string> suffixes = new List<string> { "", "K", "M", "G", "T", "P" };


	/// <summary>Convert integer number to shorten-number string</summary>
	/// <remarks>Ex) Convert 1000 to 1K, 10000000 to 1M</remarks>
	/// <returns>String formated shorten number</returns>
	public static string GetShortenNumber(int number) {
        for (int i = 0; i < suffixes.Count; i++)
        {
            int temp = number / (int)Mathf.Pow(1000, i + 1);
            if (temp == 0)
            {
                return (number / Mathf.Pow(1000, i)) + " " + suffixes[i];
            }
        }
        return number.ToString();
	}

    /// <summary>
    /// 서버접속 정보를 ip주소와 port정보로 나누어 반환
    /// </summary>
    /// <param name="serverAddr">The server addr.</param>
    /// <param name="ipAddress">The ip address.</param>
    /// <param name="port">The port.</param>
    public static void GetServerIpPortInfo(string serverAddr, out string ipAddress, out int port)
    {
        //dev.sloticagames.com:12400
        string[] serverInfo = serverAddr.Split(':');
        ipAddress = serverInfo[0];
        port = int.Parse(serverInfo[1]);
    }

    public static string DomainToIPAddress(string domainName, bool favorIpV6 = false)
    {
        var favoredFamily = favorIpV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
        IPAddress[] ips = Dns.GetHostAddresses(domainName);
        //return addrs.FirstOrDefault(addr => addr.AddressFamily == favoredFamily)
        //       ??
        //       addrs.FirstOrDefault();
        foreach(IPAddress ipAddr in ips)
        {
            if(ipAddr.AddressFamily == favoredFamily)
            {
                //CLog.Log(TAG, "## Utils : DomainToIPAddress >> ipAddress = " + ipAddr.ToString());
                return ipAddr.ToString();
            }
        }
        return null;
    }

    public static string ConvertArrayToString(string[] array)
    {
        string temp = "";
        for(int i = 0; i < array.Length; i++)
        {
            temp += array[i];
            temp += ", ";
        }
        return temp;
    }

}
