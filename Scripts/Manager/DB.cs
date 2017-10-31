using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using xLIB;

/// <summary>
/// 자료형 데이터및 테스쳐 데이터 관리
/// </summary>
/// <seealso cref="xLIB.Singleton{DB}" />
public class DB : SingletonSchedule<DB>
{
    public enum IDs {
        WinCast,
    }
    /// <summary>
    /// 
    /// </summary>
    private static Dictionary<string, Texture2D> _dicGameIcons = new Dictionary<string, Texture2D>();

    public delegate void LoadPictureCallback(Texture2D texture);
    protected List<PK.WinCast.REData> CastList = new List<PK.WinCast.REData>();
    protected List<PK.WinCast.REData> TopCastList = new List<PK.WinCast.REData>();


    public override void virAwake()
    {
        SetCallback_HandleMessage(ParserCommand);
    }

    public void Initialize()    {}

    void ParserCommand(Hashtable has)
    {
        IDs _id = (IDs)has["id"].GetHashCode();
        if(_id == IDs.WinCast)
        {
            if(CastList.Count <= 5)
            {
                PK.WinCast.RECEIVE pk = (PK.WinCast.RECEIVE)has["data"];
                CastList.Add(pk.data);
                TopCastList.Add(pk.data);
                Debug.Log("pk data name = " + pk.data.userName + " , game : " + pk.data.gameName);
            }
            base.remove(IDs.WinCast);
        }
    }

    public int GetCurrentWinCastCount() { return CastList.Count; }
    public List<PK.WinCast.REData> GetWinCastList()
    {
        List<PK.WinCast.REData> temp = new List<PK.WinCast.REData>();
        for (int i = 0; i < CastList.Count; i++)
            temp.Add(CastList[i]);
        CastList.Clear();
        return temp;
    }

    public int GetCurrentWinTopCastCount() { return TopCastList.Count; }
    public List<PK.WinCast.REData> GetWinTopCastList()
    {
        List<PK.WinCast.REData> temp = new List<PK.WinCast.REData>();
        for (int i = 0; i < TopCastList.Count; i++)
            temp.Add(TopCastList[i]);
        TopCastList.Clear();
        return temp;
    }

    public void WebDownloadImage(string url, LoadPictureCallback callback)
    {
        // 로드한 이미지가 있는지 확인.
        if (_dicGameIcons.ContainsKey(url))
        {
            Texture2D txt;
            _dicGameIcons.TryGetValue(url, out txt);
            if (txt != null)
            {
                if (callback != null) callback(txt);
                return;
            }
        }
        // 이미지 다운로드 등록
        StartCoroutine(LoadPictureEnumerator(url, pic =>
        {
            if (pic != null) _dicGameIcons.Add(url, pic);
            if (callback != null) callback(pic);
        }));
    }
    IEnumerator LoadPictureEnumerator(string url, LoadPictureCallback callback)
    {
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) callback(null);
        else callback(www.texture);
    }

    public class Icon
    {
        static public Sprite GetInboxSprite(string filename)
        {
            return BUNDLE.I.LoadAsset<Sprite>(filename);
            //return Resources.Load<Sprite>("Icons/Inbox/" + filename);
        }
        static public Sprite GetFrinedsRankSprite(int rank)
        {
            return BUNDLE.I.LoadAsset<Sprite>("i_friends_medal_" + rank);
            //return Resources.Load<Sprite>("Icons/Friends/i_friends_medal_" + rank);
        }
    }
}
