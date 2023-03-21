using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace ArchivingTelegramChatsAPI
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            using var playwright = await Playwright.CreateAsync();
            var chromePath = string.Empty;

            while (true)
            {
                Console.WriteLine("Enter the path to chrome.exe. Example : \"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\"");
                chromePath = Console.ReadLine();

                if (string.IsNullOrEmpty(chromePath))
                {
                    Console.Clear();
                    Console.WriteLine("You entered the wrong path to chrome.exe");
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("Do you want to see the browser window?");
            Console.WriteLine("Y - yes. N - no");

            var showBrowser = Console.ReadLine();
            var headlessValue = !(!string.IsNullOrEmpty(showBrowser) && showBrowser == "Y");

            try
            {
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    ExecutablePath = $"{chromePath}",
                    Headless = headlessValue
                });

                var page = await browser.NewPageAsync();
                await page.GotoAsync("https://web.telegram.org/z/");
                await page.WaitForTimeoutAsync(10000);

                await page.ClickAsync("//*[@type='button'][1]");
                await page.WaitForTimeoutAsync(2000);

                var phoneNumber = string.Empty;

                while (true)
                {
                    Console.WriteLine("Enter the phone number. Example : 380935851461");
                    phoneNumber = Console.ReadLine();

                    if (string.IsNullOrEmpty(phoneNumber))
                    {
                        Console.Clear();
                        Console.WriteLine("You entered the wrong phoneNumber");
                    }
                    else
                    {
                        break;
                    }
                }


                await page.FillAsync("input[id='sign-in-phone-number']", $"+{phoneNumber}");
                await page.WaitForTimeoutAsync(2000);

                await page.ClickAsync("button[type='submit']");
                await page.WaitForTimeoutAsync(2000);

                Console.WriteLine("Please enter the code ...");

                var code = string.Empty;

                while (true)
                {
                    code = Console.ReadLine();

                    if (string.IsNullOrEmpty(code))
                    {
                        Console.WriteLine("You must enter code!");
                    }
                    else
                    {
                        break;
                    }
                }

                await page.FillAsync("//*[@id='sign-in-code']", code);
                await page.WaitForTimeoutAsync(2000);

                var groups = await page.QuerySelectorAllAsync("//*[contains(@class,'ListItem') and contains(@class,'group')]");

                var groupHrefs = new List<string>();

                foreach (var group in groups)
                {
                    var groupBlockHtml = await group.InnerHTMLAsync();
                    var groupHref = Regex.Match(groupBlockHtml, "href=\"#-(\\d+)\"").Groups[1].Value;
                    groupHrefs.Add(groupHref);
                    await group.ClickAsync(new ElementHandleClickOptions { Button = MouseButton.Right });
                    await page.WaitForTimeoutAsync(2000);

                    await page.ClickAsync("//*[text() = 'Archive']");
                    await page.WaitForTimeoutAsync(2000);
                }

                await page.ClickAsync("//*[contains(@class,'chat-item-archive')]");
                await page.WaitForTimeoutAsync(2000);

                var querySelector = new StringBuilder("//*[");
                for (var i = 0; i < groupHrefs.Count; i++)
                {
                    querySelector.Append(i == groupHrefs.Count - 1
                        ? $"contains(@href,'{groupHrefs[i]}')"
                        : $"contains(@href,'{groupHrefs[i]}') and");
                }

                querySelector.Append("]");

                var groupsToFix = await page.QuerySelectorAllAsync(querySelector.ToString());

                foreach (var group in groupsToFix)
                {
                    var groupBlockHtml = await group.InnerHTMLAsync();
                    var groupHref = Regex.Match(groupBlockHtml, "href=\"#-(\\d+)\"").Groups[1].Value;
                    groupHrefs.Add(groupHref);
                    await group.ClickAsync(new ElementHandleClickOptions { Button = MouseButton.Right });
                    await page.WaitForTimeoutAsync(2000);

                    await page.ClickAsync("//*[text() = 'Pin to top']");
                    await page.WaitForTimeoutAsync(2000);
                }

                Console.WriteLine("Enter to close program");
                Console.ReadKey();

                await browser.CloseAsync();
            }
            catch
            {
                Console.WriteLine("Error. Please restart program");
            }
        }
    }
}