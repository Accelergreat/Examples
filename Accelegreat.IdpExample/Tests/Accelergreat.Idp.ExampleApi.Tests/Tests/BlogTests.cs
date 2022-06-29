using System.Net;
using System.Net.Http.Json;
using System.Text;
using Accelergreat.Environments.Pooling;
using Accelergreat.Idp.Database.Entities;
using Accelergreat.Idp.ExampleApi.Tests.Components;
using Accelergreat.Xunit;
using FluentAssertions;
using Newtonsoft.Json;

namespace Accelergreat.Idp.ExampleApi.Tests.Tests;

public class BlogTests : AccelergreatXunitTest
{

    public BlogTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    protected override Task InitializeAsync()
    {

        return base.InitializeAsync();
    }

    [Fact]
    public async Task CanAddANewBlog()
    {
        var idpComponent = GetComponent<IdentityServer4Component>();
        var dbComponent = GetComponent<DatabaseComponent>();
        var apiComponent = GetComponent<ExampleApiComponent>();

        var idpToken = idpComponent.IdpToken;

        var httpClient = apiComponent.CreateClient();
        var postRequest = new HttpRequestMessage(HttpMethod.Post, "/blogs");
        postRequest.Headers.Add("Authorization", $"Bearer {idpToken!["access_token"]}");
        var blogEntity = new Blog();
        var blogJson = JsonConvert.SerializeObject(blogEntity);
        postRequest.Content = new StringContent(blogJson, Encoding.UTF8, "application/json");
        var response = await httpClient.SendAsync(postRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("1");


    }
}