using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class FBPictureItemData
{
    public bool is_silhouette { get; set; }
    public string url { get; set; }
}

public class FBPictureItem
{
    public FBPictureItemData data { get; set; }
}

public class FBInvitableFriendItem
{
    public string id { get; set; }
    public string name { get; set; }
    public FBPictureItem picture { get; set; }

    /// <summary>
    /// Gets the invite identifier.
    /// </summary>
    /// <returns></returns>
    public string GetInviteID()
    {
        return id;
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return name;
    }

    /// <summary>
    /// Gets the picture URL.
    /// </summary>
    /// <returns></returns>
    public string GetPictureURL()
    {
        return picture.data.url;
    }
}

public class FBInvitableFriendData
{
    public FBInvitableFriendItem[] data { get; set; }
}

public class FBInvitableFriendsVO
{
    public FBInvitableFriendData data;

    public FBInvitableFriendsVO(string strRawResult)
    {
        this.data = JsonConvert.DeserializeObject<FBInvitableFriendData>(strRawResult);
    }

    public string DebugString()
    {
        string ret = "FBInvitableFriendsVO info >> count = " + this.data.data.Length + "\n";
        for (int i = 0; i < this.data.data.Length; i++)
        {
            FBInvitableFriendItem friendInfo = this.data.data[i];
            ret += "name = " + friendInfo.GetName() + ", picture = " + friendInfo.GetPictureURL() + "\n";
        }

        return ret;
    }
}
