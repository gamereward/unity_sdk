using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grd
{
    internal delegate void GrdNetworkEventHandler(string data);
    public enum GrdNet
    {
        Main,Test
    }
    /// <summary>
    /// Class GameReward client constains all method use to connect to gamereward server
    /// </summary>
    public class GrdManager
    {
        /// <summary>
        /// Init all the parameter for connect to reward
        /// </summary>
        /// <param name="appId">The application id</param>
        /// <param name="secret">The secret of the application</param>
        /// <param name="net">The net of the application: TestNet use for testing your game. MainNet use when public game to users.</param>
        public static void Init(string appId,string secret,GrdNet net)
        {
            if (handler == null)
            {
                GameObject g = new GameObject();
                g.name = "GrdManager";
                handler = g.AddComponent<GrdHandler>();
            }
            handler.Init(appId, secret, net==GrdNet.Main? apiMainUrl: apiTestUrl);
        }
        private const string apiTestUrl = "https://test.gamereward.io/appapi/";
        private const string apiMainUrl = "https://gamereward.io/appapi/";
        private static GrdHandler handler;
        /// <summary>
        /// User information
        /// </summary>
        public static GrdUser User
        {
            get
            {
                return user;
            }
        }
        private static GrdUser user;
        /// <summary>
        /// Get the unix timestamp at the momen.
        /// </summary>
        /// <returns></returns>
        public static long GetEpochTime()
        {

            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            return  (long)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        }
        /// <summary>
        /// Md5 the string
        /// </summary>
        /// <param name="strToEncrypt">The string to md5</param>
        /// <returns>The string had md5</returns>
        public static string Md5Sum(string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);

            // encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }
        private static void GetImageSize(byte[] imageData, out int width, out int height)
        {
            width = ReadInt(imageData, 3 + 15);
            height = ReadInt(imageData, 3 + 15 + 2 + 2);
        }
        private static int ReadInt(byte[] imageData, int offset)
        {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }
        private static Dictionary<string, object> GetObjectData(string data)
        {
            Dictionary<string, object> result = null;
            try
            {
                result = (Dictionary<string, object>)MiniJSON.Json.Deserialize(data);
            }
            catch
            {
            }
            if (result == null)
            {
                result = new Dictionary<string, object>();
                result.Add("error", 100);
                result.Add("message", data);
            }
            return result;
        }
        /// <summary>
        /// LogOut user from system.
        /// </summary>
        /// <param name="callback"></param>
        public static void LogOut(GrdEventHandler callback)
        {
            handler.Post((data) =>
            {
                Dictionary<string, object> result = GetObjectData(data);
                handler.Logout();
                user = null;
                if (callback != null)
                {
                    int error=int.Parse(result["error"].ToString());
                    GrdEventArgs args = new GrdEventArgs(error,data, result["message"].ToString());
                    callback(error, args);
                }
            }, "logout", null);
        }
        /// <summary>
        /// This method use to reset password for user. System will send an email to change password.
        /// </summary>
        /// <param name="email">Email of user</param>
        /// <param name="callback">Call when completed.</param>
        public static void RequestResetPassword(string email, GrdEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("email", email);
            handler.Post((data) =>
            {
                Dictionary<string, object> result = GetObjectData((string)data);
                if (callback != null)
                {
                    int error = int.Parse(result["error"].ToString());
                    GrdEventArgs args = new GrdEventArgs(error, data, result["message"].ToString());
                    callback(error, args);
                }
            }, "requestresetpassword", pars);
        } 
        /// <summary>
          /// This method use to reset password for user.
          /// </summary>
          /// <param name="token">Token use to reset. Get this token from email.</param>
          /// <param name="password">New password</param>
          /// <param name="callback">Call when completed.</param>
        public static void ResetPassword(string token,string password, GrdEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("token", token);
            pars.Add("password", Md5Sum(password));
            handler.Post((data) =>
            {
                Dictionary<string, object> result = GetObjectData((string)data);
                if (callback != null)
                {
                    int error = int.Parse(result["error"].ToString());
                    GrdEventArgs args = new GrdEventArgs(error, data, result["message"].ToString());
                    callback(error, args);
                }
            }, "doresetpassword", pars);
        }
        /// <summary>
        /// This function use to register a new user
        /// </summary>
        /// <param name="username">Username use to login</param>
        /// <param name="password">Password of the account</param>
        /// <param name="email">Email use for reseting password or receiving the message from app.</param>
        /// <param name="userdata">Any data in string</param>
        /// <param name="callback">Call when finish.</param>
        public static void Register(string username, string password, string email, string userdata, GrdEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("username", username);
            pars.Add("password", Md5Sum(password));
            pars.Add("email", email);
            pars.Add("userdata", userdata);
            handler.Post((data) =>
            {
                Dictionary<string, object> result = GetObjectData((string)data);
                if (callback != null)
                {
                    int error = int.Parse(result["error"].ToString());
                    GrdEventArgs args = new GrdEventArgs(error, data, result["message"].ToString());
                    callback(error, args);
                }
            }, "createaccount", pars);
        }

        /// <summary>
        /// This method use for user login to system. Server response the result is success or failed.
        /// </summary>
        /// <param name="username">Username user use to login</param>
        /// <param name="password">Password user use to login</param>
        /// <param name="callback">Call when completed.</param>
        public static void Login(string username, string password, string otp, GrdEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("username", username);
            pars.Add("password", Md5Sum(password));
            pars.Add("otp", otp);
            handler.Post((data) =>
            {
                GrdEventArgs args;
                Dictionary<string, object> result = GetObjectData((string)data);
                int error = int.Parse(result["error"].ToString());
                if (error==0)
                {
                    user = new GrdUser();
                    user.username = username;
                    user.address = result["address"].ToString();
                    user.email = result["email"].ToString();
                    user.balance = decimal.Parse(result["balance"].ToString());
                    user.otp = result["otpoptions"].ToString() != "0";
                    string token = result["token"].ToString();
                    handler.Login(token);
                    args = new GrdEventArgs(error, data, "Login successfully");
                }
                else
                {
                    args = new GrdEventArgs(error,data, result["message"].ToString());
                }
                if (callback != null)
                {
                    callback(error, args);
                }
            }, "login", pars);
        }
        /// <summary>
        /// Get the qrcode gamereward wallet address
        /// </summary>
        /// <param name="address">Address of the wallet to encode</param>
        /// <param name="callback">Call when server response the QR code</param>
        public static void GetAddressQRCode(string address, GrdTextureEventHandler callback)
        {
            GetQRCode("gamereward:" + address, callback);
        }
        /// <summary>
        /// Get the qrcode from text
        /// </summary>
        /// <param name="text">The text to encode to QR code</param>
        /// <param name="callback">Call when server response QR code.</param>
        public static void GetQRCode(string text, GrdTextureEventHandler callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("text", text);
            dic.Add("type", "1");
            handler.Post((data) =>
            {
                if (callback != null)
                {

                    Dictionary<string, object> result = GetObjectData((string)data);
                    Texture2D texture = null;
                    int error = int.Parse(result["error"].ToString());
                    string message =text;
                    if (error==0)
                    {
                        string qrcode = result["qrcode"].ToString();
                        if (qrcode.Length > 0)
                        {
                            qrcode = qrcode.Substring("data:image/image/png;base64,".Length);
                        }
                        try
                        {
                            texture = GetTexture(qrcode);
                        }
                        catch
                        {
                            error = 1;
                        }
                    }
                    else
                    {
                        text = result["message"].ToString();
                    }
                    GrdTextureEventArgs args = new GrdTextureEventArgs(error, data, text, texture);
                    callback(error, args);
                }
            }, "qrcode", dic);
        }
        /// <summary>
        /// Attract the texture from text response from server
        /// </summary>
        /// <param name="responseText">The Base 64 text return from server</param>
        /// <returns></returns>
        private static Texture2D GetTexture(string responseText)
        {

            Texture2D texture = null;
            byte[] array = System.Convert.FromBase64String(responseText);
            int width, height;
            GetImageSize(array, out width, out height);
            texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(array);
            return texture;
        }
        /// <summary>
        /// Call the server script to do logic game on server
        /// </summary>
        /// <param name="scriptName">The name of the script defined on server</param>
        /// <param name="functionName">The name of function you want to call. If the script have return statement in global scope, the functionName can be empty</param>
        /// <param name="parameters">The parameters to pass to the function</param>
        /// <param name="callback">Call when server response result.</param>
        public static void CallServerScript(string scriptName, string functionName, object[] parameters, GrdCustomEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            string values = MiniJSON.Json.Serialize(parameters);
            pars.Add("vars", values);
            pars.Add("fn", functionName);
            pars.Add("script", scriptName);
            handler.Post((data) =>
            {
                GrdCustomEventArgs args;
                Dictionary<string, object> result = GetObjectData((string)data);
                int error = int.Parse(result["error"].ToString());
                if (error != 0)
                {
                    args = new GrdCustomEventArgs(error,data, result["message"].ToString(), null);
                }
                else
                {
                    args = new GrdCustomEventArgs(0,data, "", result["result"]);
                }
                if (callback != null)
                {
                    callback(error, args);
                }
            }, "callserverscript", pars);
        }
        /// <summary>
        /// Format the number as string that server accept
        /// </summary>
        /// <param name="number">The number</param>
        /// <returns></returns>
        private static string FormatNumber(decimal number)
        {
            char[] array = number.ToString().ToCharArray();
            bool isDecimal = false;
            for (int i = array.Length - 1; i > 0; i--)
            {
                if (!isDecimal)
                {
                    if (!char.IsDigit(array[i]))
                    {
                        array[i] = '.';
                        isDecimal = true;
                    }
                }
                else
                {
                    if (!char.IsDigit(array[i]))
                    {
                        array[i] = ' ';
                        isDecimal = true;
                    }
                }
            }
            string result = new string(array);
            result = result.Replace(" ", "");
            return result;
        }
        /// <summary>
        /// Get the newest user balance from server update to user object.
        /// </summary>
        /// <param name="callback">Call when finished.</param>
        public static void UpdateBalance(GrdEventHandler callback)
        {
            handler.Get((data) =>
            {
                GrdEventArgs args;
                Dictionary<string, object> result = GetObjectData((string)data);
                int error = int.Parse(result["error"].ToString());
                if (error==0)
                {
                    if (result.ContainsKey("balance"))
                    {
                        User.balance = decimal.Parse(result["balance"].ToString());
                    }
                    args = new GrdEventArgs(0, data, user.balance.ToString());
                }
                else
                {
                    args = new GrdEventArgs(error, data, result["message"].ToString());
                }
                if (callback != null)
                {
                    callback(error, args);
                }
            }, "accountbalance");
        }
        /// <summary>
        /// Transfer user money to another wallet
        /// </summary>
        /// <param name="toAddress">Address of the wallet to transfer to</param>
        /// <param name="money">The amount of money to tranfer</param>
        /// <param name="callback">Call when the transfer finished!</param>
        public static void Transfer(string toAddress, decimal money, string otp, GrdEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("to", toAddress);
            pars.Add("value", FormatNumber(money));
            pars.Add("otp", otp);
            handler.Post((data) =>
            {
                GrdEventArgs args=null;
                Dictionary<string, object> result = GetObjectData((string)data);
                int error = int.Parse(result["error"].ToString());
                if (error==0)
                {
                    if (!result.ContainsKey("balance"))
                    {
                        User.balance -= money;
                    }
                    else
                    {
                        User.balance = decimal.Parse(result["balance"].ToString());
                    }
                    args = new GrdEventArgs(error, data, User.balance.ToString());
                }
                else
                {
                    args = new GrdEventArgs(error, data, result["message"].ToString());
                }
                if (callback != null)
                {
                    callback(error, args);
                }
            }, "transfer", pars);
        }
        /// <summary>
        /// Use when user want to turn on the 2 steps verification.
        /// </summary>
        /// <param name="callback">Call when the request is completed and return the result</param>
        public static void RequestEnableOtp(GrdTextureEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            handler.Post((data) =>
            {
                Dictionary<string, object> result = GetObjectData((string)data);
                if (callback != null)
                {
                    GrdTextureEventArgs args = null;
                    int error = int.Parse(result["error"].ToString());
                    object r = null;
                    if (error == 0)
                    {
                        string qrcode = result["qrcode"].ToString().Replace("data:image/image/png;base64,", "");
                        Texture2D texture = null;
                        try
                        {
                            texture = GetTexture(qrcode);
                        }
                        catch
                        {
                        }
                        string text = result["secret"].ToString();
                        args=new GrdTextureEventArgs(0,data,text, texture);
                    }
                    else
                    {
                        args = new GrdTextureEventArgs(error, data, result["message"].ToString(), null);
                    }
                    callback(error, args);
                }
            }, "requestotp", pars);
        }
        /// <summary>
        /// Allow user enable or disable the 2 steps verification security options
        /// </summary>
        /// <param name="otp">The string 6 digits otp code generate by google authentication app</param>
        /// <param name="enabled">True if enable otp, false if disable</param>
        /// <param name="callback">Call when finished the request</param>
        public static void EnableOtp(string otp, bool enabled, GrdEventHandler callback)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("otp", otp);
            pars.Add("otpoptions", enabled ? "1" : "0");
            handler.Post((data) =>
            {
                Dictionary<string, object> result = GetObjectData((string)data);
                int error = int.Parse(result["error"].ToString());
                if (error == 0)
                {
                    User.otp = enabled;
                }
                if (callback != null)
                {
                    GrdEventArgs args = new GrdEventArgs(error,data, result["message"].ToString());
                    callback(error, args);
                }
            }, "enableotp", pars);
        }
        /// <summary>
        /// Get the leaderboard list by score type
        /// </summary>
        /// <param name="scoreType">The score type want to get leaderboard</param>
        /// <param name="start">Start from rank</param>
        /// <param name="count">Number of item return</param>
        /// <param name="callBack">Call when finished the action</param>
        public static void GetLeaderBoard(string scoreType,int start,int count,GrdLeaderBoardEventHandler callBack)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("scoretype", scoreType);
            pars.Add("start", start.ToString());
            pars.Add("count", count.ToString());
            handler.Post((data) =>
            {
                if (callBack != null)
                {
                    Dictionary<string, object> result = GetObjectData((string)data);
                    int error = int.Parse(result["error"].ToString());
                    System.Collections.ObjectModel.Collection<LeaderBoardItem>leaderBoard=null;
                    if (error == 0)
                    {
                        leaderBoard = new System.Collections.ObjectModel.Collection<LeaderBoardItem>(MiniJSON.Json.GetObject<LeaderBoardItem[]>(result["leaderboard"]));
                    }
                    GrdLeaderBoardEventArgs args = new GrdLeaderBoardEventArgs(error, data,"", leaderBoard);
                    callBack(error, args);
                }
            }, "getleaderboard", pars);
        }
        /// <summary>
        /// Get the user session data with the key name in a session store
        /// </summary>
        /// <param name="store">The session store name.</param>
        /// <param name="key">The key name of the data.</param>
        /// <param name="start">Start record number begin with 0</param>
        /// <param name="count">Number of record will return</param>
        /// <param name="callBack">Call when finished</param>
        public static void GetUserSessionData(string store, string key, int start, int count, GrdSessionDataEventHandler callBack)
        {
            GetUserSessionData(store,new string[] { key }, start, count, callBack);
        }
        /// <summary>
        /// Get the user session data with all the key in the keys array in a session store
        /// </summary>
        /// <param name="store">The session store name.</param>
        /// <param name="keys">The array of key name of the data.</param>
        /// <param name="start">Start record number begin with 0</param>
        /// <param name="count">Number of record will return</param>
        /// <param name="callBack">Call when finished</param>
        public static void GetUserSessionData(string store,string[]keys,int start, int count,GrdSessionDataEventHandler callBack)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("store", store);
            pars.Add("keys", string.Join(",", keys));
            pars.Add("start", start.ToString());
            pars.Add("count", count.ToString());
            handler.Post((data) =>
            {
                if (callBack != null)
                {
                    Dictionary<string, object> result = GetObjectData((string)data);
                    int error = int.Parse(result["error"].ToString());
                    object list = null;
                    if (result.ContainsKey("data"))
                    {
                        list = result["data"];
                    }
                    string message = "";
                    if (result.ContainsKey("message"))
                    {
                        message = result["message"].ToString();
                    }
                    GrdSessionDataEventArgs args = new GrdSessionDataEventArgs(error, data, message, list);
                    callBack(error, args);
                }
            }, "getusersessiondata", pars);
        }
        /// <summary>
        /// Get the user's wallet transactions.
        /// </summary>
        /// <param name="start">Start record number. Begin with 0.</param>
        /// <param name="count">The number of transactions will return</param>
        /// <param name="callBack">Call when finished.</param>
        public static void GetTransactions( int start, int count, GrdTransactionEventHandler callBack)
        {
            Dictionary<string, string> pars = new Dictionary<string, string>();
            pars.Add("start", start.ToString());
            pars.Add("count", count.ToString());
            handler.Post((data) =>
            {
                if (callBack != null)
                {
                    Dictionary<string, object> result = GetObjectData((string)data);
                    int error = int.Parse(result["error"].ToString());
                    object list = null;
                    if (result.ContainsKey("transactions"))
                    {
                        list = result["transactions"];
                    }
                    string message = "";
                    if (result.ContainsKey("message"))
                    {
                        message = result["message"].ToString();
                    }
                    GrdTransactionEventArgs args = new GrdTransactionEventArgs(error, data, message, list);
                    callBack(error, args);
                }
            }, "transactions", pars);
        }
    }
    /// <summary>
    /// Delegate use to handle event when gamereward server response the result
    /// </summary>
    /// <param name="error">Error value: 0 if no error, else there was an error while call</param>
    /// <param name="args">args contains properties: Text is the text data field,RawData is the json data response from server</param>
    public delegate void GrdEventHandler(int error, GrdEventArgs args);
    /// <summary>
    /// Delegate use to handle event when gamereward server response string and texture result
    /// </summary>
    /// <param name="error">Error value: 0 if no error, else there was an error while call</param>
    /// <param name="args">args contains properties:Texture is the texture return from server, Text is the text data field,RawData is the json data response from server,ErrorMessage is describe the error</param>
    public delegate void GrdTextureEventHandler(int error, GrdTextureEventArgs args);
    /// <summary>
    /// Delegate use to handle event when gamereward server response string and texture result
    /// </summary>
    /// <param name="error">Error value: 0 if no error, else there was an error while call</param>
    /// <param name="args">args contains:Properties Data is the object data structure in Dictionary<string,object> or List<object>, Text is the text data field,RawData is the json data response from server,ErrorMessage is describe the error.\r\n Method generic GetObject to convert to your custom object</param>
    public delegate void GrdCustomEventHandler(int error, GrdCustomEventArgs args);
    /// <summary>
    /// Delegate use to handle event when gamereward server response string and leaderboard result
    /// </summary>
    /// <param name="error">Error value: 0 if no error, else there was an error while call</param>
    /// <param name="args">args contains properties:Data is the list of leaderboard return from server,RawData is the json data response from server,ErrorMessage is describe the error</param>
    public delegate void GrdLeaderBoardEventHandler(int error, GrdLeaderBoardEventArgs args);
    /// <summary>
    /// Delegate use to handle event when gamereward server response string and sessiondata result
    /// </summary>
    /// <param name="error">Error value: 0 if no error, else there was an error while call</param>
    /// <param name="args">args contains properties:Data is the list of SessionData return from server,RawData is the json data response from server,ErrorMessage is describe the error</param>
    public delegate void GrdSessionDataEventHandler(int error, GrdSessionDataEventArgs args);
    /// <summary>
    /// Delegate use to handle event when gamereward server response string and  result
    /// </summary>
    /// <param name="error">Error value: 0 if no error, else there was an error while call</param>
    /// <param name="args">args contains properties:Data is the list of Transaction return from server,RawData is the json data response from server,ErrorMessage is describe the error</param>
    public delegate void GrdTransactionEventHandler(int error, GrdTransactionEventArgs args);
    /// <summary>
    /// Class stored gamereward user information
    /// </summary>
    public class GrdUser
    {
        /// <summary>
        /// The username of the user
        /// </summary>
        public string username;
        /// <summary>
        /// The email of the user
        /// </summary>
        public string email;
        /// <summary>
        /// The Grd wallet Address
        /// </summary>
        public string address;
        /// <summary>
        /// The balance of coin in this user
        /// </summary>
        public decimal balance;
        /// <summary>
        ///  if true:use the otp security otherwise false
        /// </summary>
        public bool otp = false;
    }
    /// <summary>
    /// Class to store the callback data from the action that return text
    /// </summary>
    public class GrdEventArgs
    {
        public GrdEventArgs(int error, string rawData, string text)
        {
            this.text = text;
            this.RawData = rawData;
            this.error = error;
        }
        private int error;
        private string text;
        public string RawData { get; private set; }
        /// <summary>
        /// The data in string return from server
        /// </summary>
        public string Text { get { if (error != 0) { return ""; } else { return text; } } }
        /// <summary>
        /// The error message if the action had an error
        /// </summary>
        public string ErrorMessage { get { if (error == 0) { return ""; } else { return text; } } }
    }
    /// <summary>
    /// Class to store the callback data from the action that return Image
    /// </summary>
    public class GrdTextureEventArgs : GrdEventArgs
    {
        public GrdTextureEventArgs(int error,string rawData, string text, Texture2D texture)
            : base(error,rawData, text)
        {
            this.Texture = texture;
        }
        /// <summary>
        /// The texture data return from server
        /// </summary>
        public Texture2D Texture { get; private set; }
    }
    /// <summary>
    /// Class to store the callback data from the action that return LeaderBoard data
    /// </summary>
    public class GrdLeaderBoardEventArgs : GrdEventArgs
    {
        public GrdLeaderBoardEventArgs(int error, string rawData, string text, System.Collections.ObjectModel.Collection<LeaderBoardItem> data)
            : base(error,rawData, text)
        {
            this.Data = data;
        }
        /// <summary>
        /// The list of leaderboard
        /// </summary>
        public System.Collections.ObjectModel.Collection<LeaderBoardItem> Data { get; private set; }
    }
    /// <summary>
    /// Class to store the callback data from the action that return Json data
    /// </summary>
    public class GrdCustomEventArgs : GrdEventArgs
    {
        public GrdCustomEventArgs(int error, string rawData, string text, object data)
            : base(error,rawData, text)
        {
            this.Data = data;
        }
        /// <summary>
        /// The object that store the data parse from json string
        /// </summary>
        public object Data { get; private set; }
        /// <summary>
        /// Convert the data to custom class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetData<T>() where T : class
        {
            return MiniJSON.Json.GetObject<T>(Data);
        }
    }
    /// <summary>
    /// Class to store the callback data from the action that return user session data
    /// </summary>
    public class GrdSessionDataEventArgs : GrdEventArgs
    {
        public GrdSessionDataEventArgs(int error, string rawData, string text, object data)
            : base(error,rawData, text)
        {
            if (error == 0)
            {
                if (data == null)
                {
                    this.data = new System.Collections.ObjectModel.Collection<SessionData>();
                }
                else
                {
                    this.data = new System.Collections.ObjectModel.Collection<SessionData>(MiniJSON.Json.GetObject<SessionData[]>(data));
                }
            }
        }
        private System.Collections.ObjectModel.Collection<SessionData> data;
        /// <summary>
        /// The list of session and it's data
        /// </summary>
        public System.Collections.ObjectModel.Collection<SessionData> Data
        {
            get
            {
                return data;
            }
        }
    }
    /// <summary>
    /// Class to store the callback data from the action that return transaction data
    /// </summary>
    public class GrdTransactionEventArgs : GrdEventArgs
    {
        public GrdTransactionEventArgs(int error, string rawData, string text, object data)
            : base(error, rawData, text)
        {
            if (error == 0)
            {
                if (data == null)
                {
                    this.data = new System.Collections.ObjectModel.Collection<Transaction>();
                }
                else
                {
                    this.data = new System.Collections.ObjectModel.Collection<Transaction>(MiniJSON.Json.GetObject<Transaction[]>(data));
                }
            }
        }
        private System.Collections.ObjectModel.Collection<Transaction> data;
        /// <summary>
        /// The list of transaction
        /// </summary>
        public System.Collections.ObjectModel.Collection<Transaction> Data
        {
            get
            {
                return data;
            }
        }
    }
    /// <summary>
    /// Class store the data of leaderboard
    /// </summary>
    public class LeaderBoardItem
    {
        /// <summary>
        /// User name of player
        /// </summary>
        public string username;
        /// <summary>
        /// Score of player
        /// </summary>
        public double score;
        /// <summary>
        /// Rank of player sort by score
        /// </summary>
        public int rank;
    }
    /// <summary>
    /// Class store the data of user session
    /// </summary>
    public class SessionData
    {
        /// <summary>
        /// The id of the session
        /// </summary>
        public int sessionid;
        /// <summary>
        /// The unix timestamp when session start
        /// </summary>
        public long sessionstart;
        /// <summary>
        /// Convert the sessionstart to datetime
        /// </summary>
        /// <returns></returns>
        public System.DateTime GetTime()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            epochStart.AddSeconds(sessionstart);
            return epochStart;
        }
        /// <summary>
        /// The dictionary of values by key of the session data
        /// </summary>
        public Dictionary<string, string> values = new Dictionary<string, string>();
    }
    /// <summary>
    /// The transaction type enum
    /// </summary>
    public enum TransactionType
    {
        Base=1,Internal=2,External=3
    }
    /// <summary>
    /// The transaction status enum
    /// </summary>
    public enum TransactionStatus
    {
        Pending = 0, Success = 1, Error = 2
    }
    /// <summary>
    /// Class store the transaction data
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The Tx of the transaction
        /// </summary>
        public string tx;
        /// <summary>
        /// The address send transaction
        /// </summary>
        public string from;
        /// <summary>
        /// The addess received transaction
        /// </summary>
        public string to;
        /// <summary>
        /// Amount of GRD send
        /// </summary>
        public decimal amount;
        /// <summary>
        /// The unix timestamp when send transaction
        /// </summary>
        public long transdate;
        /// <summary>
        /// The type of transaction: Internal,External, Base
        /// </summary>
        public TransactionType transtype;
        /// <summary>
        /// The status of transaction: Success, Failed, Pending
        /// </summary>
        public TransactionStatus status;
        /// <summary>
        /// Convert the Transdate to DateTime
        /// </summary>
        /// <returns></returns>
        public System.DateTime GetTime()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            epochStart.AddSeconds(transdate);
            return epochStart;
        }
    }
}