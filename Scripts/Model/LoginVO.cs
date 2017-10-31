using UnityEngine;
using System.Collections;

#region RequestProtocol
/// <summary>
/// Login request packet data
/// </summary>
public class ReqLoginItem
{
	public string userID {get; set;}
	public string ver {get; set;}
	public string signedRequest {get; set;}
	
	public ReqLoginItem(string userID, string ver, string signReq)
	{
		this.userID = userID;
		this.ver = ver;
		this.signedRequest = signReq;
	}
}

/// <summary>
/// Login request packet info
/// </summary>
public class CmdLogin
{
	public ReqLoginItem data {get; set;}
	public string cmd {get; set;}
	public CmdLogin(string cmd, ReqLoginItem data)
	{
		this.cmd = cmd;
		this.data = data;
	}
}
#endregion  // RequestProtocol

#region ResponseProtocol

/// <summary>
/// Login response packet info
/// </summary>
public class LoginData 
{
	public long balance {get; set;}
	public int level {get; set;}
	public int levelPercent {get; set;}
	public int gameLevel {get; set;}
	public int gameLevelPercent {get; set;}
	public long jackpotPool {get; set;}
	public int last_line_bet {get; set;}
	public int min_line_bet {get; set;}
    public int max_line_bet { get; set; }

    public long[] subjackpotPool { get; set; }
}

/// <summary>
/// Login response packet info
/// </summary>
public class LoginVO : PacketData {
	public LoginData data {get; set;}	
}
#endregion // ResponseProtocol
