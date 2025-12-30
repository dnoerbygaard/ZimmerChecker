using Microsoft.Playwright;
using System.Text;
using System.Text.Json;

class Program
{
    private const string ApiKey = "8086739530:AAEHU3CZYTvXBbS9-b90sXaOS2f6z49JWgs";
    private const string ChatId = "8438288554";
    private static string Url => $"https://api.telegram.org/bot{ApiKey}/sendMessage?chat_id={ChatId}";
    static async Task Main()
    {
        await CheckResale();
    }

    private static async Task CheckResale()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });

        BrowserNewContextOptions options = new();

        var context = await browser.NewContextAsync(options);
        var page = await context.NewPageAsync();

        await page.GotoAsync("https://www.ticketmaster.dk/event/1279000466");

        var consent = page.Locator("#onetrust-accept-btn-handler");
        await Task.Delay(5000);

        if (await consent.IsVisibleAsync())
        {
            await consent.ClickAsync();
        }

        // Click button by text
        var buttonName = "button:has-text(\"Se bedst ledige\")";
        var listName = "ul.sc-8ba94237-0.bTHTpA";

        await page.WaitForSelectorAsync(buttonName);
        await page.ClickAsync(buttonName);

        await page.WaitForSelectorAsync(listName);
        var count = await page.Locator(listName).Locator("li").CountAsync();

        if (count > 0)
        {
            await Notify();
        }
    }

    private static async Task Notify()
    {
        var payload = new
        {
            text = "Resale available"
        };

        using var client = new HttpClient();
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        await client.PostAsync(Url, content);
    }
}
