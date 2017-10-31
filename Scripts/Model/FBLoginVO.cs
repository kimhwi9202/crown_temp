using UnityEngine;
using System.Collections;
using LitJson;

/// <summary>
/// 페이스북 로그인 API 응답 데이터
/// </summary>
public class FBLoginVO {

    public string id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string picUrl { get; set; }
    public string first_name { get; set; }
    public string gender { get; set; }
    public string last_name { get; set; }
    public string locale { get; set; }
    public int timezone { get; set; }

    public FBLoginVO(string strId, string strName, string strEmail, string strPicUrl, 
            string strFirstName, string strGender, string strLastName, string strLocale, int nTimeZone)
    {
        this.id = strId;
        this.name = strName;
        this.email = strEmail;
        this.picUrl = strPicUrl;
        this.first_name = strFirstName;
        this.gender = strGender;
        this.last_name = strLastName;
        this.locale = strLocale;
        this.timezone = nTimeZone;
    }

    public FBLoginVO(string strRawResult)
    {
        JsonData data = JsonMapper.ToObject(strRawResult);
        id = (string)data["id"];
        name = (string)data["name"];
        try {
            email = (string)data["email"];
        }
        catch
        {
            email = "";
        }
        
        picUrl = (string)data["picture"]["data"]["url"];
        first_name = (string)data["first_name"];
        gender = (string)data["gender"];
        last_name = (string)data["last_name"];
        locale = (string)data["locale"];
        timezone = (int)data["timezone"];
    }

    new public string ToString()
    {
        return "id = " + id + ", name=" + name + ", email=" + email + ", picUrl=" + picUrl +
            ", first_name=" + first_name + ", gender=" + gender + ", last_name=" + last_name + ", locale=" + locale + ", timezone=" + timezone;
    }
}
