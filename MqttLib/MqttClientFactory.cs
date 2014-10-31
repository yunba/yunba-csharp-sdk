using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Configuration;
using System.Reflection;
using MqttLib.Core;
using MqttLib;
using MqttLib.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MqttLib
{
    public class MqttClientFactory
    {
        public static IMqtt CreateClient(string connString, string clientId, string username = null, string password = null, IPersistence persistence = null)
        {
            return new Mqtt(connString, clientId, username, password, persistence);
        }

        public static IMqtt CreateClientWithAppkey(string yunbaAppkey)
        {
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            RegInfo regInfo = new RegInfo();
            if (appConfig.AppSettings.Settings["username"] != null)
                regInfo.username = appConfig.AppSettings.Settings["username"].Value;

            if (appConfig.AppSettings.Settings["password"] != null)
                regInfo.password = appConfig.AppSettings.Settings["password"].Value;

            if (appConfig.AppSettings.Settings["client_id"] != null)
                regInfo.clientId = appConfig.AppSettings.Settings["client_id"].Value;

            if (regInfo.username == "" || regInfo.password == "" || regInfo.clientId == "")
            {
                try
                {
                    regInfo = GetRegInfoWithAppkey(yunbaAppkey);
                    if (regInfo.username == null || regInfo.password == null || regInfo.clientId == null)
                        throw new Exception("username or password or client_id is null.");
                }
                catch (Exception e)
                {
                    Log.Write(LogLevel.ERROR, e.ToString());
                    throw e;
                }

                if (appConfig.AppSettings.Settings["username"] == null)
                    appConfig.AppSettings.Settings.Add("username", regInfo.username);
                else
                    appConfig.AppSettings.Settings["username"].Value = regInfo.username;

                if (appConfig.AppSettings.Settings["password"] == null)
                    appConfig.AppSettings.Settings.Add("password", regInfo.password);
                else
                    appConfig.AppSettings.Settings["password"].Value = regInfo.password;

                if (appConfig.AppSettings.Settings["client_id"] == null)
                    appConfig.AppSettings.Settings.Add("client_id", regInfo.clientId);
                else
                    appConfig.AppSettings.Settings["client_id"].Value = regInfo.clientId;

                appConfig.Save(ConfigurationSaveMode.Full, true);
                ConfigurationManager.RefreshSection("appSettings");
            }

            string host = null;
            try
            {
                host = GetHostWithAppkey(yunbaAppkey);
                if (host == null)
                    throw new Exception("host is null.");
            }
            catch(Exception e)
            {
                Log.Write(LogLevel.ERROR, e.ToString());
                throw e;
            }

            /*
            Log.Write(LogLevel.INFO, "host: " + host);
            Log.Write(LogLevel.INFO, "client id: " + regInfo.clientId);
            */

            return new Mqtt(host, regInfo.clientId, regInfo.username, regInfo.password, null);
        }

        public static IMqttShared CreateSharedClient(string connString, string clientId, string username = null, string password = null)
        {
            return new Mqtt(connString, clientId, username, password, null);
        }

        public static IMqtt CreateBufferedClient(string connString, string clientId)
        {
            throw new NotImplementedException();
        }

        private static RegInfo GetRegInfoWithAppkey(string yunbaAppkey)
        {
            HttpClient httpClient = new HttpClient();

            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            JObject req = new JObject();
            req["a"] = yunbaAppkey;
            req["p"] = 2;
            string strReq = JsonConvert.SerializeObject(req);

            var resp = httpClient.PostAsync("http://reg.yunba.io:8383/device/reg/", new StringContent(strReq, Encoding.UTF8, "application/json"));
            resp.Result.EnsureSuccessStatusCode();

            JObject regInfo = (JObject)JsonConvert.DeserializeObject(resp.Result.Content.ReadAsStringAsync().Result);
            return new RegInfo { username = (string)regInfo["u"], password = (string)regInfo["p"], clientId = (string)regInfo["c"] };
        }

        private static string GetHostWithAppkey(string yunbaAppkey)
        {
            HttpClient httpClient = new HttpClient();

            // Limit the max buffer size for the response so we don't get overwhelmed
            httpClient.MaxResponseContentBufferSize = 256000;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            JObject req = new JObject();
            req["a"] = yunbaAppkey;
            req["n"] = "1";
            req["v"] = "v1.0.0";
            req["o"] = "1";
            string strReq = JsonConvert.SerializeObject(req);

            var resp = httpClient.PostAsync("http://tick.yunba.io:9999/", new StringContent(strReq, Encoding.UTF8, "application/json"));
            resp.Result.EnsureSuccessStatusCode();

            JObject hostInfo = (JObject)JsonConvert.DeserializeObject(resp.Result.Content.ReadAsStringAsync().Result);
            return (string)hostInfo["c"];
        }

        private class RegInfo
        {
            public string username = "";
            public string password = "";
            public string clientId = "";
        }
    }
}
