// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Samples.DependencyInjectionScopes
{
    public class SampleFunction
    {
        private readonly MyServiceA _serviceA;
        private readonly MyServiceB _serviceB;
        private readonly IGlobalIdProvider _globalIdProvider;
        private readonly IHttpClientFactory _clientFactory;

        public SampleFunction(MyServiceA serviceA, MyServiceB serviceB, IGlobalIdProvider globalIdProvider, IHttpClientFactory clientFactory)
        {
            _serviceA = serviceA ?? throw new ArgumentNullException(nameof(serviceA));
            _serviceB = serviceB ?? throw new ArgumentNullException(nameof(serviceB));
            _globalIdProvider = globalIdProvider ?? throw new ArgumentNullException(nameof(globalIdProvider));
            _clientFactory = clientFactory;
        }

        [FunctionName("SampleFunction")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Service A ID: {_serviceA.IdProvider.Id}. Service B ID: {_serviceB.IdProvider.Id}");

            var httpClient = _clientFactory.CreateClient();
            var response = await httpClient.GetAsync("http://en.wikipedia.org/");
            string responseHtml = await response.Content.ReadAsStringAsync();

            return new OkObjectResult(new
            {
                ProvidersMatch = ReferenceEquals(_serviceA.IdProvider, _serviceB.IdProvider),
                GlobalId = _globalIdProvider.Id,
                ServiceAId = _serviceA.IdProvider.Id,
                ServiceBId = _serviceB.IdProvider.Id,
                HttpContent = responseHtml
            });
        }
    }
}
