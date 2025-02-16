using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Net;

namespace Numbers.Web.Services
{
    public class RandomNumberService
    {
        private readonly IConfiguration _config;

        public string ApiUrl {get; private set;}

        public RandomNumberService(IConfiguration config)
        {
            _config = config;
            // allow for simple name to get around ACA secret issue: https://github.com/microsoft/azure-container-apps/issues/495
            ApiUrl = _config["RngApiUrl"];
            if (string.IsNullOrEmpty(ApiUrl))
            {
                ApiUrl = _config["RngApi:Url"];
            }
        }

        public int GetNumber()
        {
            var client = new RestClient(ApiUrl);
            var request = new RestRequest();
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                throw new Exception($"Service call failed, status: {response.StatusCode}, message: {response.ErrorMessage}");
            }
            return int.Parse(response.Content);
        }
    }
}
