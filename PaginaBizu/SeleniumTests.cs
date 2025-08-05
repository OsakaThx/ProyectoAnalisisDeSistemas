using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;
using System;
using System.Threading;

public class SeleniumTests : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl = "https://localhost:5116"; // Update this with your app's URL

    public SeleniumTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");
        
        // Uncomment the line below to run in headless mode
        // options.AddArgument("--headless");
        
        _driver = new ChromeDriver(options);
    }

    [Fact]
    public void NavigateToCartThroughShop_ShouldDisplayCartPage()
    {
        try
        {
            // Arrange - Navigate to the home page
            _driver.Navigate().GoToUrl(_baseUrl);
            Thread.Sleep(2000); // Wait for page to load

            // Find and click the Shop link in the navbar
            var shopLink = _driver.FindElement(By.LinkText("Shop"));
            shopLink.Click();
            Thread.Sleep(2000);

            // Now find and click the Cart link in the navbar
            var cartLink = _driver.FindElement(By.CssSelector("a[href*='Cart']"));
            cartLink.Click();
            Thread.Sleep(2000);

            // Assert that we're on the cart page
            Assert.Contains("cart", _driver.Url.ToLower());
            Assert.True(_driver.PageSource.Contains("Shopping Cart") || 
                       _driver.PageSource.Contains("Your Cart"));
        }
        catch (Exception ex)
        {
            // Take a screenshot on failure
            var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
            screenshot.SaveAsFile("cart_navigation_error.png");
            throw new Exception($"Test failed: {ex.Message}");
        }
    }

    [Fact]
    public void Login_WithValidCredentials_ShouldSucceed()
    {
        try
        {
            // Arrange - Navigate to the login page
            _driver.Navigate().GoToUrl($"{_baseUrl}/Identity/Account/Login");
            Thread.Sleep(2000);

            // Find the email and password fields and login button
            var emailField = _driver.FindElement(By.Id("Input_Email"));
            var passwordField = _driver.FindElement(By.Id("Input_Password"));
            var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));

            // Act - Enter credentials and click login
            emailField.SendKeys("hoshuacastillo48@gmail.com"); // Replace with test user email
            passwordField.SendKeys("Joshua0905."); // Replace with test user password
            loginButton.Click();
            Thread.Sleep(2000);

            // Assert - Check if login was successful by looking for a logout button or user's name
            Assert.True(_driver.PageSource.Contains("Logout") || 
                       _driver.PageSource.Contains("Hello, ") ||
                       _driver.PageSource.Contains("Manage"));
        }
        catch (Exception ex)
        {
            // Take a screenshot on failure
            var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
            screenshot.SaveAsFile("login_test_error.png");
            throw new Exception($"Login test failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
