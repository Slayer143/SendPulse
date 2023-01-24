using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace Terrasoft.Configuration.SendPulse.Core
{
    public class SendPulseRouter
    {
        private class SendPulseAuthRequestBody
        {
            public string grant_type {get; set; }
            public string client_id { get; set; }
            public string client_secret { get; set; }
        }

        private class SendPulseAuthResponseBody
        {
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string access_token { get; set; }
        }

        private string _result { get; set; }

        private string _route { get; set; }

        private string _sendPulseId { get; set; }

        private string _sendPulseSecret { get; set; }

        private string _sendPulseAuthToken { get; set; }

        public SendPulseRouter(string route, string sendPulseId, string sendPulseSecret, string sendPulseAuthToken)
        {
            _route = route;
            _sendPulseId = sendPulseId;
            _sendPulseSecret = sendPulseSecret;
            _sendPulseAuthToken = sendPulseAuthToken;
        }

        public void ChangeRoute (string route) => _route = route;

        public string GetAuthRequestResult()
        {
            var result = new SendPulseAuthResponseBody();

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_route);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(new SendPulseAuthRequestBody() 
                {
                    grant_type = "client_credentials",
                    client_id = _sendPulseId,
                    client_secret = _sendPulseSecret
                }));
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = JsonConvert.DeserializeObject<SendPulseAuthResponseBody>(streamReader.ReadToEnd());
            }

            _sendPulseAuthToken = result.access_token;
            return _sendPulseAuthToken;
        }

        public string GetRequestResult(string requestType, string body = "")
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(_route);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = requestType;
            httpWebRequest.Headers.Add("Authorization", $"Bearer {_sendPulseAuthToken}");

            if (body.Length > 0)
            {
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(body);
                }
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _result = streamReader.ReadToEnd();
            }

            return _result;
        }
    }
}