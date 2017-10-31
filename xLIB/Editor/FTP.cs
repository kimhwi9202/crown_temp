#if UNITY_5

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using xLIB;

using UnityEditor;

/*
*            FTP.SetConnectInfo("ftp://artnas.iptime.org:15100/bluegames/test/", "hyun");
*            FTP.SetLocalPathInfo(@"D:\FTPTrialLocalPath\");
*/

namespace xLIB.Editor
{
    public class FTP : MonoBehaviour
    {
        private static byte[] _secret;
        public static string ftpAddress;
        public static string Id;
        public static string Password;
        public struct FileStruct
        {
            public string Flags;
            public string Owner;
            public string Group;
            public bool IsDirectory;
            public DateTime CreateTime;
            public string time;
            public long Size;
            public string Name;
        }
        public enum FileListStyle
        {
            UnixStyle,
            WindowsStyle,
            Unknown
        }
        public struct STFTPFiles
        {
            public string Name;
            public List<object> _dirlist;
            public List<object> _attlist;
            public List<object> _datelist;
            public List<long> _szlist;
        }

        private static IEnumerator OnFtpProgressBar(string dir, string filename, int current, int totalAmount)
        {
            while (current < totalAmount)
            {
                EditorUtility.DisplayProgressBar(dir, filename, current / (float)totalAmount);
                current += 2048;
                yield return null;
            }

            EditorUtility.ClearProgressBar();
        }


        public static void SaveLoinInfo()
        {
            if (_secret == null)
            {
                MD5 md5Hash = new MD5CryptoServiceProvider();
                _secret = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Application.productName));
            }

            xEncrypt.SetString("EditorFtpServer", ftpAddress , _secret);
            xEncrypt.SetString("EditorFtpId", Id, _secret);
            xEncrypt.SetString("EditorFtpPass", Password, _secret);
        }
        public static void LoadLoinInfo()
        {
            if (_secret == null)
            {
                MD5 md5Hash = new MD5CryptoServiceProvider();
                _secret = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Application.productName));
            }

            ftpAddress = xEncrypt.GetString("EditorFtpServer", _secret);
            Id = xEncrypt.GetString("EditorFtpId", _secret);
            Password = xEncrypt.GetString("EditorFtpPass", _secret);
        }
        public static void DeleteLoinInfo()
        {
            xEncrypt.RemoveString("EditorFtpServer", _secret);
            xEncrypt.RemoveString("EditorFtpId", _secret);
            xEncrypt.RemoveString("EditorFtpPass", _secret);
            _secret = null;

            //Debug.Log("PlayerPrefs Remove -> EditorSaveLogin" + Application.productName);
        }

        public static string GetAddress()
        {
            return "ftp://" + ftpAddress + "/";
        }

        private static void SetNetworkCreadentials(FtpWebRequest request)
        {
            request.Credentials = new NetworkCredential(Id, Password);
        }

        public static bool UploadtoLocalDirectory(string _localPath, string _ftpDirectory="")
        {
            string[] files = Directory.GetFiles(_localPath);

            int max_count = files.Length;
            for (int i = 0; i < max_count; i++)
            {
                string filepath = files[i];
                string fileName = Path.GetFileName(filepath);

                try
                {
                    FileUploadtoFTP(_localPath, fileName, _ftpDirectory);
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception.Message.ToString());
                    return false;
                }
            }

            EditorUtility.ClearProgressBar();
            return true;
        }


        public static void FileDownloadFromFTP(string _localPath, string fileName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetAddress() + fileName);
            SetNetworkCreadentials(request);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            FtpWebResponse responseFileDownload = (FtpWebResponse)request.GetResponse();

            Stream responseStream = responseFileDownload.GetResponseStream();
            FileStream writeStream = new FileStream(_localPath + fileName, FileMode.Create);

            int Length = 2048;
            Byte[] buffer = new Byte[Length];
            int bytesRead = responseStream.Read(buffer, 0, Length);

            IEnumerator progressShow = OnFtpProgressBar(_localPath, fileName, 0, (int)Length);

            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = responseStream.Read(buffer, 0, Length);
                progressShow.MoveNext();
            }

            responseStream.Close();
            writeStream.Close();

            request = null;
            responseFileDownload = null;
        }

        public static void FileUploadtoFTP(string _localPath, string fileName, string _ftpDirectory)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetAddress() + _ftpDirectory + fileName);
            SetNetworkCreadentials(request);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            FileInfo fileInfo = new FileInfo(_localPath + fileName);
            FileStream fileStream = fileInfo.OpenRead();

            int bufferLength = 2048;
            byte[] buffer = new byte[bufferLength];

            Stream uploadStream = request.GetRequestStream();
            int contentLength = fileStream.Read(buffer, 0, bufferLength);

            IEnumerator progressShow = OnFtpProgressBar(fileInfo.DirectoryName, fileInfo.Name, 0, (int)fileInfo.Length);
            while (contentLength != 0)
            {
                uploadStream.Write(buffer, 0, contentLength);
                contentLength = fileStream.Read(buffer, 0, bufferLength);
                progressShow.MoveNext();
            }

            uploadStream.Close();
            fileStream.Close();
            request = null;
        }

        public static void FileDeleteFromFTP(string fileName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetAddress() + fileName);
            SetNetworkCreadentials(request);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            //FtpWebResponse responseFileDelete = (FtpWebResponse)request.GetResponse();
        }

        public static void MakeDirectoryFromFTP(string folderName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetAddress() + folderName);
            SetNetworkCreadentials(request);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            //FtpWebResponse responseFileDelete = (FtpWebResponse)request.GetResponse();
        }

        public static void FileListFormLocalDirectory(string _localPath)
        {
            string[] files = Directory.GetFiles(_localPath);

            foreach (string filepath in files)
            {
                string fileName = Path.GetFileName(filepath);
                Debug.Log(fileName);
            }
        }

        public static bool FindFileFormFTPDirectory(string _fileName, string _directory="")
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetAddress() + _directory);
            SetNetworkCreadentials(request);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            StreamReader _sr = new StreamReader(request.GetResponse().GetResponseStream());
            bool result = false;
            while (!_sr.EndOfStream)
            {
                string temp = _sr.ReadLine();
                //Debug.Log(temp);

                int s = temp.IndexOf(_fileName);
                if( s >= 0 )
                {
                    if((temp.Substring(s, _fileName.Length)).Equals(_fileName))
                    {
                        result = true;
                        break;
                    }
                }
            }

            request = null;
            _sr = null;

            return result;
        }

        public static List<STFTPFiles> FileListFormFTPDirectory(string DirectoryPath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(GetAddress()+ DirectoryPath);
            SetNetworkCreadentials(request);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream());

            string fileName = streamReader.ReadLine();

            List<STFTPFiles> files = new List<STFTPFiles>();

            while (fileName != null)
            {
                STFTPFiles data = new STFTPFiles();
                data.Name = fileName;
                files.Add(data);
                fileName = streamReader.ReadLine();
            }

            request = null;
            streamReader = null;
            return files;
        }
        public static List<STFTPFiles> FileListFormFTPDirectoryDetails(string DirectoryPath)
        {
            FtpWebRequest _fwr = (FtpWebRequest)WebRequest.Create(GetAddress() + DirectoryPath) as FtpWebRequest;
            SetNetworkCreadentials(_fwr);
            _fwr.UseBinary = true;
            _fwr.UsePassive = true;
            _fwr.KeepAlive = true;
            _fwr.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            StreamReader _sr = new StreamReader(_fwr.GetResponse().GetResponseStream(), System.Text.Encoding.UTF8);

            List<STFTPFiles> files = new List<STFTPFiles>();
            STFTPFiles data = new STFTPFiles();

            data._dirlist = new List<object>();
            data._attlist = new List<object>();
            data._datelist = new List<object>();
            data._szlist = new List<long>();

            while (!_sr.EndOfStream)
            {
                Debug.Log(_sr.ReadLine());
                string[] buf = _sr.ReadLine().Split(' ');
                //string Att, Dir;
                int numcnt = 0, offset = 4; 
                long sz = 0;
                for (int i = 0; i < buf.Length; i++)
                {
                    //Count the number value markers, first before the ftp markers and second
                    //the file size.
                    if (long.TryParse(buf[i], out sz)) numcnt++;
                    if (numcnt == 2)
                    {
                        //Get the attribute
                        string cbuf = "", dbuf = "", abuf = "";
                        if (buf[0][0] == 'd') abuf = "Dir"; else abuf = "File";
                        //Get the Date
                        if (!buf[i + 3].Contains(":")) offset++;
                        for (int j = i + 1; j < i + offset; j++)
                        {
                            dbuf += buf[j];
                            if (j < buf.Length - 1) dbuf += " ";
                        }
                        //Get the File/Dir name
                        for (int j = i + offset; j < buf.Length; j++)
                        {
                            cbuf += buf[j];
                            if (j < buf.Length - 1) cbuf += " ";
                        }
                        //Store to a list.
                        data._dirlist.Add(cbuf);
                        data._attlist.Add(abuf);
                        data._datelist.Add(dbuf);
                        data._szlist.Add(sz);

                        offset = 0;
                        break;
                    }
                }
            }

            files.Add(data);

            return files;
        }

        public static FileStruct[] FileListFormFTPDirectoryDetails2(string DirectoryPath)
        {
            FtpWebRequest _fwr = (FtpWebRequest)WebRequest.Create(GetAddress() + DirectoryPath) as FtpWebRequest;
            SetNetworkCreadentials(_fwr);
            _fwr.UseBinary = true;
            _fwr.UsePassive = true;
            _fwr.KeepAlive = true;
            _fwr.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            StreamReader _sr = new StreamReader(_fwr.GetResponse().GetResponseStream(), System.Text.Encoding.UTF8);
            return GetList(_sr.ReadToEnd());
        }
        static private FileStruct[] GetList(string datastring)
        {
            List<FileStruct> myListArray = new List<FileStruct>();
            string[] dataRecords = datastring.Split('\n');
            FileListStyle _directoryListStyle = GuessFileListStyle(dataRecords);
            foreach (string s in dataRecords)
            {
                if (_directoryListStyle != FileListStyle.Unknown && s != "")
                {
                    FileStruct f = new FileStruct();
                    f.Name = "..";
                    switch (_directoryListStyle)
                    {
                        case FileListStyle.UnixStyle:
                            f = ParseFileStructFromUnixStyleRecord(s);
                            break;
                        case FileListStyle.WindowsStyle:
                            f = ParseFileStructFromWindowsStyleRecord(s);
                            break;
                    }
                    if (!(f.Name == "." || f.Name == ".."))
                    {
                        myListArray.Add(f);
                    }
                }
            }
            return myListArray.ToArray(); ;
        }

        static private FileStruct ParseFileStructFromWindowsStyleRecord(string Record)
        {
            ///Assuming the record style as 
            /// 02-03-04  07:46PM       <DIR>          Append
            FileStruct f = new FileStruct();
            string processstr = Record.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            f.CreateTime = DateTime.Parse(dateStr + " " + timeStr);
            if (processstr.Substring(0, 5) == "<DIR>")
            {
                f.IsDirectory = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
            }
            else
            {
                string[] strs = processstr.Split(new char[]{ ' ' });
                processstr = strs[1].Trim();
                f.IsDirectory = false;
            }
            f.Name = processstr;  //Rest is name   
            return f;
        }

        static public FileListStyle GuessFileListStyle(string[] recordList)
        {
            foreach (string s in recordList)
            {
                if (s.Length > 10
                 && Regex.IsMatch(s.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FileListStyle.UnixStyle;
                }
                else if (s.Length > 8
                 && Regex.IsMatch(s.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FileListStyle.WindowsStyle;
                }
            }
            return FileListStyle.Unknown;
        }

        static private FileStruct ParseFileStructFromUnixStyleRecord(string Record)
        {
            ///Assuming record style as
            /// dr-xr-xr-x   1 owner    group               0 Nov 25  2002 bussys
            FileStruct f = new FileStruct();
            string processstr = Record.Trim();
            f.Flags = processstr.Substring(0, 9);
            f.IsDirectory = (f.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //skip one part
            f.Owner = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            f.Group = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
            //_cutSubstringFromStringWithTrim(ref processstr, ' ', 0);   //skip one part
            f.Size = Convert.ToInt64(_cutSubstringFromStringWithTrim(ref processstr, ' ', 0));
            //f.CreateTime = DateTime.Parse(_cutSubstringFromStringWithTrim(ref processstr, ' ', 8));
            f.time = _cutSubstringFromStringWithTrim(ref processstr, ' ', 8);
            //DateTime dateTime = DateTime.ParseExact(_cutSubstringFromStringWithTrim(ref processstr, ' ', 8), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            f.Name = processstr;   //Rest of the part is name
            return f;
        }

        static private string _cutSubstringFromStringWithTrim(ref string s, char c, int startIndex)
        {
            int pos1 = s.IndexOf(c, startIndex);
            string retString = s.Substring(0, pos1);
            s = (s.Substring(pos1)).Trim();
            return retString;
        }
    }
}

#endif