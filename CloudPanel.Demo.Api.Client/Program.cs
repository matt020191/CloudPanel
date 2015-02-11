//
// Demo client communicating with the CloudPanel API
// 
// Jacob Dixon 1/16/2015
//

using CloudPanel.Base.Models.Database;
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

        // Your API Key
        static string ApiKey = "{jJAf4tA4];%d)!Q7`geQ6V1j0oa1Lxu1]V8HWw*lg)RSo3ED6_L7W2v4p)Ek71";

        static void Main(string[] args)
        {
            GetUsers();
            Console.ReadKey();
        }

        static void GetUsers()
        {
            var client = new RestClient(BaseUrl);
            var request = new RestRequest("company/COM1/users", Method.GET);
            request.AddParameter("ApiKey", ApiKey); // Add the API Key as a querystring parameter for each request

            var response = client.Execute<Users>(request);
            foreach (var u in response.Data.data)
            {
                Console.WriteLine(u.DisplayName);
            }
        }
    }
}
