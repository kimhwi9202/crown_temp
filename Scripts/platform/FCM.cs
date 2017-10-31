using UnityEngine;
using System.Collections;

public class FCM : MonoBehaviour {

    private string ReceiveToken;
    public void Initialize()
    {
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        ReceiveToken = token.Token;
        //UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.RawData);
        
        //UI.I.ShowMsgBox(e.Message.Notification.Body);
    }

}
