/// <summary>
/// ABC Wallet Kit for Android.
/// Author: Frank T (frank@saseeme.com)
/// </summary>
namespace ABCWalletKit.Android
{
    using System.Collections.Generic;
    using UnityEngine;

    public class bid
    {
        public static void Initialize()
        {

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass bidClass = new AndroidJavaClass("io.ahnlab.abcwalletkit.a.bid");
            bidClass.CallStatic("initialize", context);
        }
    }

    /// <summary>
    /// Provide ABC service methods such as login, user registration, token reissue, 
    /// keyshare creation/recovery, transaction signing, and token transfer.
    /// </summary>
    public class Service
    {
        private AndroidJavaObject _nativeService;
        private string rpcUrl;
        private Dictionary<string, string> rpcHeaders;
        public string serviceId { get; }
        private string serverAuthURL;
        private string serverAppURL;
        private string mpcServerName;
        private string mpcPort;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rpcUrl"></param>
        /// <param name="rpcHeaders"></param>
        /// <param name="serviceId"></param>
        /// <param name="serverAuthURL"></param>
        /// <param name="serverAppURL"></param>
        /// <param name="mpcServerName"></param>
        /// <param name="mpcPort"></param>
        public Service(string rpcUrl, Dictionary<string, string> rpcHeaders, string serviceId, string serverAuthURL, string serverAppURL, string mpcServerName, string mpcPort)
        {
            this.rpcUrl = rpcUrl;
            this.rpcHeaders = rpcHeaders;
            this.serviceId = serviceId;
            this.serverAuthURL = serverAuthURL;
            this.serverAppURL = serverAppURL;
            this.mpcServerName = mpcServerName;
            this.mpcPort = mpcPort;

            AndroidJavaObject rpcHeadersHashMap = ABCWalletKitHelper.DictionaryToHashMap(rpcHeaders);
            _nativeService = new AndroidJavaObject("io.ahnlab.abcwalletkit.service.Service", rpcUrl, rpcHeadersHashMap, serviceId, serverAuthURL, serverAppURL, mpcServerName, mpcPort);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rpcUrl"></param>
        /// <param name="serviceId"></param>
        /// <param name="serverAuthURL"></param>
        /// <param name="serverAppURL"></param>
        /// <param name="mpcServerName"></param>
        /// <param name="mpcPort"></param>
        public Service(string rpcUrl, string serviceId, string serverAuthURL, string serverAppURL, string mpcServerName, string mpcPort)
        {
            this.rpcUrl = rpcUrl;
            this.rpcHeaders = null;
            this.serviceId = serviceId;
            this.serverAuthURL = serverAuthURL;
            this.serverAppURL = serverAppURL;
            this.mpcServerName = mpcServerName;
            this.mpcPort = mpcPort;

            _nativeService = new AndroidJavaObject("io.ahnlab.abcwalletkit.service.Service", rpcUrl, serviceId, serverAuthURL, serverAppURL, mpcServerName, mpcPort);
        }

        /// <summary>
        ///  Send email signup confirmation code
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="language">"ko" or "en"</param>
        /// <returns>"OK" if success</returns>
        /// <example><c>
        /// try {
        ///     service.SendCode(email, "en");
        /// }catch (Exception e) {
        /// }
        /// <c></example>
        public string SendCode(string email, string language)
        {
            return _nativeService.Call<string>("sendCode", email, language);
        }

        /// <summary>
        ///Create a membership (sign up), an email verification code is required. 
        ///(Go to the email verification code sending step. Consent for users over 14 years of age, agree and you can receive a code).
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <param name="code">Email verification code</param>
        /// <param name="boverage">Consent for users over 14 years of age</param>
        /// <param name="bagree">Consent to use the service</param>
        /// <param name="bcollect">Personal information collection and usage agreement</param>
        /// <param name="bthirdparty">Consent to provide to third parties</param>
        /// <param name="badverties">Consent to receive advertising information</param>
        /// <returns>Return value <c>"Created"</c> if success</returns
        /// <example><c>
        /// try {
        ///     service.AddUser(
        ///         email, 
        ///         password, 
        ///         code, 
        ///         true, 
        ///         true, 
        ///         true, 
        ///         true, 
        ///         true
        ///     );
        /// }catch (e: AddUserError) {
        /// }
        /// </c></example>
        public string AddUser(string email, string password, string code, bool boverage, bool bagree, bool bcollect, bool bthirdparty, bool badvertise)
        {
            return _nativeService.Call<string>("addUser", email, password, code, boverage, bagree, bcollect, bthirdparty, badvertise);
        }

        /// <summary>
        /// Login to ABC wallet
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <returns>An instance of <c>LoginResponse</c> if success</returns>
        public LoginResponse Login(string email, string password)
        {
            AndroidJavaObject resp = _nativeService.Call<AndroidJavaObject>("login", email, password);
            return new LoginResponse(resp);
        }

        /// <summary>
        /// Create a wallet and store its information
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="devicePassword">Device password (Keyshare password) plain text</param>
        /// <param name="accessToken">Authentication access token, responsed from Login method</param>
        /// <returns>An instance of <c>KeyShareInfo</c> if success</returns>
        /// <example><c>
        /// try {
        ///     service = new Service(
        ///             "https://goerli.infura.io/v3",
        ///             "https://mw.myabcwallet.com",
        ///             "https://dev-api.id.myabcwallet.com",
        ///             "https://cs.dev-mw.myabcwallet.com:9000",
        ///             "cs.dev-mw.myabcwallet.com",
        ///             "9000"
        ///         );
        ///     KeyShareInfo share = service.GenerateShare(email, password, accessToken);
        /// catch (Exception e) {
        /// }
        /// </c></example>
        public KeyShareInfo GenerateShare(string email, string devicePassword, string accessToken)
        {
            AndroidJavaObject resp = _nativeService.Call<AndroidJavaObject>("generateShare", email, devicePassword, accessToken);
            return new KeyShareInfo(resp);
        }

        /// <summary>
        /// Recover a wallet and store it information
        /// </summary>
        /// <param name="devicePassword">Device password (Keyshare password) plain text</param>
        /// <param name="accessToken">Authentication access token, responsed from Login method</param>
        /// <returns>An instance of <c>KeyShareInfo</c> if success</returns>
        /// <example><c>
        /// try
        /// {
        ///     service = new Service(
        ///         "https://goerli.infura.io/v3",
        ///         "https://mw.myabcwallet.com",
        ///         "https://dev-api.id.myabcwallet.com",
        ///         "https://cs.dev-mw.myabcwallet.com:9000",
        ///         "cs.dev-mw.myabcwallet.com",
        ///         "9000"
        ///     );
        ///     KeyShareInfo share = service.RecoverShare(password, accessToken);
        /// } catch (Exception e) {
        /// }
        /// </c></example>
        public KeyShareInfo RecoverShare(string devicePassword, string accessToken)
        {
            AndroidJavaObject resp = _nativeService.Call<AndroidJavaObject>("recoverShare", devicePassword, accessToken);
            return new KeyShareInfo(resp);
        }

        /// <summary>
        /// Generate a Hex string to transmit token assets to another address using ERC20 network
        /// </summary>
        /// <param name="to">Receiving address</param>
        /// <param name="value">Transfering amount</param>
        /// <returns></returns>
        public string Erc20TransferData(string to, string value)
        {
            ABCEthereum ethereum;

            if (this.rpcHeaders != null)
            {
                ethereum = new ABCEthereum(this.rpcUrl, this.rpcHeaders);
            }
            else
            {
                ethereum = new ABCEthereum(this.rpcUrl);
            }

            return ethereum.Erc20TransferData(to, value);
        }

        public string SignLegacy(string from, string to, string gasLimit, string gasPrice, string value, string data, string nonce,
            string chainId, string accessToken, string encryptDevicePassword, string pvencstr, string uid, int wid, string sid)
        {
            return _nativeService.Call<string>("signLegacy", from, to, gasLimit, gasPrice, value, data, nonce, chainId, accessToken, encryptDevicePassword, pvencstr, uid, wid, sid);
        }

        public string SendRawTransaction(string signedserializetx)
        {
            return _nativeService.Call<string>("sendRawTransaction", signedserializetx);
        }

        public string PersonalSign(string message, string chainId, string accessToken, string encryptDevicePassword, string pvencstr, string uid, int wid, string sid)
        {
            return _nativeService.Call<string>("personalSign", message, chainId, accessToken, encryptDevicePassword, pvencstr, uid, wid, sid);
        }

        public string SignTypedDataV3(string messageJson, string accessToken, string encryptDevicePassword, string pvencstr, string uid, int wid, string sid)
        {
            return _nativeService.Call<string>("signTypedDataV3", messageJson, accessToken, encryptDevicePassword, pvencstr, uid, wid, sid);
        }

        public string SignTypedDataV4(string messageJson, string accessToken, string encryptDevicePassword, string pvencstr, string uid, int wid, string sid)
        {
            return _nativeService.Call<string>("signTypedDataV4", messageJson, accessToken, encryptDevicePassword, pvencstr, uid, wid, sid);
        }

        public User GetUserWallet(string accessToken)
        {
            AndroidJavaObject nativeUser = _nativeService.Call<AndroidJavaObject>("getUserWallet", accessToken);
            return new User(nativeUser);
        }



    }

    public class LoginResponse
    {
        public string accessToken { get; set; }
        public string tokenType { get; set; }
        public int expireIn { get; set; }
        public string refreshToken { get; set; }

        private AndroidJavaObject _nativeResponse;

        public LoginResponse(AndroidJavaObject nativeResponse)
        {
            _nativeResponse = nativeResponse;

            this.accessToken = nativeResponse.Get<string>("access_token");
            this.tokenType = nativeResponse.Get<string>("token_type");
            this.expireIn = nativeResponse.Get<int>("expire_in");
            this.refreshToken = nativeResponse.Get<string>("refresh_token");
        }
    }

    public class KeyShareInfo
    {
        public string Uid { get; set; }
        public int wid { get; set; }
        public string sid { get; set; }
        public string pvencstr { get; set; }
        public string encryptDevicePassword { get; set; }

        private AndroidJavaObject _nativeInfo;

        public KeyShareInfo(AndroidJavaObject nativeInfo)
        {
            _nativeInfo = nativeInfo;

            this.Uid = nativeInfo.Get<string>("uid");
            this.wid = nativeInfo.Get<int>("wid");
            this.sid = nativeInfo.Get<string>("sid");
            this.pvencstr = nativeInfo.Get<string>("pvencstr");
            this.encryptDevicePassword = nativeInfo.Get<string>("encryptDevicePassword");
        }
    }

    public class Account
    {
        public string id { get; set; }
        public string sid { get; set; }
        public string ethAddress { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public string signer { get; set; }

        public Account(string id, string sid, string ethAddress, string icon, string name, string signer)
        {
            this.id = id;
            this.sid = sid;
            this.ethAddress = ethAddress;
            this.icon = icon;
            this.name = name;
            this.signer = signer;
        }

        public Account(AndroidJavaObject nativeAccount = null)
        {
            if (nativeAccount != null)
            {
                this.id = nativeAccount.Get<string>("id");
                this.sid = nativeAccount.Get<string>("sid");
                this.ethAddress = nativeAccount.Get<string>("ethAddress");
                this.icon = nativeAccount.Get<string>("icon");
                this.name = nativeAccount.Get<string>("name");
                this.signer = nativeAccount.Get<string>("signer");
            }
        }
    }

    public class User
    {
        public string _id { get; set; }
        public string uid { get; set; }
        public int wid { get; set; }
        public string email { get; set; }
        public List<Account> accounts { get; set; }

        /*public List<Favorite> favorites { get; set; }
        public List<Autoconfirm> autoconfirms { get; set; }*/

        public bool twoFactorEnabled { get; set; }
        public string hashToSign { get; set; }
        public int twoFactorResetRetryCount { get; set; }
        public long twoFactorRetryFreezeEndTime { get; set; }
        public long twoFactorFreezeEndTime { get; set; }

        private AndroidJavaObject _nativeUser;

        public User(AndroidJavaObject nativeUser = null)
        {
            if (nativeUser != null)
            {
                this._id = nativeUser.Get<string>("_id");
                this.uid = nativeUser.Get<string>("uid");
                this.wid = nativeUser.Get<int>("wid");
                this.email = nativeUser.Get<string>("email");
                AndroidJavaObject nativeAccounts = nativeUser.Get<AndroidJavaObject>("accounts");
                if (nativeAccounts != null)
                {
                    accounts = new List<Account>();
                    int size = nativeAccounts.Call<int>("size");
                    for (int i = 0; i < size; i++)
                    {
                        AndroidJavaObject nativeAccount = nativeAccounts.Call<AndroidJavaObject>("get", i);
                        if (nativeAccount != null)
                        {
                            accounts.Add(new Account(nativeAccount));
                        }
                    }
                }

                /*this.favorites = favorites;
                this.autoconfirms = autoconfirms;*/

                this.twoFactorEnabled = nativeUser.Get<bool>("twoFactorEnabled");
                this.hashToSign = nativeUser.Get<string>("hashToSign");
                this.twoFactorResetRetryCount = nativeUser.Get<int>("twoFactorResetRetryCount");
                this.twoFactorRetryFreezeEndTime = nativeUser.Get<long>("twoFactorRetryFreezeEndTime");
                this.twoFactorFreezeEndTime = nativeUser.Get<long>("twoFactorFreezeEndTime");
            }

            _nativeUser = nativeUser;
        }
    }

    public class ABCEthereum
    {
        private readonly string rpcUrl;
        private string httpHeaders;
        private AndroidJavaObject _nativeABCEthereum;

        public ABCEthereum(string rpcUrl)
        {
            this.rpcUrl = rpcUrl;
            this.httpHeaders = "";
            _nativeABCEthereum = new AndroidJavaObject("io.ahnlab.abcwalletkit.ethereum.ABCEthereum", rpcUrl);
        }

        public ABCEthereum(string rpcUrl, Dictionary<string, string> httpHeaders)
        {
            this.rpcUrl = rpcUrl;
            this.httpHeaders = JsonUtility.ToJson(httpHeaders);
            _nativeABCEthereum = new AndroidJavaObject("io.ahnlab.abcwalletkit.ethereum.ABCEthereum", rpcUrl, ABCWalletKitHelper.DictionaryToHashMap(httpHeaders));
        }

        public string GetBalance(string address)
        {
            return _nativeABCEthereum.Call<string>("getBalance", address);
        }

        public string Erc20TransferData(string to, string value)
        {
            return _nativeABCEthereum.Call<string>("erc20TransferData", to, value);
        }

        public string PersonalSignHashMessage(string message, string chainId)
        {
            return _nativeABCEthereum.Call<string>("personalSignHashMessage", message, chainId);
        }

    }

    public class MpcTokenResponse
    {
        public string uid { get; set; }
        public string mpcJwt { get; set; }

        public MpcTokenResponse(AndroidJavaObject nativeResponse)
        {
            uid = nativeResponse.Get<string>("uid");
            mpcJwt = nativeResponse.Get<string>("mpcJwt");
        }
    }

    public class ABCApp
    {
        private readonly string serverURL;
        private readonly string language;
        private readonly string device;

        private AndroidJavaObject _nativeABCApp;

        public ABCApp(string serverURL, string language, string device)
        {
            this.serverURL = serverURL;
            this.language = language;
            this.device = device;

            _nativeABCApp = new AndroidJavaObject("io.ahnlab.abcwalletkit.app.ABCApp", serverURL, language, device);
        }

        public string GetPersonalSignJson(string message, string chainId)
        {
            return _nativeABCApp.Call<string>("getPersonalSignJson", message, chainId);
        }

        public MpcTokenResponse GetMpcToken(string accessToken, string body)
        {
            AndroidJavaObject resp = _nativeABCApp.Call<AndroidJavaObject>("getMpcToken", accessToken, body);
            return new MpcTokenResponse(resp);
        }

    }

    public class MPC
    {
        private AndroidJavaObject _nativeMPC;

        public MPC()
        {
            _nativeMPC = new AndroidJavaObject("io.ahnlab.abcwalletkit.mpc.MPC");
        }

        public string Sign(string aesEncKey, string serverName, string port, string myAddress, string uid, int wid, string messageHS, string pvEncStr, string accessToken, bool tls)
        {
            return _nativeMPC.Call<string>("sign", aesEncKey, serverName, port, myAddress, uid, wid, messageHS, pvEncStr, accessToken, tls);
        }
    }

    public class Util
    {
        private AndroidJavaObject _nativeUtil;
        public Util()
        {
            _nativeUtil = new AndroidJavaObject("io.ahnlab.abcwalletkit.util.Util");
        }

        public string MpcSignConcat(string sigStr)
        {
            return _nativeUtil.Call<string>("mpcSignConcat", sigStr);
        }
    }

    /// <summary>
    /// Provide ABC Wallet's authentication methods for signing up, logging in, and changing password.
    /// </summary>
    public class ABCAuth
    {
        private readonly string serverURL;
        private readonly string language;
        private readonly string device;

        private AndroidJavaObject _nativeABCAuth;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverURL">The server URL. For dev enviroment, set to "https://dev-api.id.myabcwallet.com"</param>
        /// <param name="language">"ko" or "en"</param>
        /// <param name="device">The user agent</param>
        /// <example><c>
        /// ABCAuth auth = new ABCAuth(serverAuthURL, "en", device);
        /// </c></example>
        public ABCAuth(string serverURL, string language, string device)
        {
            this.serverURL = serverURL;
            this.language = language;
            this.device = device;

            _nativeABCAuth = new AndroidJavaObject("io.ahnlab.abcwalletkit.auth.ABCAuth");
        }

        /// <summary>
        /// Login to ABC wallet
        /// </summary>
        /// <param name="grantType">Set to <c>"password"</c></param>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <param name="serviceId">ID of the current working Service instance</param>
        /// <returns>An instance of <c>LoginResponse</c> if success</returns>
        public LoginResponse Login(string grantType, string email, string password, string serviceId)
        {
            AndroidJavaObject resp = _nativeABCAuth.Call<AndroidJavaObject>("login", grantType, email, password, serviceId);
            return new LoginResponse(resp);
        }

        /// <summary>
        ///  Send email authentication/verification code
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="template">template Mail template type (default: verify). Use following value:
        /// <c>"verify"</c> for user signing up
        /// <c>"changepassword"</c> for changing password
        /// <c>"initpassword"</c> for password reset 
        /// </param>
        /// <returns>"OK" if success</returns>
        /// <example><c>
        /// try {
        ///     abcAuth.SendCode(email, "verify");
        /// }catch (Exception e) {
        /// }
        /// <c></example>
        public string SendCode(string email, string template)
        {
            return _nativeABCAuth.Call<string>("sendCode", email, template);
        }

        /// <summary>
        ///  Email authentication code verification
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="code">Authentication code</param>
        /// <returns>"OK" if success</returns>
        /// <example><c>
        /// try {
        ///     abcAuth.VerifyCode(email, inputCode);
        /// }catch (Exception e) {
        ///     error(e)
        /// }
        /// </c></example>
        public string VerifyCode(string email, string code)
        {
            return _nativeABCAuth.Call<string>("verifyCode", email, code);
        }

        /// <summary>
        ///Create a membership (sign up), an email verification code is required. 
        ///(Go to the email verification code sending step. Consent for users over 14 years of age, agree and you can receive a code).
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <param name="code">Email verification code</param>
        /// <param name="serviceId">Set to "https://mw.myabcwallet.com"</param>
        /// <param name="boverage">Consent for users over 14 years of age</param>
        /// <param name="bagree">Consent to use the service</param>
        /// <param name="bcollect">Personal information collection and usage agreement</param>
        /// <param name="bthirdparty">Consent to provide to third parties</param>
        /// <param name="badverties">Consent to receive advertising information</param>
        /// <returns>Return value <c>"Created"</c> if success</returns
        /// <example><c>
        /// try {
        ///     abcAuth.AddUser(
        ///         email, 
        ///         password, 
        ///         code, 
        ///         "https://mw.myabcwallet.com", 
        ///         true, 
        ///         true, 
        ///         true, 
        ///         true, 
        ///         true
        ///     );
        /// }catch (e: AddUserError) {
        /// }
        /// </c></example>
        public string AddUser(string email, string password, string code, string serviceId, bool boverage, bool bagree, bool bcollect, bool bthirdparty, bool badverties)
        {
            return _nativeABCAuth.Call<string>("addUser", email, password, code, serviceId, boverage, bagree, bcollect, bthirdparty, badverties);
        }

        /// <summary>
        /// Request for deleting an user
        /// </summary>
        /// <param name="accessToken">User access token</param>
        /// <param name="email">User email</param>
        /// <param name="serviceId">ID of the current working Service instance</param>
        /// <param name="code">Email confirmation code</param>
        /// <returns>"OK" if success</returns>
        /// <example><c>
        /// try {
        ///     abcAuth.ChangePassword(
        ///         email, 
        ///         oldPassword, 
        ///         newPassoword, 
        ///         "https://mw.myabcwallet.com"
        ///     );
        /// }catch (Exception e) {
        /// }
        /// </c></example>
        public string DeleteUser(string accessToken, string email, string serviceId, string code)
        {
            return _nativeABCAuth.Call<string>("deleteUser", accessToken, email, serviceId, code);
        }

        /// <summary>
        /// Refresh (renew) an access token
        /// </summary>
        /// <param name="grantType">Set to "refresh_token"</param>
        /// <param name="refreshToken">The current access token</param>
        /// <returns>An instance of <c>LoginResponse</c> if success</returns>
        /// <example><c>
        /// try {
        ///     LoginResponse refreshResult = abcAuth.refresh(
        ///         grantType: "refresh_token",
        ///         refreshToken: loginResult.refresh_token
        ///     );
        /// }catch (Exception e) {
        /// }
        /// </c></example>
        public LoginResponse Refresh(string grantType, string refreshToken)
        {
            AndroidJavaObject resp = _nativeABCAuth.Call<AndroidJavaObject>("refresh", grantType, refreshToken);
            return new LoginResponse(resp);
        }

        /// <summary>
        /// Login using social network service (Googler, Apple, Facebook)
        /// </summary>
        /// <param name="token">The token name, use "id_token" for Google, Apple or "access_token" for Facebook</param>
        /// <param name="service">Use "google", "apple" or "facebook"</param>
        /// <param name="serviceId">Use "https://mw.myabcwallet.com"</param>
        /// <returns>An instance of <c>LoginResponse</c> if success</returns>
        /// <example><c>
        /// try {
        ///     LoginResponse snsLoginResult = abcAuth.snsLogin(
        ///         "id_token",
        ///         "facebook",
        ///         "https://mw.myabcwallet.com"
        ///     );
        /// }catch (Exception e) {
        /// }
        /// </c></example>
        public LoginResponse SnsLogin(string token, string service, string serviceId)
        {
            AndroidJavaObject resp = _nativeABCAuth.Call<AndroidJavaObject>("snsLogin", token, service, serviceId);
            return new LoginResponse(resp);
        }

        /// <summary>
        /// Sign up an account using social network service.
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="code">Email verification code</param>
        /// <param name="serviceId">Use "https://mw.myabcwallet.com"</param>
        /// <param name="joinPath">Signup callback path</param>
        /// <param name="socialType">Use "google", "apple" or "facebook"</param>
        /// <param name="boverage">Consent for users over 14 years of age</param>
        /// <param name="bagree">Consent to use the service</param>
        /// <param name="bcollect">Personal information collection and usage agreement</param>
        /// <param name="bthirdparty">Consent to provide to third parties</param>
        /// <param name="badverties">Consent to receive advertising information</param>
        /// <returns>Return value <c>"Created"</c> if success</returns
        /// <example><c>
        /// try {
        ///     abcAuth.snsAddUser(
        ///         email, 
        ///         code, 
        ///         "https://mw.myabcwallet.com",
        ///         "service url",
        ///         "google",
        ///         true, 
        ///         true, 
        ///         true, 
        ///         true, 
        ///         true
        ///     );
        /// }catch (Exception e) {
        /// }
        /// </c></example>
        public string SnsAddUser(string email, string code, string serviceId, string joinPath, string socialType, bool boverage, bool bagree, bool bcollect, bool bthirdparty, bool badverties)
        {
            return _nativeABCAuth.Call<string>("snsAddUser", email, code, serviceId, joinPath, socialType, boverage, bagree, bcollect, bthirdparty, badverties);
        }

        /// <summary>
        /// Initialize or reset user password. To reset a password, you need an email verification code.
        /// (Go to the email verification code sending step)
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">New password</param>
        /// <param name="code">Email verification code</param>
        /// <param name="serviceId">ID of the current working Service instance</param>
        /// <returns><c>"OK" if success</c></returns>
        public string InitPassword(string email, string password, string code, string serviceId)
        {
            return _nativeABCAuth.Call<string>("initPassword", email, password, code, serviceId);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        /// <param name="serviceId">ID of the current working Service instance</param>
        /// <returns></returns>
        public string ChangePassword(string email, string oldPassword, string newPassword, string serviceId)
        {
            return _nativeABCAuth.Call<string>("changePassword", email, oldPassword, newPassword, serviceId);
        }
    }


}