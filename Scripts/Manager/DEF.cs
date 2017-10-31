using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 글로벌 Define 선언은 여기서 다 정의해서 사용
/// </summary>
public enum eLoginType
{
    none,
    facebook,
    guest
}

public enum eSaleType
{
    normal, x2, x3, flash,
}

public enum eGameList
{
    none = 0,

    //HighDiamonds = 35,
    //emeraldSevens = 37,
    //HotLotto = 26,
    //flyingPiggy = 38,
    //DoubleLuck = 46,
    //DoubleJackpot7s = 40,

    ToyFactory = 1,
    //ClassicSlots = 2,
    //WildWestBullets = 3,
    TheWizardofOz = 4,
    //ElephantsDiamond = 5,
    TripleFortune = 6,
    Cleopatra = 7,
    GottabeKitty = 8,
    LuckyOcean = 9,
    GoldDigger = 10,
    PirateCannons = 11,
    MysticUnicon = 12,
    JackpotWheel = 13,
    MoneyBlaster = 14,
    ArcticLegend = 15,
    FireSeven = 16,
    FortuneDynasty = 17,
    AllStarClassic = 18,
    WheelsOfWonder = 19,
    VagasDiamonds = 20,
    ClassicSlots = 21,
    ZeusAndHera = 22,
    ElephantsDiamond = 23,
    MegaStar = 24,
    PiggyRich = 25,
    HotLotto = 26, // open
    fairyGarden = 27,
    GoldenPharaoh = 28,
    LuckyBell = 29,
    CrackTheVault = 30,
    ShiningSevens = 31,
    DoubleDragon = 32,
    HotSevensDeluxe = 33,
    WildWestBullets = 34,
    HighDiamonds = 35,  // open
    TrickorTreat = 36,
    emeraldSevens = 37,  // open
    flyingPiggy = 38,  // open
    classicSlotDice = 39,
    DoubleJackpot7s = 40, // open
    snowWhite = 41,
    yinAndYang = 42,
    MoneyBlaster2 = 43,
    Aztec = 44,
    Irish = 45,
    DoubleLuck = 46,    // open
    BlastingWheel = 47,

    MysteryGemstone = 48,
    Monkeyking = 49,
    JewelRiches = 50,
    DoubleLuck_High = 51,
    DiceOfFortune = 52,
    GoldenPharaoh_High = 53,
    FishingBonanza = 54,
    yinAndYang_High = 55,
    _3Reel = 56,
    High = 57,
    Dancing = 58,
    max,
}

/// <summary>
/// 현재화면 정보
/// </summary>
public enum eView
{
    none = 0,
    Login,
    Lobby,
    Game,
}

public enum eDealKind
{
    none,
    first,
    exclusive_1,
    exclusive_2,
    exclusive_3,
    exclusive_4,
    exclusive_5,
    exclusive_vip,
}


public class DEF
{
    public static int GameDownloadingCount = 0; // 게임 다운로드 참조 카운터 

    public enum eSortingLayer
    {
        Default, effect, 
    }

    /// <summary>
    /// 모든 인게임 승리타입 정의 
    /// </summary>
    public enum eSlotWinType
    {
        none,
        normal,
        bigWin,
        megaWin,
        jackpot,
    }

    /// <summary>
    /// 스텍틱 변수형
    /// </summary>

    // Facebook FeeShare Url
    // 인덱스 붙은거는 랜덤처리
    public class SHARE
    {
        public const string BaseShareURL = "https://d3kjdk8bsa0don.cloudfront.net/images/";
        public const string Jackpot = BaseShareURL + "share/jmb_j_01.png";
        public const string MegaWin_1 = BaseShareURL + "share/share_mega_01.png";
        public const string MegaWin_2 = BaseShareURL + "share/share_mega_02.png";
        public const string MegaWin_3 = BaseShareURL + "share/share_mega_03.png";
        public const string BigWin_1 = BaseShareURL + "share/share_big_01.png";
        public const string BigWin_2 = BaseShareURL + "share/share_big_02.png";
        public const string BigWin_3 = BaseShareURL + "share/share_big_03.png";
        public const string DailySpin_1 = BaseShareURL + "share/wheel_share_1.png";
        public const string DailySpin_2 = BaseShareURL + "share/wheel_share_2.png";
        public const string DailySpin_3 = BaseShareURL + "share/wheel_share_3.png";
        public const string DailySpin_4 = BaseShareURL + "share/wheel_share_4.png";
        public const string DailySpin_5 = BaseShareURL + "share/wheel_share_5.png";
        public const string TournamentWin_1 = BaseShareURL + "share/tournament_win_1.png";
        public const string TournamentWin_2 = BaseShareURL + "share/tournament_win_2.png";
        public const string TournamentWin_3 = BaseShareURL + "share/tournament_win_3.png";
        public const string TournamentWin_4 = BaseShareURL + "share/tournament_win_4.png";
        public const string TournamentWin_5 = BaseShareURL + "share/tournament_win_5.png";

        static public string GetDailySpinURL()
        {
            int index = Random.Range(0, 6);
            return BaseShareURL + "share/wheel_share_" + index.ToString() + ".png";
        }
        static public string GetTournamentWinURL()
        {
            int index = Random.Range(0, 6);
            return BaseShareURL + "share/tournament_win_" + index.ToString() + ".png";
        }
        static public string GetWinBigURL()
        {
            int index = Random.Range(0, 4);
            return BaseShareURL + "share/share_big_0" + index.ToString() + ".png";
        }
        static public string GetWinMegaURL()
        {
            int index = Random.Range(0, 4);
            return BaseShareURL + "share/share_mega_0" + index.ToString() + ".png";
        }
        static public string GetWinJackpotURL()
        {
            return BaseShareURL + "share/jmb_j_01.png";
        }
        static public string GetWinBonusURL(string code, eSlotWinType type)
        {
            string url = "";
            //https://apps.facebook.com/slotica_slots_test/?share_id=C7JSK0X7rm&fb_ref=share_jackpot_win

            if (xLIB.CONFIG.CurrentNetworkMode == xLIB.eNetworkMode.RealNetwork)
            {
                url = "https://apps.facebook.com/slotica_slots/?share_id=" + code + "&fb_ref=";
            }
            else
            {
                url = "https://apps.facebook.com/slotica_slots_test/?share_id=" + code + "&fb_ref=";
            }

            switch (type)
            {
                case eSlotWinType.bigWin: url += "share_big_win"; break;
                case eSlotWinType.megaWin: url += "share_mega_win"; break;
                case eSlotWinType.jackpot: url += "share_jackpot_win"; break;
            }
            return url;
        }
    }


#region 슬롯 게임오브젝트 이름
    public static readonly string SYMBOL_POOL = "_SymbolPool";
    public static readonly string SYMBOL_POOL_EXTRA = "_SymbolPoolExtra";
    public static readonly string GAMEOBJECT_LINE = "Line";
    public static readonly string GAMEOBJECT_PAYLINE = "PayLine";
#endregion  // 슬롯 게임오브젝트 이름

#region 게임정보로 에셋번들 이름 얻기

    public static eGameList FIRST_INSTALL_GAME = eGameList.DoubleLuck;
    /// <summary>
    /// 설치가 가능한 게임 리스트 
    /// </summary>
    static public bool IsUseGameID(eGameList id)
    {
        PK.GamesInfo.REData data = USER.I.GetGameListInfo((int)id);
        if (data != null) return true;
        return false;
        /*
        switch (id)
        {
            case eGameList.emeraldSevens:
            case eGameList.HighDiamonds:
            case eGameList.HotLotto:
            case eGameList.flyingPiggy:
            case eGameList.DoubleLuck:
            case eGameList.DoubleJackpot7s:
                return true;
        }
        return false;
        */
    }

    static public string GetLobbyBundleName()
    {
        string extends = "";
        if (xLIB.CONFIG.IsRunningAndroid())
        {
            extends = ".ad";
        }
        else if (xLIB.CONFIG.IsRunningiOS())
        {
            extends = ".ios";
        }
        return "lobby" + extends;
    }
    static public string GetGameCommonBundleName()
    {
        string extends = "";
        if (xLIB.CONFIG.IsRunningAndroid())
        {
            extends = ".ad";
        }
        else if (xLIB.CONFIG.IsRunningiOS())
        {
            extends = ".ios";
        }
        return "gamecommon" + extends;
    }
    static public string GetFirstInstallGameBundleName()
    {
        string extends = "";
        if (xLIB.CONFIG.IsRunningAndroid())
        {
            extends = ".ad";
        }
        else if (xLIB.CONFIG.IsRunningiOS())
        {
            extends = ".ios";
        }
        return "lobby" + extends;
    }


    static public Sprite GetGameLoadingImage(eGameList name)
    {
        int id = (int)name;
        return xLIB.BUNDLE.I.LoadAsset<Sprite>("mobile_loading_" + id.ToString());
    }
    static public string GetGameBundleName(eGameList name)
    {
        int id = (int)name;
        string file = name.ToString();
        string bundleName = file.ToLower();
        string extends = "";
        if (xLIB.CONFIG.IsRunningAndroid())
        {
            extends = ".ad";
        }
        else if(xLIB.CONFIG.IsRunningiOS())
        {
            extends = ".ios";
        }
        return id.ToString() + "_" + bundleName + extends;
    }
    static public string GetGamePrefabName(eGameList name)
    {
        int id = (int)name;
        string file = name.ToString();
        string bundleName = file.ToLower();
        return id.ToString() + "_" + bundleName;
    }
#endregion  게임정보로 에셋번들 이름 얻기


    /// <summary>
    /// 로비 사운드 
    /// </summary>
    public class SND
    {
        public const string lobby_bgm = "lobby_bgm";
        public const string common_click = "common_click";
        public const string balance_up = "balance_up";
        public const string balance_flow = "balance_flow";
        public const string dailyspin_count_end = "dailyspin_count_end";
        public const string dailyspin_counting = "dailyspin_counting";
        public const string dailyspin_wheel = "dailyspin_wheel";
        public const string dailyspin_win = "dailyspin_win";
        public const string game_click = "game_click";
        public const string popup_open = "popup_open";
        public const string purchase_popup = "purchase_popup";
        //public const string win_cheers = "tf_cheers";
        SND(string name)
        {
            SOUND.I.Play(name);
        }
    }

    // IAP Purchase 패킷 공통 클래스
    public class IAPData
    {
        public int id { get; set; }
        public string tag { get; set; }
        public int sale_percentage { get; set; }
        public long regular_coins { get; set; }
        public int free_percentage { get; set; }
        public long coins { get; set; }
        public double price { get; set; }
        public string product_url { get; set; }
        public long level_up_bonus { get; set; }
        public string level_bonus_percentage { get; set; }
        public long regular_price { get; set; }
        public long code { get; set; }
        public IAPData(PK.GetPurchaseItems.REDataPacks data)
        {
            this.id = data.id;
            this.tag = data.tag;
            this.sale_percentage = data.sale_percentage;
            this.regular_coins = data.regular_coins;
            this.free_percentage = data.free_percentage;
            this.coins = data.coins;
            this.price = data.price;
            this.product_url = data.product_url;
            this.level_up_bonus = data.level_up_bonus;
            this.level_bonus_percentage = data.level_bonus_percentage;
            this.regular_price = data.regular_price;
            this.code = data.code;
        }
        public IAPData(PK.GetDailyWheelShop.REDataPacks data)
        {
            this.id = data.id;
            this.tag = data.tag;
            this.sale_percentage = data.sale_percentage;
            this.regular_coins = data.regular_coins;
            this.free_percentage = data.free_percentage;
            this.coins = data.coins;
            this.price = data.price;
            this.product_url = data.product_url;
            this.level_up_bonus = data.level_up_bonus;
            this.level_bonus_percentage = data.level_bonus_percentage;
            this.regular_price = data.regular_price;
            this.code = data.code;
        }
        //public IAPData(PK.GetVaultShop.REDataPacks data)
        //{
        //    this.id = data.id;
        //    this.tag = data.tag;
        //    this.sale_percentage = data.sale_percentage;
        //    this.regular_coins = data.regular_coins;
        //    this.free_percentage = data.free_percentage;
        //    this.coins = data.coins;
        //    this.price = data.price;
        //    this.product_url = data.product_url;
        //    this.level_up_bonus = data.level_up_bonus;
        //    this.level_bonus_percentage = data.level_bonus_percentage;
        //    this.regular_price = data.regular_price;
        //    this.code = data.code;
        //}
    }
}
