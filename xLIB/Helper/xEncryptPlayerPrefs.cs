using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace xLIB
{
    public class xEncryptPlayerPrefs
    {
        public static void SetString(string _key, string _value)
        {
            // Hide '_key' string.  
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Encrypt '_value' into a byte array  
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(_value);

            // Eecrypt '_value' with 3DES.  
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = hashData;
            des.Mode = CipherMode.ECB;
            ICryptoTransform xform = des.CreateEncryptor();
            byte[] encrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

            // Convert encrypted array into a readable string.  
            string encryptedString = System.Convert.ToBase64String(encrypted);

            // Set the ( key, encrypted value ) pair in regular PlayerPrefs.  
            PlayerPrefs.SetString(hashKey, encryptedString);
        }

        public static bool Is(string sKey)
        {
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sKey));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Retrieve encrypted '_value' and Base64 decode it.  
            string _value = PlayerPrefs.GetString(hashKey);
            if (_value.Length == 0)
                return false;

            return true;
        }

        public static int GetInt(string _key, int value=1)
        {
            return System.Convert.ToInt32(GetString(_key, value.ToString()));
        }
        public static void SetInt(string _key, int value)
        {
            SetString(_key, value.ToString());
        }
        public static string GetString(string _key, string defaultValue = "")
        {
            // Hide '_key' string.  
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Retrieve encrypted '_value' and Base64 decode it.  
            string _value = PlayerPrefs.GetString(hashKey);
            if (_value.Length == 0)
                return defaultValue;

            byte[] bytes = System.Convert.FromBase64String(_value);

            // Decrypt '_value' with 3DES.  
            TripleDES des = new TripleDESCryptoServiceProvider();
            des.Key = hashData;
            des.Mode = CipherMode.ECB;
            ICryptoTransform xform = des.CreateDecryptor();
            byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

            return System.Text.Encoding.UTF8.GetString(decrypted);
        }

        public static void GetIntList(string _key, List<int> list)
        {
            string sValue = GetString(_key);
            if (sValue.Length == 0)
                return;

            string[] asString = sValue.Split(',');
            for (int i = 0; i < asString.Length; i++)
                list.Add(System.Convert.ToInt32(asString[i]));
        }

        public static void SetIntList(string _key, List<int> list)
        {
            if (list.Count == 0)
                return;

            string sValue = list[0].ToString();
            for (int i = 1; i < list.Count; i++)
                sValue = sValue + "," + list[i].ToString();

            SetString(_key, sValue);
        }
        public static void GetIntArray(string _key, int[] aiValue)
        {
            string sValue = GetString(_key);
            if (sValue.Length == 0)
                return;

            string[] asString = sValue.Split(',');
            if (aiValue.Length != asString.Length)
                return;
            for (int i = 0; i < asString.Length; i++)
                aiValue[i] = System.Convert.ToInt32(asString[i]);
        }

        public static void SetIntArray(string _key, int[] aiValue)
        {
            if (aiValue.Length == 0)
                return;

            string sValue = aiValue[0].ToString();
            for (int i = 1; i < aiValue.Length; i++)
                sValue = sValue + "," + aiValue[i].ToString();

            SetString(_key, sValue);
        }

        public static void DeleteKey(string _key)
        {
            // Hide '_key' string.  
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Retrieve encrypted '_value' and Base64 decode it.  
            PlayerPrefs.DeleteKey(hashKey);
        }

        public static bool HasKey(string _key)
        {
            // Hide '_key' string.  
            MD5 md5Hash = MD5.Create();
            byte[] hashData = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_key));
            string hashKey = System.Text.Encoding.UTF8.GetString(hashData);

            // Retrieve encrypted '_value' and Base64 decode it.  
            return PlayerPrefs.HasKey(hashKey);
        }
    }
}