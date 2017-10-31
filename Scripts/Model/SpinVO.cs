using UnityEngine;
using System.Collections;


//namespace GPK.Spin
//{
    public class ReqSpinItem
    {
        public int lineBet { get; set; }
        public ReqSpinItem(int lineBet)
        {
            this.lineBet = lineBet;
        }
    }
    public class CmdSpin
    {
        public ReqSpinItem data { get; set; }
        public string cmd { get; set; }
        public CmdSpin(string cmd, ReqSpinItem spinItem)
        {
            this.cmd = cmd;
            this.data = spinItem;
        }
    }
    public class CmdFreeSpin
    {
        public ReqSpinItem data { get; set; }
        public string cmd { get; set; }
        public string freeSpinKey { get; set; }

        public CmdFreeSpin(string cmd, string freeSpinKey, ReqSpinItem spinItem)
        {
            this.cmd = cmd;
            this.freeSpinKey = freeSpinKey;
            this.data = spinItem;
        }
    }



    public class PayLinesItem
    {
        public int line { get; set; }
        public long payout { get; set; }
        public bool isJackpot { get; set; }
        public int wintable { get; set; }
        public int matches { get; set; }
    }

    public class SpinsItem
    {
        public bool isNormal { get; set; }
        public long totalPayout { get; set; }
        public int freeSpinCount { get; set; }
        public int[] dices { get; set; }
        public string[] reel { get; set; }
        public string[] extraReel { get; set; }
        public string[] after_reel_window { get; set; } // null or array-object
        public PayLinesItem[] payLines { get; set; }
        public int[] scatterArray { get; set; }

        public bool isBonusSpin { get; set; }
        public int[] fixedreel { get; set; }
    }
    public class PayoutsItem
    {
        public bool isBigWin { get; set; }
        public bool isMegaWin { get; set; }
        public bool isJackpot { get; set; }
        public int lineBet { get; set; }
        public int multipleWin { get; set; }
        public int accumulateSum { get; set; }
        public long totalPayout { get; set; }

        public SpinsItem[] spins { get; set; }
    }
    public class SpinData
    {
        public long balance { get; set; }
        public int level { get; set; }
        public int levelPercent { get; set; }
        public int gameLevel { get; set; }
        public int gameLevelPercent { get; set; }
        public long jackpotPool { get; set; }
        public long level_up_bonus { get; set; }
        public int level_up_spins { get; set; }
        public int winID { get; set; }
        public PayoutsItem payouts { get; set; }
        public string freeSpinKey { get; set; }

        public long[] subjackpotPool { get; set; }
    }
    public class SpinVO : PacketData
    {
        public SpinData data { get; set; }
    }
//}