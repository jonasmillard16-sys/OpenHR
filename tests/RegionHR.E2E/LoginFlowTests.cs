// E2E tests require Playwright. Install with:
// dotnet add package Microsoft.Playwright.NUnit
// npx playwright install
//
// These tests verify the login flow, navigation, and form submission
// against a running instance of OpenHR.

namespace RegionHR.E2E;

// Uncomment when Playwright is installed:
// [Parallelizable(ParallelScope.Self)]
// [TestFixture]
// public class LoginFlowTests : PageTest
// {
//     [Test]
//     public async Task LoginPage_ShowsSITHSAndBankID()
//     {
//         await Page.GotoAsync("http://localhost:5076/login");
//         await Expect(Page.GetByText("SITHS-kort")).ToBeVisibleAsync();
//         await Expect(Page.GetByText("BankID")).ToBeVisibleAsync();
//     }
//
//     [Test]
//     public async Task Login_AsAdmin_ShowsAllMenuItems()
//     {
//         await Page.GotoAsync("http://localhost:5076/login");
//         await Page.GetByText("BankID").ClickAsync();
//         await Task.Delay(3000);
//         await Page.SelectOptionAsync("select", "Admin");
//         await Page.GetByRole(AriaRole.Button, new() { Name = "Logga in" }).ClickAsync();
//         await Expect(Page.GetByText("Dashboard")).ToBeVisibleAsync();
//     }
// }
