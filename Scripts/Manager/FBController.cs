using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Newtonsoft.Json;
using System.Linq;

using Facebook.MiniJSON;

public class FBController : MonoBehaviour
{
    public delegate void OnInitializedCallback(bool isInitialized, bool isLoggedIn, string msg);
    public delegate void OnUserInfoCallback(bool isComplete);
    private FBLoginVO _fbLoginVO = null;

    public FBAppFriendsVO _FBFriends = null;
    public FBInvitableFriendsVO _FBInvitableFriends = null;
    public List<string> _AppFriendsIDs = new List<string>();

    #region Initialize
    public void Initialize(OnInitializedCallback callback)
    {
        /*
                string RawResult = "{\"to\":\"2017504565143641,1275128422565917\",\"callback_id\":\"2\",\"request\":\"732056516973623\"}";
                var responseObject = Json.Deserialize(RawResult) as Dictionary<string, object>;
                string[] ids = xLIB.xCSVParser.SplitCsvLine(responseObject["to"].ToString());
                List<string> toIDs = new List<string>();
                toIDs = ids.ToList<string>();
                //Debug.Log(list.ToString());
        */
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(()=> 
            {
                if (FB.IsInitialized)
                {
                    // Signal an app activation App Event
                    FB.ActivateApp();

                    FB.Mobile.FetchDeferredAppLinkData(DeepLinkCallback);

                    // 인증되었던 유저라면 바로 접속처리
                    if (FB.IsLoggedIn)
                    {
                        string userID = AccessToken.CurrentAccessToken.UserId;
                        string accessToken = AccessToken.CurrentAccessToken.ToString();
                        Debug.Log("## FBController : InitCallback >> userID = " + userID + ", accessToken = " + accessToken);
                        if (callback != null) callback(true, true, "");
                        //LoadUserInfo();
                    }
                    else
                    {
                        //Debug.Log("## FBController : InitCallback >> NOT LOGGEDIN");
                        if (callback != null) callback(true, false, "Failed to Initialize FB Not Loggedin");
                    }
                }
                else
                {
                    //Debug.LogError("## FBController : InitCallback >> Failed to Initialize the Facebook SDK");
                    if (callback != null) callback(false, false, "Failed to Initialize the Facebook SDK");
                }
            }, OnHideUnity);
            //Debug.Log("## FBController : Initialize >> IsInitialized >> NOT YET");
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
            //Debug.Log("## FBController : Initialize >> IsInitialized >> OK");
        }
    }

    void DeepLinkCallback(IAppLinkResult result)
    {
        if (!string.IsNullOrEmpty(result.Url))
        {
            Debug.Log(result.Url);
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }
    #endregion //Initialize


    /// <summary>
    /// 인증이 안된 유저의 첫 페이스북 로그인 
    /// </summary>
    public void FBLogin(OnUserInfoCallback complete)
    {
#if UNITY_EDITOR
        string tempResult = PlayerPrefHelper.GetFBLoginResult();
        if (!string.IsNullOrEmpty(tempResult))
        {
            // 유저 정보 
            string strResult = tempResult.Replace(@"\/", @"/");
            _fbLoginVO = new FBLoginVO(strResult);
            // frineds
            tempResult = PlayerPrefHelper.GetFBFriendsResult();
            if (!string.IsNullOrEmpty(tempResult))
            {
                strResult = tempResult.Replace(@"\/", @"/");
                _FBFriends = new FBAppFriendsVO(strResult);
                _AppFriendsIDs = new List<string>();
                for (int i = 0; i < _FBFriends._packet.data.Length; i++)
                {
                    if (_FBFriends._packet.data[i].installed)
                        _AppFriendsIDs.Add(_FBFriends._packet.data[i].id);
                }
            }
            // Invitable frineds
            tempResult = PlayerPrefHelper.GetFBInvitableFriendsResult();
            strResult = tempResult.Replace(@"\/", @"/");
            _FBInvitableFriends = new FBInvitableFriendsVO(strResult);

            if (complete != null) complete(true);
            return;
        }
#endif

        var perms = new List<string>() { "public_profile", "email", "user_friends" };
        FB.LogInWithReadPermissions(perms, (result)=> 
        {
            if (result == null) {
                if (complete != null)   complete(false);
                return;
            }
            if (!string.IsNullOrEmpty(result.Error)){
                Debug.Log("## FBController : CallbackUserInfo >> Error = " + result.Error);
                if (complete != null)   complete(false);
            }
            else if (result.Cancelled){
                Debug.Log("## FBController : CallbackUserInfo >> Cancelled = " + result.Cancelled);
                if (complete != null)   complete(false);
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                xLitJson.JsonData data = xLitJson.JsonMapper.ToObject(result.RawResult);
                string strAccessToken = (string)data["access_token"];
                PlayerPrefHelper.SetFbAccessToken(strAccessToken);
                Debug.Log("## FBController : CallbackAuth >> Success Response = " + result.RawResult);
                LoadUserInfo(complete);
            }
        });
    }

    /// <summary>
    /// 페이스북 로그인을 성공하면 페이스북을 통하여 사용자 정보를 받아온다.
    /// </summary>
    public void LoadUserInfo(OnUserInfoCallback complete)
    {
        FB.API("me?fields=id,name,email,picture.width(300),first_name,gender,last_name,locale,timezone", HttpMethod.GET, (result) => 
        {
            if (result == null)
            {
                if(complete != null)
                    complete(false);
                return;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.Log("## FBController : CallbackUserInfo >> Error = " + result.Error);
                //UI.Popup.ShowNoticeBox("FB User Info : " + result.Error, null);
                if (complete != null)
                    complete(false);
            }
            else if (result.Cancelled)
            {
                Debug.Log("## FBController : CallbackUserInfo >> Cancelled = " + result.Cancelled);
                //UI.Popup.ShowNoticeBox("FB User Info : Cancelled", null);
                if (complete != null)
                    complete(false);
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
#if UNITY_EDITOR
                PlayerPrefHelper.SetFBLoginResult(result.RawResult);
#endif
                SetFBLoginInfo(result.RawResult);
                PlayerPrefHelper.SetLoginType(eLoginType.facebook);
                if (complete != null)
                    complete(true);
            }
        });
    }

#region Facebook SDK 연동 함수
    public FBLoginVO GetFbLoginInfo()
    {
        return _fbLoginVO;
    }
    public void SetFBLoginInfo(string RawResult)
    {
        // fb 유저정보 기록
        string strResult = RawResult.Replace(@"\/", @"/");
        _fbLoginVO = new FBLoginVO(strResult);
        Debug.Log("## USER:SetFBLoginInfo >> RawResult = " + strResult);
        // fb게임 친구 정보 요청
        StartCoroutine(LoadAppFriends());
        // fb초대가능유저 정보 요청
        GetFBInvitableFriends(null);
    }
    private void ProfilePhotoCallback(IGraphResult result)
    {
        if (string.IsNullOrEmpty(result.Error) && result.Texture != null)
        {
            USER.I.CurProfileTexture = result.Texture;
            Debug.Log("## USER:ProfilePhotoCallback >> " + result.Texture.texelSize);
        }else
        {
            Debug.Log("## ProfilePhotoCallback #Error = " + result.Error);
        }
    }
    public IEnumerator LoadAppFriends(Utils.DelayCallFunction Function = null)
    {
        bool isCallBack = false;

        yield return new WaitForSeconds(1.0f);
        FB.API("me/friends?fields=installed,id,name,picture.width(300)", HttpMethod.GET, (result) =>
        {
            CallbackFriendsInfo(result);
            isCallBack = true;
        }
        );

        if (Function != null)
        {
            while (isCallBack == false)
            {
                yield return null;
            }
            Function();
        }
    }


    public void GetFBInvitableFriends(System.Action complete)
    {
        StartCoroutine(LoadInvitableFriends(complete));
    }
    IEnumerator LoadInvitableFriends(System.Action complete)
    {
        yield return new WaitForSeconds(1.0f);
        string meQueryString =  "/me?fields=id,name,invitable_friends.limit(50).fields(name,id,picture.width(100).height(100))";
        //FB.API(meQueryString, HttpMethod.GET, APICallback);
        FB.API("me/invitable_friends?fields=id,name,picture.width(300)", HttpMethod.GET, (result) => {

            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.Log("## LoadInvitableFriends #Error = " + result.Error);

                if (complete != null) complete();
                return;
            }

            if(result.Cancelled)
            {
                if (complete != null) complete();
                return;
            }

#if UNITY_EDITOR
                PlayerPrefHelper.SetFBInvitableFriendsResult(result.RawResult);
#endif
                string strResult = result.RawResult;
                strResult = strResult.Replace(@"\/", @"/");
                Debug.Log("## CallbackInvitableFriends >> strResult = " + strResult);

                _FBInvitableFriends = new FBInvitableFriendsVO(strResult);
                //CLog.Log2(TAG, "CallbackInvitableFriends >> after parse = " + _invitableFriendsVO.DebugString());

            if (complete != null) complete();
        });
    }



    void CallbackFriendsInfo(IResult result)
    {
        if (result == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("## CallbackFriendsInfo #Error = " + result.Error);
            //CLog.Log2(TAG, "## CallbackUserInfo >> HandleResult >> Error Response:" + result.Error);
        }
        else if (result.Cancelled)
        {
            //CLog.Log2(TAG, "## CallbackUserInfo >> HandleResult >> Cancelled Response:" + result.RawResult);
        }
        else if (!string.IsNullOrEmpty(result.RawResult))
        {
#if UNITY_EDITOR
            PlayerPrefHelper.SetFBFriendsResult(result.RawResult);
#endif
            string strResult = result.RawResult;
            strResult = strResult.Replace(@"\/", @"/");
            Debug.Log("## CallbackFriendsInfo >> strResult = " + strResult);

            _FBFriends = new FBAppFriendsVO(strResult);
            _AppFriendsIDs = new List<string>();
            for (int i = 0; i < _FBFriends._packet.data.Length; i++)
            {
                if (_FBFriends._packet.data[i].installed)
                    _AppFriendsIDs.Add(_FBFriends._packet.data[i].id);
            }

            // 소켓통신
            //if (appFriends.Count > 0)
            //    LobbyAppFriends(appFriends.ToArray());
        }
    }

    void CallbackInvitableFriends(IResult result)
    {
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("## CallbackInvitableFriends #Error = " + result.Error);
            return;
        }

#if UNITY_EDITOR
        PlayerPrefHelper.SetFBInvitableFriendsResult(result.RawResult);
#endif
            string strResult = result.RawResult;
            strResult = strResult.Replace(@"\/", @"/");
            Debug.Log("## CallbackInvitableFriends >> strResult = " + strResult);

            _FBInvitableFriends = new FBInvitableFriendsVO(strResult);
            //CLog.Log2(TAG, "CallbackInvitableFriends >> after parse = " + _invitableFriendsVO.DebugString());
   
    }
#endregion  // Facebook SDK 연동 함수


    public void FBInvite(List<string> inviteToken, System.Action<bool, List<string>> complete)
    {
/*
        string RawResult = "{\"to\":\"2017504565143641,1275128422565917\",\"callback_id\":\"2\",\"request\":\"732056516973623\"}";
        var responseObject = Json.Deserialize(RawResult) as Dictionary<string, object>;
*/
        FB.AppRequest(
            "This gmae is awesome, join me. now!",
            inviteToken,
            null,
            null, null, null,
            "Invite your firends",
        
            (result) => {

                if (!string.IsNullOrEmpty(result.Error))
                {
                    Debug.Log("## FBInvite #Error = " + result.Error);
                    if (complete != null) complete(false, null);
                    return;
                }

                if (!result.Cancelled)
                {
                    List<string> toIDs = new List<string>(); 
                    var responseObject = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
                    string[] ids = xLIB.xCSVParser.SplitCsvLine(responseObject["to"].ToString());
                    toIDs = ids.ToList<string>();

                    if (complete != null) complete(true,toIDs);
                }
                else if (complete != null) complete(false,null);
            });
        
    }

    public void FBSendGift(List<string> fbIDs, System.Action<bool, List<string>> complete)
    {
        /*
                string RawResult = "{\"to\":\"2017504565143641,1275128422565917\",\"callback_id\":\"2\",\"request\":\"732056516973623\"}";
                var responseObject = Json.Deserialize(RawResult) as Dictionary<string, object>;
        */
        FB.AppRequest(
            "sent you Free Coins! Click to collect!",
            fbIDs,
            null,
            null, null, null,
            "Send Gift",

            (result) => {

                if (!string.IsNullOrEmpty(result.Error))
                {
                    Debug.Log("## FBSendGift #Error = " + result.Error);
                    if (complete != null) complete(false, null);
                    return;
                }

                if (!result.Cancelled)
                {
                    Debug.Log(result.RawResult + " / " + result.Error);
                    List<string> toIDs = new List<string>();
                    var responseObject = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
                    string[] ids = xLIB.xCSVParser.SplitCsvLine(responseObject["to"].ToString());
                    toIDs = ids.ToList<string>();

                    if (complete != null) complete(true, toIDs);
                }
                else if (complete != null) complete(false, null);
            });

    }


    //공유하기 ( 서버에서 받은 정보로 조합해서 사용 )
    public void Share(string linkUrl, string imgUrl, System.Action complete)
    {
        // 순서 변경시 컴파일 에러 발생
        // linkCaption : 링크 설명(링크 본문)
        // picutre : 링크 사진
        // linkName : 링크 제목
        // link : 연결할 링크 주소
        /*
        FB.FeedShare(
            linkCaption: "I'm playing this awesome game",
            picture: "http://greyzoned.com/images/evilelf2_icon.png",
            linkName: "Check out this game",
            link: "http://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserID : "guest")
            );
        */

        if (string.IsNullOrEmpty(linkUrl)) linkUrl = "http://apps.facebook.com/";

        FB.FeedShare(
         link: new System.Uri(linkUrl),
         linkName: "The Larch",
         linkCaption: "I thought up a witty tagline about larches",
         linkDescription: "There are a lot of larch trees around here, aren't there?",
         picture: new System.Uri(imgUrl),
         callback: (result) => {
             if (!string.IsNullOrEmpty(result.Error))
             {
                 Debug.Log("## Share #Error = " + result.Error);
             }
             if (complete != null) complete();            
         }
       );
        /*
        FB.FeedShare(
         link: new System.Uri("https://example.com/myapp/?storyID=thelarch"),
         linkName: "The Larch",
         linkCaption: "I thought up a witty tagline about larches",
         linkDescription: "There are a lot of larch trees around here, aren't there?",
         picture: new System.Uri("https://example.com/myapp/assets/1/larch.jpg"),
         callback: (result) => {
             if (complete != null) complete();
         }
       );
       */
    }

    //친구초대
    public void InviteFriends()
    {
        // 순서 변경시 컴파일 에러 발생
        // message : 보낼 메시지
        // title : 메시지 보낼 친구목록 창의 타이틀
        FB.AppRequest(
            message: "This gmae is awesome, join me. now!",
            title: "Invite your firends to join you"
        );
    }
}
