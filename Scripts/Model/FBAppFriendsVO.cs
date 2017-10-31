using UnityEngine;
using System.Collections;
using LitJson;
using Newtonsoft.Json;

public class FBPictureData
{
    public bool is_silhouette;
    public string url;
}

public class FBPicture
{
    public FBPictureData data { set; get; }
}

public class FriendsData
{
    public bool installed { set; get; }
    public string id { set; get; }
    public string name { set; get; }
    public FBPicture pciture { set; get; }
}

public class FBPacket
{
    public FriendsData[] data { set; get; }
}


public class FBAppFriendsVO
{
    public FBPacket _packet { set; get; }
    public FBAppFriendsVO(string strRawResult)
    {
        this._packet = JsonConvert.DeserializeObject<FBPacket>(strRawResult);
    }
}
