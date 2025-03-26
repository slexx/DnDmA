using DMAW_DND;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Security
{
    public class ApiAuthResponse
    {
        public ApiAuthResponse() { }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("Success")]
        public bool Success { get; set; }

        [JsonProperty("Access")]
        public bool Access { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("Expires")]
        public string Expires { get; set; }

        [JsonProperty("Offsets")]
        public string Offsets { get; set; }
    }

    /// <summary>
    /// Generates a 16 byte Unique Identification code of a computer
    /// Example: 4876-8DB5-EE85-69D3-FE52-8CF7-395D-2EA9
    /// </summary>
    public class FingerPrint
    {
        private static string fingerPrint = string.Empty;

        public string Value()
        {
            if (string.IsNullOrEmpty(fingerPrint))
            {
                fingerPrint = GetHash("CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId());
                //+ "\nDISK >> " + diskId() + "\nVIDEO >> " + videoId() + "\nMAC >> " + macId());
            }
            return fingerPrint;
        }

        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        #region Original Device ID Getting Code
        //Return a hardware identifier
        private static string identifier
        (string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }
        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }
        private static string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            //string retVal = identifier("Win32_Processor", "UniqueId");
            string retVal = "";
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = identifier("Win32_Processor", "Manufacturer");
                    }
                    //Add clock speed for extra security
                    retVal += identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }
        //BIOS Identifier
        private static string biosId()
        {
            return identifier("Win32_BIOS", "Manufacturer")
            + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
            //+ identifier("Win32_BIOS", "IdentificationCode")
            + identifier("Win32_BIOS", "SerialNumber")
            + identifier("Win32_BIOS", "ReleaseDate")
            + identifier("Win32_BIOS", "Version");
        }
        //Main physical hard drive ID
        private static string diskId()
        {
            return identifier("Win32_DiskDrive", "Model")
            + identifier("Win32_DiskDrive", "Manufacturer")
            + identifier("Win32_DiskDrive", "Signature")
            + identifier("Win32_DiskDrive", "TotalHeads");
        }
        //Motherboard ID
        private static string baseId()
        {
            return 
            //identifier("Win32_BaseBoard", "Model")
            identifier("Win32_BaseBoard", "Manufacturer")
            + identifier("Win32_BaseBoard", "Name")
            + identifier("Win32_BaseBoard", "SerialNumber");
        }
        //Primary video controller ID
        private static string videoId()
        {
            return identifier("Win32_VideoController", "DriverVersion")
            + identifier("Win32_VideoController", "Name");
        }
        //First enabled network card ID
        private static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration",
                "MACAddress", "IPEnabled");
        }
        #endregion
    }

    public class Authentication
    {
        private static bool _isAuthenticated = false;
        public static bool _isAuthenticating = true;
        private static string _hwid = string.Empty;

        static Authentication()
        {
            FingerPrint fingerPrint = new();
            _hwid = fingerPrint.Value();
        }

        public static bool IsAuthenticated()
        {
            return _isAuthenticated;
        }

        public static async void Authenticate()
        {
            var TokenFromFile = ReadTokenFromFile();
            if (TokenFromFile != null)
            {
                AuthenticateWithToken(TokenFromFile);
                return;
            }

            Listener listener = new("https://discord.com/api/oauth2/authorize?client_id=635640426275274752&redirect_uri=http%3A%2F%2F127.0.0.1%3A55432&response_type=code&scope=identify%20guilds%20email");
            var result = await listener.Listen(); // returns code from discord oauth2

            if (result != null)
            {
                AuthenticateWithCode(result);
            }
            else
            {
                _isAuthenticated = false;
                _isAuthenticating = false;
            }
        }

        public static async void AuthenticateWithCode(string code)
        {
            var client = new HttpClient();

            var httpRequestMessage = new HttpRequestMessage
            {
#if DEBUG
                RequestUri = new Uri("http://192.168.0.120/auth/code"), // Post to api to check user and hwid against database
#else
                RequestUri = new Uri("https://api.dmawarehouse.com/auth/code"), // Post to api to check user and hwid against database
#endif
                Content = JsonContent.Create(new { hwid = _hwid, code, game = "dd" }),
                Method = HttpMethod.Post,
            };
            var response = await client.SendAsync(httpRequestMessage);
            var responseString = response.Content.ReadAsStringAsync().Result;

            var responseJson = JsonConvert.DeserializeObject<ApiAuthResponse>(responseString);

            if (responseJson.Success == false)
            {
                _isAuthenticated = false;
                _isAuthenticating = false;
            }
            else
            {
                WriteTokenToFile(responseJson.Token);
                AuthenticateWithToken(responseJson.Token);
            }
        }

        public static async void AuthenticateWithToken(string token)
        {
            var client = new HttpClient();

            JsonObject jsonObject = new();
            jsonObject.Add("token", token);
            jsonObject.Add("hwid", _hwid);
            jsonObject.Add("game", "dd");

            var encryptedToken = Cryptography.Base64Encode(jsonObject.ToString());

            var httpRequestMessage = new HttpRequestMessage
            {
#if DEBUG
                RequestUri = new Uri("http://192.168.0.120/auth/token/dd"), // Post to api to check user and hwid against database
#else
                RequestUri = new Uri("https://api.dmawarehouse.com/auth/token/dd"),
#endif

                Content = JsonContent.Create(new { body = encryptedToken }),
                Method = HttpMethod.Post,
            };
            var response = await client.SendAsync(httpRequestMessage);
            var responseString = await response.Content.ReadAsStringAsync();

            var responseJson = JsonConvert.DeserializeObject<ApiAuthResponse>(responseString);
            HandleResponse(responseJson);
        }

        public static void HandleResponse(ApiAuthResponse responseJson)
        {
            if (responseJson.Access == true)
            {
                string expiryDate = responseJson.Expires;
                //MessageBox.Show($"Subscription expires in {expiryDate} days.", "Authentication Success");
                Console.WriteLine($"[AUTH] Authentication Success. Subscription expires in {expiryDate} days.");

                //Offsets offsets = new(responseJson.Offsets);
                //while (!Offsets.Ready)
                //{
                //    Thread.Sleep(1);
                //}
                _isAuthenticated = true;
            }
            else
            {
                var message = responseJson.Message ?? "Authentication Failure";
                Console.WriteLine($"[AUTH] {message}");
                //MessageBox.Show(message, "Authentication Failure");
                _isAuthenticated = false;
            }
            _isAuthenticating = false;
        }

        private static string ReadTokenFromFile()
        {
            try
            {
                // check file exists
                string token = System.IO.File.ReadAllTextAsync("token.json").Result;
                return token;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static async void WriteTokenToFile(string token)
        {
            try
            {
                await File.WriteAllTextAsync("token.json", token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing token to file: {ex.Message}");
            }
        }
    }
}