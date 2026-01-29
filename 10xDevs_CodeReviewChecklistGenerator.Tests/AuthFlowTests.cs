namespace _10xDevs_CodeReviewChecklistGenerator.Tests;

public class AuthFlowTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AuthFlowTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HomePage_IsPublic()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("AI-driven code review checklists", html);
    }

    [Fact]
    public async Task ChecklistsPage_RequiresLogin()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Checklists/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task LoginPage_IsAccessible()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Identity/Account/Login");

        response.EnsureSuccessStatusCode();
    }
}
