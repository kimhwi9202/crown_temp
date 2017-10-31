using UnityEngine;
using System.Collections;
using LitJson;

// 게스트 로그인
public class STPKDataGuestUserJoin
{
    public long pid { get; set; }
    public string error { get; set; }
}
public class PKGuestUserJoin : PacketData
{
    public STPKDataGuestUserJoin data { get; set; }
}
public class CmdGuestUserJoin
{
    public string cmd { get; set; }
    public CmdGuestUserJoin(string cmd)
    {
        this.cmd = cmd;
    }
}


// 게스트 로그인 정보로 페이스북 전환
public class STPKDataGuestToFacebook
{
    public string success { get; set; }
    public string error { get; set; }
}
public class PKGuestToFacebook : PacketData
{
    public STPKDataGuestUserJoin data { get; set; }
}
public class CmdDataGuestToFacebook
{
    public long guest_id { get; set; }
    public long facebook_id { get; set; }
}
public class CmdGuestToFacebook
{
    public string cmd { get; set; }
    public CmdDataGuestToFacebook data { get; set; }
    public CmdGuestToFacebook(string cmd, long guestId, long facebookId)
    {
        this.cmd = cmd;
        data = new CmdDataGuestToFacebook();
        data.guest_id = guestId;
        data.facebook_id = facebookId;
    }
}
