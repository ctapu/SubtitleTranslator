/*using System.IO;
using System.Net;

namespace SubtitleTranslator
{
    public static class GoogleTranslator
    {
        private const string ApiUrl = "https://translation.googleapis.com/language/translate/v2";

        public static string Translate(string text, string target)
        {
            var translatedText = Post(text, target);
            return translatedText;
        }

        private static string Post(string text, string target)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(ApiUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = "{\"q\":\"" + text + "," +
                                    "\"target\":" + target + "}";

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string result;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }
    }
}*/