using DotnetMessageBusHub;
using DotnetSharedEntities;
using DotnetSharedEntities.ConfigurationModels;
using DotnetSharedEntities.MessageBusModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Text.Json;
using System.Web;
using Xunit;

namespace ContainerOrchestrationValidator;

public class BusGatewayHealthValidator
{


    [Fact]
    public async Task BusGatewayRunning()
    {
        IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile(Path.GetFullPath("../../BentoConfiguration.json"))
        .Build();

        Service service = ConfigurationHandler.GetServiceFromConfiguration(configuration, "BusGateway");  

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        var client = new HttpClient(handler);
        string url = $"https://{service.Configuration["HostSettings:BaseUrl"]}:{service.Configuration["HostSettings:HttpsPort"]}/HealthCheck";
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.Add("AuthCode", service.Configuration["Authentication:AuthCode"]);
        HttpResponseMessage response = await client.SendAsync(requestMessage);

        Assert.True(response.EnsureSuccessStatusCode().IsSuccessStatusCode);
    }
}