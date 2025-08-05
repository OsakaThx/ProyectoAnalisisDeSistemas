using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Xunit;
using System;
using System.Threading;

namespace PaginaBizu.Tests
{
    public class SeleniumTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl = "http://localhost:5116"; 
        public SeleniumTests()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--allow-insecure-localhost");
            options.AddArgument("no-sandbox");
            options.AcceptInsecureCertificates = true;
            
            // Uncomment the line below to run in headless mode
            // options.AddArgument("--headless=new");
            
            // Configurar el servicio de ChromeDriver
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            
            _driver = new ChromeDriver(service, options);
        }

        [Fact]
        public void NavigateToCartThroughShop_ShouldDisplayCartPage()
        {
            try
            {
                Console.WriteLine("Iniciando prueba de navegación al carrito...");
                
                // Asegurarse de estar autenticado
                LoginIfNeeded();
                
                // Navegar directamente a la página de tienda
                Console.WriteLine("Navegando a la página de tienda...");
                _driver.Navigate().GoToUrl($"{_baseUrl}/shop");
                
                // Esperar a que cargue la página de tienda
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
                
                // Tomar una captura de pantalla para depuración
                try {
                    var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                    screenshot.SaveAsFile("shop_page.png");
                    Console.WriteLine("Captura de pantalla guardada como shop_page.png");
                } catch (Exception ex) {
                    Console.WriteLine($"No se pudo tomar captura de pantalla: {ex.Message}");
                }
                
                // Hacer scroll para asegurar que los elementos sean visibles
                ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 0);");
                Thread.Sleep(1000);
                
                // Intentar hacer clic en el botón de carrito en la barra de navegación
                Console.WriteLine("Buscando enlace al carrito...");
                IWebElement cartLink = null;
                string[] cartSelectors = {
                    "//a[contains(., 'Cart')]",
                    "//a[contains(@href, 'cart')]",
                    "//a[contains(@href, 'Cart')]",
                    "//a[contains(., 'Carrito')]",
                    "//a[contains(., 'CARRITO')]"
                };
                
                foreach (var selector in cartSelectors)
                {
                    try
                    {
                        Console.WriteLine($"Probando selector: {selector}");
                        var elements = _driver.FindElements(By.XPath(selector));
                        Console.WriteLine($"Elementos encontrados: {elements.Count}");
                        
                        foreach (var element in elements)
                        {
                            try
                            {
                                if (element.Displayed && element.Enabled)
                                {
                                    cartLink = element;
                                    Console.WriteLine($"Enlace al carrito encontrado con texto: {element.Text}");
                                    break;
                                }
                            }
                            catch (StaleElementReferenceException)
                            {
                                Console.WriteLine("Elemento obsoleto, continuando con el siguiente...");
                                continue;
                            }
                        }
                        
                        if (cartLink != null) break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error con el selector {selector}: {ex.Message}");
                    }
                }
                
                if (cartLink == null)
                {
                    // Tomar captura de pantalla del error
                    try {
                        var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                        screenshot.SaveAsFile("cart_not_found.png");
                        Console.WriteLine("Captura de pantalla del error guardada como cart_not_found.png");
                    } catch {}
                    
                    throw new NoSuchElementException("No se pudo encontrar ningún enlace al carrito visible y habilitado");
                }
                
                // Desplazarse al elemento y hacer clic con JavaScript
                Console.WriteLine("Haciendo clic en el enlace del carrito...");
                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                    Thread.Sleep(1000); // Esperar a la animación
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].style.border='3px solid red';", cartLink); // Resaltar el elemento
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cartLink);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al hacer clic en el carrito: {ex.Message}");
                    throw;
                }
                
                Console.WriteLine("Verificando la página del carrito...");
                
                // Esperar a que se cargue la página del carrito
                wait.Until(driver => 
                    driver.Url.ToLower().Contains("cart") || 
                    driver.PageSource.ToLower().Contains("shopping cart") || 
                    driver.PageSource.ToLower().Contains("your cart"));
                
                // Verificar que estamos en la página del carrito
                Assert.True(
                    _driver.Url.ToLower().Contains("cart") ||
                    _driver.PageSource.ToLower().Contains("shopping cart") ||
                    _driver.PageSource.ToLower().Contains("your cart"),
                    "No se pudo confirmar que estamos en la página del carrito");
                
                Console.WriteLine("Prueba completada exitosamente");
            }
            catch (Exception ex)
            {
                // Take a screenshot on failure
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                screenshot.SaveAsFile($"cart_navigation_error_{timestamp}.png");
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
                emailField.SendKeys("hoshuacastillo48@gmail.com");
                passwordField.SendKeys("Joshua0905.");
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

        private void LoginIfNeeded()
        {
            try
            {
                Console.WriteLine("Verificando si es necesario iniciar sesión...");
                
                // Navegar a la página de inicio para verificar si ya estamos autenticados
                _driver.Navigate().GoToUrl(_baseUrl);
                
                // Esperar a que la página cargue completamente
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                
                // Verificar si aparece el botón de cerrar sesión (ya estamos autenticados)
                try
                {
                    var logoutButton = wait.Until(driver => 
                        driver.FindElements(By.LinkText("Cerrar sesión"))
                              .FirstOrDefault(e => e.Displayed));
                    
                    if (logoutButton != null)
                    {
                        Console.WriteLine("Usuario ya autenticado, continuando...");
                        return; // Ya estamos autenticados
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("Usuario no autenticado, procediendo con inicio de sesión...");
                }

                // Si llegamos aquí, necesitamos iniciar sesión
                _driver.Navigate().GoToUrl($"{_baseUrl}/Identity/Account/Login");
                
                // Esperar a que cargue la página de login
                wait.Until(driver => driver.FindElement(By.Id("Input_Email")).Displayed);
                
                // Llenar el formulario de login
                var emailField = _driver.FindElement(By.Id("Input_Email"));
                var passwordField = _driver.FindElement(By.Id("Input_Password"));
                var loginButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                
                Console.WriteLine("Rellenando credenciales...");
                emailField.Clear();
                emailField.SendKeys("hoshuacastillo48@gmail.com");
                passwordField.Clear();
                passwordField.SendKeys("Joshua0905.");
                
                Console.WriteLine("Haciendo clic en el botón de inicio de sesión...");
                loginButton.Click();
                
                // Esperar a que termine el inicio de sesión (redirección a la página principal)
                wait.Until(driver => !driver.Url.Contains("Identity/Account/Login", StringComparison.OrdinalIgnoreCase));
                Console.WriteLine("Inicio de sesión exitoso");
                
                // Pequeña pausa para asegurar que todo esté cargado
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el inicio de sesión: {ex.Message}");
                throw;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _driver.Quit();
                }
                finally
                {
                    _driver.Dispose();
                }
            }
        }
    }
}
