using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace TripPlanner.E2E;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright PW { get; private set; } = default!;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        PW = await Playwright.CreateAsync();
        Browser = await PW.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }
        PW?.Dispose();
    }
}