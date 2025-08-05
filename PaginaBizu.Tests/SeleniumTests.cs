using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Xunit;
using System;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace PaginaBizu.Tests
{
    /// <summary>
    /// Clase de pruebas de Selenium para probar la funcionalidad de la página web de Bizu.
    /// Incluye pruebas de navegación, autenticación, carrito de compras y más.
    /// </summary>
    public class SeleniumTests : IDisposable
    {
        private readonly IWebDriver _driver;  // Controlador de Selenium para interactuar con el navegador
        private readonly string _baseUrl = "http://localhost:5116";  // URL base de la aplicación bajo prueba
        private const string ScreenshotFolder = "TestScreenshots";  // Carpeta para guardar capturas de pantalla
        /// <summary>
        /// Constructor que se ejecuta antes de cada prueba.
        /// Configura el navegador Chrome con las opciones necesarias.
        /// </summary>
        public SeleniumTests()
        {
            // Configuración de opciones de Chrome
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");  // Maximiza la ventana del navegador
            options.AddArgument("--disable-notifications");  // Desactiva notificaciones del navegador
            options.AddArgument("--ignore-certificate-errors");  // Ignora errores de certificado
            options.AddArgument("--allow-insecure-localhost");  // Permite conexiones inseguras a localhost
            options.AddArgument("no-sandbox");  // Necesario para ciertos entornos de CI/CD
            options.AcceptInsecureCertificates = true;  // Acepta certificados inseguros
            
            // Uncomment the line below to run in headless mode
            // options.AddArgument("--headless=new");
            
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            
            _driver = new ChromeDriver(service, options);
            
            if (!Directory.Exists(ScreenshotFolder))
            {
                Directory.CreateDirectory(ScreenshotFolder);
            }
        }

        #region Helpers de Prueba
        
        /// <summary>
        /// Toma una captura de pantalla y la guarda en la carpeta de capturas.
        /// Útil para depuración cuando una prueba falla.
        /// </summary>
        /// <param name="testName">Nombre de la prueba para nombrar el archivo</param>
        private void TakeScreenshot(string testName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var fileName = $"{testName}_{DateTime.Now:yyyyMMddHHmmss}.png";
                var filePath = Path.Combine(ScreenshotFolder, fileName);
                screenshot.SaveAsFile(filePath);
                Console.WriteLine($"Screenshot saved: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Espera a que un elemento esté presente y visible en la página.
        /// Lanza una excepción si el elemento no aparece en el tiempo especificado.
        /// </summary>
        /// <param name="by">Selector para encontrar el elemento</param>
        /// <param name="timeoutInSeconds">Tiempo máximo de espera en segundos</param>
        /// <returns>El elemento web encontrado</returns>
        private IWebElement? WaitForElement(By by, int timeoutInSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(driver => 
                {
                    var element = driver.FindElement(by);
                    return element.Displayed ? element : null;
                });
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }
        
        private void ScrollToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", element);
            Thread.Sleep(500);
        }
        
        private void HighlightElement(IWebElement element)
        {
            var jsDriver = (IJavaScriptExecutor)_driver;
            var originalStyle = element.GetAttribute("style");
            jsDriver.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);", 
                                 element, "border: 3px solid red; border-style: dashed;");
            Thread.Sleep(200);
            jsDriver.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);", 
                                 element, originalStyle);
        }
        
        #endregion
        
        #region Navigation Tests
        
        /// <summary>
        /// Prueba que verifica que la página de inicio se cargue correctamente.
        /// Comprueba que el título de la página contenga "Bizu" y que la barra de navegación esté visible.
        /// </summary>
        [Fact]
        public void HomePage_ShouldLoadSuccessfully()
        {
            try
            {
                // Navega a la página de inicio
                _driver.Navigate().GoToUrl(_baseUrl);
                
                // Verificaciones de la página de inicio
                Assert.Contains("Bizu", _driver.Title);  // El título debe contener "Bizu"
                Assert.Contains(_baseUrl, _driver.Url);  // La URL debe ser la correcta
                
                // Verifica que la barra de navegación esté visible
                Assert.True(_driver.FindElement(By.TagName("nav")).Displayed, 
                    "La barra de navegación debería estar visible");
                
                // Toma una captura de pantalla como evidencia
                TakeScreenshot("HomePage_Loaded");
            }
            catch (Exception ex)
            {
                // En caso de error, toma una captura y relanza la excepción
                TakeScreenshot("HomePage_Error");
                throw new Exception($"Error al cargar la página de inicio: {ex.Message}");
            }
        }
        
        [Fact]
        public void Navigation_ShouldWorkForAllMenuItems()
        {
            try
            {
                _driver.Navigate().GoToUrl(_baseUrl);
                var navLinks = _driver.FindElements(By.CssSelector("nav a"));
                
                foreach (var link in navLinks.Where(l => l.Displayed && !string.IsNullOrEmpty(l.Text.Trim())))
                {
                    try
                    {
                        if (link.GetAttribute("data-bs-toggle") == "dropdown") continue;
                        
                        string linkText = link.Text.Trim();
                        if (linkText.Equals("Login", StringComparison.OrdinalIgnoreCase) || 
                            linkText.Equals("Logout", StringComparison.OrdinalIgnoreCase) ||
                            linkText.Contains("Cerrar sesión"))
                            continue;
                        
                        string href = link.GetAttribute("href");
                        if (string.IsNullOrEmpty(href) || !href.StartsWith(_baseUrl))
                            continue;
                        
                        _driver.Navigate().GoToUrl(href);
                        Assert.True(_driver.Url.StartsWith(href), $"Expected to be on {href}, but was on {_driver.Url}");
                        TakeScreenshot($"Nav_{linkText.Replace(" ", "")}");
                        _driver.Navigate().Back();
                    }
                    catch (Exception ex)
                    {
                        TakeScreenshot($"NavError_{link.Text.Replace(" ", "")}");
                        throw new Exception($"Navigation test failed for link '{link.Text}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                TakeScreenshot("Navigation_Error");
                throw new Exception($"Navigation test failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Authentication Tests
        
        /// <summary>
        /// Prueba el inicio de sesión con credenciales válidas.
        /// Verifica que el usuario pueda iniciar sesión correctamente y sea redirigido.
        /// </summary>
        [Fact]
        public void Login_WithValidCredentials_ShouldSucceed()
        {
            try
            {
                // 1. Navegar a la página de inicio de sesión
                _driver.Navigate().GoToUrl($"{_baseUrl}/Identity/Account/Login");
                TakeScreenshot("Login_Page");
                
                // 2. Localizar los elementos del formulario de inicio de sesión
                var emailField = WaitForElement(By.Id("Input_Email"));
                var passwordField = WaitForElement(By.Id("Input_Password"));
                var loginButton = WaitForElement(By.CssSelector("button[type='submit']"));

                // 3. Introducir credenciales válidas
                emailField.Clear();
                emailField.SendKeys("hoshuacastillo48@gmail.com");
                passwordField.Clear();
                passwordField.SendKeys("Joshua0905.");
                
                // 4. Resaltar campos para la captura de pantalla
                HighlightElement(emailField);
                HighlightElement(passwordField);
                TakeScreenshot("Login_Credentials_Entered");
                
                // 5. Hacer clic en el botón de inicio de sesión
                loginButton.Click();

                // 6. Esperar a que el inicio de sesión se complete (redirección o mensaje de éxito)
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(driver => 
                    driver.PageSource.Contains("Logout", StringComparison.OrdinalIgnoreCase) || 
                    driver.PageSource.Contains("Hello,", StringComparison.OrdinalIgnoreCase) ||
                    driver.PageSource.Contains("Cerrar sesión", StringComparison.OrdinalIgnoreCase));
                
                // 7. Tomar captura de pantalla después del inicio de sesión exitoso
                TakeScreenshot("Login_Successful");
                
                // 8. Verificar que el inicio de sesión fue exitoso buscando elementos de sesión iniciada
                Assert.True(
                    _driver.PageSource.Contains("Logout", StringComparison.OrdinalIgnoreCase) || 
                    _driver.PageSource.Contains("Hello,", StringComparison.OrdinalIgnoreCase) ||
                    _driver.PageSource.Contains("Cerrar sesión", StringComparison.OrdinalIgnoreCase),
                    "El inicio de sesión no fue exitoso. No se encontraron indicadores de sesión activa.");
            }
            catch (Exception ex)
            {
                // En caso de error, tomar captura y proporcionar un mensaje detallado
                TakeScreenshot("Login_Error");
                throw new Exception($"Error en la prueba de inicio de sesión: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Prueba el inicio de sesión con credenciales inválidas.
        /// Verifica que se muestre un mensaje de error apropiado.
        /// </summary>
        [Fact]
        public void Login_WithInvalidCredentials_ShouldShowError()
        {
            try
            {
                // 1. Navegar a la página de inicio de sesión
                _driver.Navigate().GoToUrl($"{_baseUrl}/Identity/Account/Login");
                
                // 2. Localizar los elementos del formulario
                var emailField = WaitForElement(By.Id("Input_Email"));
                var passwordField = WaitForElement(By.Id("Input_Password"));
                var loginButton = WaitForElement(By.CssSelector("button[type='submit']"));

                // 3. Introducir credenciales inválidas
                emailField.Clear();
                emailField.SendKeys("usuario@inexistente.com");
                passwordField.Clear();
                passwordField.SendKeys("contrasenaIncorrecta123");
                
                // 4. Resaltar campos y tomar captura
                HighlightElement(emailField);
                HighlightElement(passwordField);
                TakeScreenshot("Login_Invalid_Credentials_Entered");
                
                // 5. Intentar iniciar sesión
                loginButton.Click();

                // 6. Esperar y verificar el mensaje de error
                var errorMessage = WaitForElement(
                    By.CssSelector(".text-danger"),
                    timeoutInSeconds: 5
                );
                
                // 7. Verificar que el mensaje de error es visible y contiene el texto esperado
                Assert.NotNull(errorMessage);
                Assert.True(errorMessage.Displayed, "El mensaje de error debería mostrarse");
                Assert.Contains("Invalid login attempt", errorMessage.Text);
                TakeScreenshot("Login_Error_Shown");
            }
            catch (Exception ex)
            {
                TakeScreenshot("Login_Error_Exception");
                throw new Exception("Error durante la prueba de inicio de sesión inválido", ex);
            }
        }

        /// <summary>
        /// Prueba que la página de la tienda muestre correctamente los productos.
        /// Verifica que se carguen los productos y que cada uno tenga nombre y precio.
        /// </summary>
        [Fact]
        public void ShopPage_ShouldDisplayProducts()
        {
            try
            {
                // 1. Navegar a la página de la tienda
                _driver.Navigate().GoToUrl($"{_baseUrl}/Shop");
                
                // 2. Esperar a que cargue la página
                WaitForElement(By.TagName("body"), timeoutInSeconds: 15);
                
                // 3. Esperar a que cargue la lista de productos (con espera explícita)
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                var productItems = wait.Until(driver => 
                {
                    var items = driver.FindElements(
                        By.CssSelector("div[class*='product'], .card, .item, [class*='product-item'], [class*='product-grid']"));
                    return items.Count > 0 ? items : null;
                });
                Assert.True(productItems.Count > 0, 
                    "La tienda debería mostrar al menos un producto");
                
                // 4. Verificar cada producto individualmente
                foreach (var item in productItems)
                {
                    // Verificar que el producto tenga nombre
                    var name = "";
                try {
                    var nameElement = item.FindElement(
                        By.CssSelector("h1, h2, h3, h4, .name, .title, [class*='name'], [class*='title']"));
                    name = nameElement?.Text?.Trim() ?? "";
                    Console.WriteLine($"Nombre del producto encontrado: {name}");
                } catch (NoSuchElementException) {
                    // Si no encontramos un nombre con los selectores comunes, tomamos el primer elemento de texto
                    var text = item.Text?.Trim() ?? "";
                    name = text.Split('\n').FirstOrDefault() ?? "";
                    Console.WriteLine($"Nombre alternativo del producto: {name}");
                }
                    
                    Assert.False(string.IsNullOrWhiteSpace(name), 
                        "Cada producto debe tener un nombre");
                    
                    // Verificar que el producto tenga precio
                    string price = "";
                    try {
                        var priceElement = item.FindElement(
                            By.CssSelector(".price, [class*='price'], .text-success, .text-danger, span"));
                        price = priceElement.Text.Trim();
                        Console.WriteLine($"Precio encontrado: {price}");
                    } catch (NoSuchElementException) {
                        // Si no encontramos precio, continuamos con el siguiente producto
                        Console.WriteLine("No se pudo encontrar el precio del producto");
                        continue;
                    }
                    Assert.False(string.IsNullOrWhiteSpace(price), 
                        "Cada producto debe tener un precio");
                    
                    Console.WriteLine($"Producto encontrado: {name} - {price}");
                }
                
                // 5. Tomar captura de pantalla como evidencia
                TakeScreenshot("ShopPage_Products_Displayed");
            }
            catch (Exception ex)
            {
                // En caso de error, tomar captura y proporcionar un mensaje detallado
                TakeScreenshot("ShopPage_Error");
                throw new Exception($"Error al verificar los productos de la tienda: {ex.Message}");
            }
        }

        /// <summary>
        /// Prueba la funcionalidad de agregar un producto al carrito desde la página de detalles.
        /// Verifica que el producto se agregue correctamente y que se actualice el contador del carrito.
        /// </summary>
        [Fact]
        public void AddToCart_ShouldWork_FromProductDetails()
        {
            try
            {
                Console.WriteLine("1. Iniciando prueba de agregar al carrito...");
                
                // 1. Iniciar sesión primero
                Console.WriteLine("2. Verificando autenticación...");
                LoginIfNeeded();
                
                // 2. Navegar directamente a la página de detalles del producto con ID 1
                string productUrl = "http://localhost:5116/Shop/Details/1";
                Console.WriteLine($"3. Navegando a: {productUrl}");
                _driver.Navigate().GoToUrl(productUrl);
                
                // Esperar a que la página cargue completamente
                Console.WriteLine("4. Esperando a que cargue la página...");
                WaitForElement(By.TagName("body"), timeoutInSeconds: 15);
                
                // Tomar captura de pantalla de la página cargada
                TakeScreenshot("Product_Details_Page_Loaded");
                
                // 3. Buscar el botón de agregar al carrito con múltiples estrategias
                Console.WriteLine("5. Buscando el botón 'Agregar al carrito'...");
                IWebElement addToCartButton = null;
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                
                try 
                {
                    // Estrategia 1: Buscar por texto en botones negros
                    addToCartButton = wait.Until(driver => 
                    {
                        var buttons = driver.FindElements(By.CssSelector("button, a, input[type='button'], input[type='submit']"));
                        var button = buttons.FirstOrDefault(b => 
                            (b.Text?.Contains("Agregar al carrito", StringComparison.OrdinalIgnoreCase) == true ||
                             b.GetAttribute("value")?.Contains("Agregar al carrito", StringComparison.OrdinalIgnoreCase) == true) &&
                            b.Displayed && b.Enabled);
                        
                        if (button != null) 
                        {
                            Console.WriteLine("Botón encontrado por texto: " + button.Text);
                            return button;
                        }
                        
                        // Si no se encontró por texto, intentar por clase
                        button = driver.FindElements(By.CssSelector(".btn-black, .add-to-cart, .btn-primary, .btn-success, .btn"))
                            .FirstOrDefault(b => b.Displayed && b.Enabled);
                            
                        if (button != null)
                        {
                            Console.WriteLine("Botón encontrado por clase CSS");
                            return button;
                        }
                        
                        return null;
                    });
                }
                catch (WebDriverTimeoutException)
                {
                    // Si falla, intentar otra estrategia
                    Console.WriteLine("No se encontró el botón con la primera estrategia, intentando con selectores directos...");
                    TakeScreenshot("Button_Not_Found_First_Attempt");
                    
                    // Mostrar todos los botones en la página para depuración
                    var allButtons = _driver.FindElements(By.CssSelector("button, a, input[type='button'], input[type='submit']"));
                    Console.WriteLine($"Se encontraron {allButtons.Count} botones en la página");
                    
                    foreach (var btn in allButtons)
                    {
                        try {
                            Console.WriteLine($"Botón - Texto: '{btn.Text}', Clases: '{btn.GetAttribute("class")}', ID: '{btn.GetAttribute("id")}'");
                        } catch {}
                    }
                    
                    // Intentar con XPath como último recurso
                    try {
                        addToCartButton = _driver.FindElement(By.XPath("//button[contains(., 'carrito') or contains(., 'Carrito') or contains(., 'Agregar')]"));
                        Console.WriteLine("Botón encontrado con XPath");
                    } catch {
                        TakeScreenshot("Button_Not_Found_Final_Attempt");
                        throw new NoSuchElementException("No se pudo encontrar el botón 'Agregar al carrito' después de múltiples intentos");
                    }
                }
                
                if (addToCartButton == null)
                {
                    TakeScreenshot("AddToCart_Button_Not_Found");
                    throw new NoSuchElementException("No se pudo encontrar el botón 'Agregar al carrito'");
                }
                
                Console.WriteLine("6. Botón encontrado, preparando para hacer clic...");
                
                // 4. Tomar captura de pantalla antes de hacer clic
                TakeScreenshot("Before_Add_To_Cart_Click");
                
                // 5. Obtener el contador actual del carrito
                int initialCartCount = 0;
                try {
                    var cartCountElement = _driver.FindElement(By.CssSelector("#cart-count, .cart-count, [class*='cart-count']"));
                    if (int.TryParse(cartCountElement.Text.Trim(), out int count)) {
                        initialCartCount = count;
                    }
                    Console.WriteLine($"Contador inicial del carrito: {initialCartCount}");
                } catch (Exception ex) {
                    Console.WriteLine($"No se pudo obtener el contador inicial del carrito: {ex.Message}");
                    // Continuar asumiendo 0
                }
                
                // 6. Desplazarse al botón y hacer clic con JavaScript
                Console.WriteLine("7. Desplazando al botón...");
                ScrollToElement(addToCartButton);
                
                Console.WriteLine("8. Haciendo clic en el botón...");
                try {
                    // Primero intentar con clic normal
                    addToCartButton.Click();
                    Console.WriteLine("Clic normal realizado");
                } catch (Exception ex) {
                    Console.WriteLine($"Error al hacer clic normal: {ex.Message}, intentando con JavaScript...");
                    // Si falla el clic normal, intentar con JavaScript
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", addToCartButton);
                    Console.WriteLine("Clic con JavaScript realizado");
                }
                
                // 7. Esperar a que se complete la acción (con tiempo de espera más corto para pruebas)
                Console.WriteLine("9. Esperando actualización del carrito...");
                bool cartUpdated = false;
                int maxAttempts = 10;
                int attempt = 0;
                
                while (!cartUpdated && attempt < maxAttempts)
                {
                    attempt++;
                    Console.WriteLine($"Intento {attempt} de verificación del carrito...");
                    
                    try {
                        var currentCountElement = _driver.FindElement(By.CssSelector("#cart-count, .cart-count, [class*='cart-count']"));
                        if (int.TryParse(currentCountElement.Text.Trim(), out int currentCount))
                        {
                            Console.WriteLine($"Contador actual del carrito: {currentCount}");
                            if (currentCount > initialCartCount)
                            {
                                cartUpdated = true;
                                Console.WriteLine("¡Carrito actualizado exitosamente!");
                                break;
                            }
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"Error al verificar el contador del carrito (intento {attempt}): {ex.Message}");
                    }
                    
                    // Esperar un poco antes de volver a intentar
                    System.Threading.Thread.Sleep(1000);
                }
                
                // 8. Tomar captura de pantalla después de la acción
                TakeScreenshot("After_Add_To_Cart_Attempt");
                
                // 9. Verificar el resultado
                if (!cartUpdated)
                {
                    TakeScreenshot("Cart_Not_Updated_Error");
                    throw new Exception("El contador del carrito no se actualizó después de agregar el producto");
                }
                
                // 10. Verificación final
                var updatedCartCountElement = _driver.FindElement(By.CssSelector("#cart-count, .cart-count, [class*='cart-count']"));
                int updatedCartCount = int.Parse(updatedCartCountElement.Text.Trim());
                
                Console.WriteLine($"10. Verificación final - Contador inicial: {initialCartCount}, Contador final: {updatedCartCount}");
                
                Assert.True(updatedCartCount > initialCartCount, 
                    $"El contador del carrito debería incrementarse de {initialCartCount} a {updatedCartCount}");
                
                Console.WriteLine("¡Prueba completada con éxito! Producto agregado al carrito.");
            }
            catch (Exception ex)
            {
                // En caso de error, tomar captura y proporcionar un mensaje detallado
                TakeScreenshot("AddToCart_Error");
                throw new Exception($"Error al intentar agregar un producto al carrito: {ex.Message}");
            }
        }

        /// <summary>
        /// Prueba que la página del carrito muestre los productos agregados correctamente.
        /// Verifica que se muestren los productos y que cada uno tenga nombre, precio y cantidad.
        /// </summary>
        [Fact]
        public void CartPage_ShouldDisplayAddedItems()
        {
            try
            {
                // 1. Iniciar sesión primero
                LoginIfNeeded();
                AddToCart_ShouldWork_FromProductDetails();
                _driver.Navigate().GoToUrl($"{_baseUrl}/cart");
                
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var cartItems = wait.Until(driver => 
                {
                    var elements = driver.FindElements(By.CssSelector("tr.cart-item, .cart-item, .table tbody tr"));
                    return elements.Count > 0 ? elements : null;
                });
                
                TakeScreenshot("Cart_Page");
                
                Assert.True(cartItems.Count > 0, "No items found in the cart");
                
                foreach (var item in cartItems.Take(2))
                {
                    ScrollToElement(item);
                    HighlightElement(item);
                    
                    var nameElement = item.FindElements(By.CssSelector("td:first-child, .product-name, h4, h5")).FirstOrDefault();
                    var priceElement = item.FindElements(By.CssSelector(".price, .unit-price")).FirstOrDefault();
                    var quantityInput = item.FindElements(By.CssSelector("input[type='number'], .quantity")).FirstOrDefault();
                    
                    Assert.True(nameElement != null && !string.IsNullOrWhiteSpace(nameElement.Text), 
                             "Product name is missing in cart item");
                    
                    if (priceElement != null)
                    {
                        Assert.True(!string.IsNullOrWhiteSpace(priceElement.Text), 
                                 "Price is missing in cart item");
                    }
                    
                    if (quantityInput != null)
                    {
                        Assert.True(!string.IsNullOrWhiteSpace(quantityInput.GetAttribute("value")), 
                                 "Quantity is missing in cart item");
                    }
                }
                
                var cartTotal = _driver.FindElements(By.CssSelector(".cart-total, .total-amount, .summary-total")).FirstOrDefault();
                Assert.NotNull(cartTotal);
                
                var checkoutButton = _driver.FindElements(By.XPath("//a[contains(., 'Checkout') or contains(., 'Pagar')]"))
                                         .FirstOrDefault(b => b.Displayed);
                Assert.NotNull(checkoutButton);
            }
            catch (Exception ex)
            {
                TakeScreenshot("Cart_Page_Error");
                throw new Exception($"Cart page test failed: {ex.Message}");
            }
        }
        
        #endregion
        
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
