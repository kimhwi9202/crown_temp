using UnityEngine;
using System.Security.Cryptography;


namespace xLIB
{
    public class xEncrypt : MonoBehaviour
    {
        /*
            public static void Test()
            {
                string userName = "Ronnie Jang";

                MD5 md5Hash = new MD5CryptoServiceProvider();
                byte[] secret = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userName));

                // Game progress ( key, value ) pair.  
                string key = "test_key";
                string _value = "Encrypt_Example";

                // Insert ( key, value ) pair.  
                SetString(key, _value, secret);

                // Retrieve ( key, value ) pair.  
                string ret = GetString(key, secret);

                // Output.  
                Debug.Log("userName: " + userName);
                Debug.Log(key + " : " + ret);
            }
        */
        public static void SetString(string _key, string _value, byte[] _secret)
        {
            // Hide '_key' string.  
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Encrypt '_value' into a byte array  
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(_value);

            // Eecrypt '_value' with 3DES.  
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = _secret;
            des.Mode = CipherMode.ECB;
            ICryptoTransform xform = des.CreateEncryptor();
            byte[] encrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

            // Convert encrypted array into a readable string.  
            string encryptedString = System.Convert.ToBase64String(encrypted);

            // Set the ( key, encrypted value ) pair in regular PlayerPrefs.  
            PlayerPrefs.SetString(hashKey, encryptedString);

            //Debug.Log("SetString hashKey: " + hashKey + " Encrypted Data: " + encryptedString);
        }

        public static string GetString(string _key, byte[] _secret)
        {
            // Hide '_key' string.  
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Retrieve encrypted '_value' and Base64 decode it.  
            string _value = PlayerPrefs.GetString(hashKey);
            if (string.IsNullOrEmpty(_value)) return _value;
            byte[] bytes = System.Convert.FromBase64String(_value);

            // Decrypt '_value' with 3DES.  
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = _secret;
            des.Mode = CipherMode.ECB;
            ICryptoTransform xform = des.CreateDecryptor();
            byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

            // decrypte_value as a proper string.  
            string decryptedString = System.Text.Encoding.UTF8.GetString(decrypted);

            //Debug.Log("GetString hashKey: " + hashKey + " GetData: " + _value + " Decrypted Data: " + decryptedString);

            return decryptedString;
        }

        public static void RemoveString(string _key, byte[] _secret)
        {
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            PlayerPrefs.DeleteKey(hashKey);
        }
    }
}