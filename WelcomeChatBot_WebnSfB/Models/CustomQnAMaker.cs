using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;

namespace WelcomeChatBot_WebnSfB.Models
{
    public class CustomQnAMaker
    {
        public static async Task<string> GetResultAsync(string messageText)
        {
            string endpoint = ConfigurationManager.AppSettings["QnAEndpointHostName"] + "/knowledgebases/" + ConfigurationManager.AppSettings["QnAKnowledgebaseId"] + "/generateAnswer";
            string input_json = "{\"question\":\"" + messageText + "\",\"top\": \"5\"}";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("EndpointKey", ConfigurationManager.AppSettings["QnAAuthKey"]);
                    request.Content = new StringContent(input_json, Encoding.UTF8, "application/json");

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            return json;

                        }

                        string failure = "failure";
                        return failure;
                    }
                }
            }
        }
    }
}