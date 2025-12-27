using Microsoft.Playwright;

class Program
{
    private static readonly TimeSpan WaitingTime = TimeSpan.FromHours(1);
    private const int InitialWaitingTime = 5000;
    private const string StorageKey = "storage.json";
    static async Task Main()
    {
        await CheckResale();
    }

    private static async Task CheckResale()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false
        });

        BrowserNewContextOptions options = new();

        if (File.Exists(StorageKey))
        {
            options.StorageStatePath = StorageKey;
        }

        var context = await browser.NewContextAsync(options);
        var page = await context.NewPageAsync();

        await page.GotoAsync("https://www.ticketmaster.dk/event/1279000466");

        if (!File.Exists(StorageKey))
        {
            try
            {
                await page.Locator("#onetrust-accept-btn-handler")
                    .ClickAsync(new() { Timeout = InitialWaitingTime });

                await context.StorageStateAsync(new()
                {
                    Path = StorageKey
                });
            }
            catch
            {
                // banner not shown
            }
        }

        // Click button by text
        var buttonName = "button:has-text(\"Se bedst ledige\")";
        var listName = "ul.sc-8ba94237-0.bTHTpA";

        await page.WaitForSelectorAsync(buttonName);
        await page.ClickAsync(buttonName);

        await page.WaitForSelectorAsync(listName);
        var count = await page.Locator(listName).Locator("li").CountAsync();
    }
}
