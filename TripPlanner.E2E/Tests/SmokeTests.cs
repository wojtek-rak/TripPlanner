using System.Threading.Tasks;
using Microsoft.Playwright;
using TripPlanner.E2E.PageObjects;
using Xunit;

namespace TripPlanner.E2E.Tests;

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<AppFactory>, ICollectionFixture<PlaywrightFixture> { }

[Collection("E2E")]
public class SmokeTests
{
    private readonly AppFactory _factory;
    private readonly PlaywrightFixture _pw;

    public SmokeTests(AppFactory factory, PlaywrightFixture pw)
    {
        _factory = factory;
        _pw = pw;
    }

    [Fact]
    public async Task Home_ShowsBrandAndNavigation()
    {
        var context = await _pw.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = _factory.RootUri,
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync("/");

        var nav = new NavMenu(page);

        await Assertions.Expect(nav.HomeLink).ToBeVisibleAsync();
        await Assertions.Expect(nav.TripsLink).ToBeVisibleAsync();
        await Assertions.Expect(nav.LoginLink).ToBeVisibleAsync();
        await Assertions.Expect(nav.RegisterLink).ToBeVisibleAsync();

        await context.CloseAsync();
    }

    [Fact]
    public async Task Trips_RequiresAuthentication_RedirectsToLogin()
    {
        var context = await _pw.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = _factory.RootUri,
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync("/trips");

        // Blazor + Identity should end up at the login page for anonymous users
        await page.WaitForURLAsync("**/Account/Login*");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Exact = true }))
                        .ToBeVisibleAsync();

        await context.CloseAsync();
    }
}