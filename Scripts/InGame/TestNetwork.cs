using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;

/// <summary>
/// 인게임 패킷 로컬 테스트용이다.. 
/// 개발과정에서 특정 패킷을 반복적으로 확인하기 위해 만들었다.
/// 원하는 패킷은 TEST 폴더에 txt 파일로 기록해서 사용
/// *** 주의 사항 ***
/// 이 스크립터는 유니티 에디터에서만 운용해라.. 
/// </summary>
public class TestNetwork : MonoBehaviour
{

    static public TestNetwork I;

#if UNITY_EDITOR
    public enum eCaseSpin
    {
        none,
        spin_user_load,     // 새로운 패킷 정보 파일을 직접 로딩
        spin_case_custom,   // 원하는 패킷을 직접 수정해서 사용할때
        spin_case_a,
        spin_case_b,
        spin_case_c,
        spin_case_d,
        spin_case_j,
        spin_case_r,
        spin_case_s,
        spin_case_1,
        spin_case_2,
        spin_case_3,
    }
    public eCaseSpin select_spin_packet = eCaseSpin.none;
    eCaseSpin old_spin_packet = eCaseSpin.none;

    public string LoadFileName = "";
    string _spin;
    public string _path = "";
    bool _enable = false;
    public string strSpin { get { return _spin; } set { _spin = value; } }
    public bool IsTestEnable { get { return _enable; } set { _enable = value; } }
    // Use this for initialization
    void Awake()
    {
        I = this;
    }

    void Start()
    {
        string gameName = DEF.GetGamePrefabName(InGame.I.gameId);
        _path = xLIB.xSystem.GetPlatformPath() + "/Assets/GameWorks/" + gameName + "/TestNetFiles/";
    }

    public void LoadFile(string fileFullPath)
    {
        LoadFileName = Path.GetFileName(fileFullPath);
        _spin = xLIB.xSystem.LoadTextFile(fileFullPath);
        _spin.Replace("\t", "");
    }

    void OnGUI()
    {
        if (old_spin_packet != select_spin_packet)
        {
            old_spin_packet = select_spin_packet;
            if (select_spin_packet != eCaseSpin.none && select_spin_packet != eCaseSpin.spin_user_load)
            {
                IsTestEnable = true;
                string strLoadFullPath = _path + select_spin_packet.ToString() + ".txt";
                LoadFile(strLoadFullPath);
            }
            else if (select_spin_packet == eCaseSpin.spin_user_load)
            {
                IsTestEnable = true;
                string path = _path;
                string strLoadFullPath = UnityEditor.EditorUtility.OpenFilePanel("Load Test Protocal txt Files", path, "txt");
                if (strLoadFullPath.Length > 20)
                {
                    LoadFile(strLoadFullPath);
                }
            }
            else
            {
                IsTestEnable = false;
            }
        }
    }

#endif
}
