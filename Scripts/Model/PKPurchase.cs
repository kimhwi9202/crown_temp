﻿using UnityEngine;
using System.Collections;


namespace PK.Purchase
{
    public class SendData
    {
        public string status { get; set; }
        public string signed_request { get; set; }
        public string quantity { get; set; }
        public string currency { get; set; }
        public string purchase_type { get; set; }
        public string product_url { get; set; }
        public string amount { get; set; }
        public long payment_id { get; set; }
    }
    public class SEND
    {
        public string cmd { get; set; }
        public SendData data { get; set; }
        public SEND(string cmd, SendData data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }

    public class SendDataAndroid
    {
        public string status { get; set; }
        public string currency { get; set; }
        public string quantity { get; set; }
        public string product_url { get; set; }
        public string purchase_type { get; set; }
        public double amount { get; set; }
        public string packageName { get; set; }
        public string productId { get; set; }
        public string transactionID { get; set; }
        public double purchaseTime { get; set; }
        public int purchaseState { get; set; }
        public string purchaseToken { get; set; }
    }
    public class SENDAndroid
    {
        public string cmd { get; set; }
        public SendDataAndroid data { get; set; }
        public SENDAndroid(string cmd, SendDataAndroid data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }

    public class SendDataIOS
    {
        public string status { get; set; }
        public string currency { get; set; }
        public string quantity { get; set; }
        public string product_url { get; set; }
        public string purchase_type { get; set; }
        public double amount { get; set; }
        public string purchaseToken { get; set; }
    }
    public class SENDIOS
    {
        public string cmd { get; set; }
        public SendDataIOS data { get; set; }
        public SENDIOS(string cmd, SendDataIOS data)
        {
            this.cmd = cmd;
            this.data = data;
        }
    }

    
    
    /*
    - balance: 구매후 구매한 금액이 더해진 유저의 balance
    - coins: 구매한 금액
    - status: 구매 성공/실패/보류 여부
    - old_balance: 구매전 유저의 balance
    */

    public class REData
    {
        public long coins { get; set; }
        public long old_balance { get; set; }
        public long balance { get; set; }
        public string status { get; set; }
    }

    public class RECEIVE : PacketData
    {
        public REData data { get; set; }
    }

}