using System.Threading.Tasks;
using Microsoft.Playwright;

namespace TripPlanner.E2E.PageObjects;

public class NavMenu
{
    private readonly IPage _page;

    public NavMenu(IPage page) => _page = page;

    public ILocator HomeLink => _page.GetByRole(AriaRole.Link, new() { Name = "Home" });
    public ILocator TripsLink => _page.GetByRole(AriaRole.Link, new() { Name = "Trips" });
    public ILocator LoginLink => _page.GetByRole(AriaRole.Link, new() { Name = "Login" });
    public ILocator RegisterLink => _page.GetByRole(AriaRole.Link, new() { Name = "Register" });

    public Task ClickTripsAsync() => TripsLink.ClickAsync();
}