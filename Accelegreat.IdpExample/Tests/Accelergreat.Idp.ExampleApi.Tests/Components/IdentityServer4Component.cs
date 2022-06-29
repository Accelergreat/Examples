using System.Text.Json.Nodes;
using Accelergreat.Idp.IdentityServer4;
using Accelergreat.Web;

namespace Accelergreat.Idp.ExampleApi.Tests.Components;

public class IdentityServer4Component : KestrelWebAppComponent<Startup>
{
    public JsonNode? IdpToken { get; set; }

    protected override async Task OnWebAppInitializedAsync()
    {
        var httpClient = CreateClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "/connect/token");
        var formData = new Dictionary<string, string>()
        {
            {"client_id", "accelergreat"},
            {"client_secret", "secret"},
            {"grant_type", "password"},
            {"username", "alice"},
            {"password", "alice"}
        };

        tokenRequest.Content = new FormUrlEncodedContent(formData);
        var tokenResponse = await httpClient.SendAsync(tokenRequest);
        var token = await tokenResponse.Content.ReadAsStringAsync();
        IdpToken = JsonNode.Parse(token);
    }
}