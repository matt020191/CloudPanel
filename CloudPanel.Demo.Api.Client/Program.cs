//
// Demo client communicating with the CloudPanel API
// 
// Jacob Dixon 12/3/2014
//

using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Exchange;
using RestSharp;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Demo.Api.Client
{
    class Program
    {
        // Base URL to CloudPanel web application. HTTPS is strongly recommended
        const string BaseUrl = "http://lab-dc/CloudPanel";

        // Token retrieved after you authenticate. Once your client has received a token, it must send the token as an HTTP header with each subsequent request.
        // Authorization: Token {your-token-goes-here}
        static string LoginToken = "";

        static void Main(string[] args)
        {
            // Login to CloudPanel to retrieve your token
            Login("administrator@lab.local", "Password1");

            GetResellers();

            Console.ReadKey();
        }

        static void Login(string username, string password)
        {
            var client = new RestClient(BaseUrl);

            var request = new RestRequest("/login/token", Method.POST);
            request.AddParameter("Username", "administrator@lab.local");
            request.AddParameter("Password", "Password1");

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var deserialize = new JsonDeserializer(); // Return for logging in will be Dictionary<string, string>
                var JSONObj = deserialize.Deserialize<Dictionary<string, string>>(response);
                LoginToken = JSONObj["token"]; // Token will be lower case "token"
            }
            else
            {
                throw response.ErrorException == null ? new Exception("Unknown Error") : response.ErrorException;
            }
        }

        static void GetResellers()
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("admin/settings/exchange/databases", Method.GET);
            request.AddHeader("Authorization", "Token " + LoginToken);

            IRestResponse<List<Databases>> response = client.Execute<List<Databases>>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                foreach (var m in response.Data)
                {
                    
                }
            }
        }
    }
}
