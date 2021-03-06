﻿using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common;
using FrontendServices.Models;
using Newtonsoft.Json;
using ProjectManagement.Domain;
using Image = Common.Image;

namespace IntegrationControllerTests.Helpers
{
    public static class RequestHelper
    {
        public static async Task<HttpResponseMessage> LoginToAdminAsync()
        {
            var account = Settings.ExistantAdminAccount;
            var credentials = new Credentials
            {
                Email = account.Email.Address,
                Password = account.Password.Value
            };

            var content = JsonConvert.SerializeObject(credentials);
            var responseMessage = await RequestHelper.Client.PostAsync(
                GetEndpointAddress("login"),
                new StringContent(content) { Headers = { ContentType = new MediaTypeHeaderValue(@"text/json") } }
                );

            return responseMessage;
        }

        public static async Task<HttpResponseMessage> CreateProjectAsync()
        {
            var client = new HttpClient();
            var project = new ProjectActionRequest()
            {
                Name = "TypicalNamsdfse",
                AccessLevel = AccessLevel.Public,
                Info = "TypicalInfo",
                LandingImage = new Image(
                    new Uri("https://pp.vk.me/c543107/v543107881/af01/zxFX1YLyVOE.jpg"),
                    new Uri("https://pp.vk.me/c543107/v543107881/af01/zxFX1YLyVOE.jpg")),
                ProjectTypes = new[] { ProjectType.Game, ProjectType.MobileApp, }
            };
            var response =
                await client.PostAsync(
                    new Uri(Settings.EndpointUri, "projects"),
                    project,
                    new JsonMediaTypeFormatter());
            return response;
        }

        public static HttpClient Client => new HttpClient();

        public static Uri GetEndpointAddress(string address)
        {
            return new Uri(Settings.EndpointUri, address);
        }
    }
}