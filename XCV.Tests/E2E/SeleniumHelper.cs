using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace XCV.Tests.E2E
{
    public static class SeleniumHelper
    {
        public static IWebDriver GetWebDriver<TWebDriver>() where TWebDriver : IWebDriver, new()
        {
            var isPipelineActive = Environment.MachineName.Contains("runner"); 
            if (typeof(TWebDriver) == typeof(ChromeDriver))
            {
                var chromeOptions = new ChromeOptions();
                if (isPipelineActive)
                {
                    chromeOptions.AddArgument("--headless");
                    chromeOptions.AddArgument("--verbose");
                    chromeOptions.AddArgument("--whitelisted-ips");
                    chromeOptions.AddArgument("--disable-dev-shm-usage");
                    chromeOptions.AddArgument("--no-sandbox");
                }

                return new ChromeDriver(chromeOptions);
            }
            else
            {
                var firefoxOptions = new FirefoxOptions()
                {
                    AcceptInsecureCertificates = true,
                };
                if (isPipelineActive)
                {
                    firefoxOptions.AddArgument("--headless");
                    firefoxOptions.AddArgument("--verbose");
                    firefoxOptions.AddArgument("--whitelisted-ips");
                }

                return new FirefoxDriver(firefoxOptions);
            }
        }

        /// <summary>
        /// Login of the given user. Page after execution will be /employeepage.
        /// </summary>
        /// <param name="driver">The webdriver</param>
        /// <param name="username">The username of the user to sign in</param>
        public static void Login(IWebDriver driver, string username)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));
            driver.Navigate().GoToUrl("https://localhost:5001");

            wait.Until(d =>
                d.FindElement(
                    By.XPath("/html/body/div[1]/div[2]/div/div[1]/div/div[2]/form/div[1]/div/input")));
            var elementUsername =
                driver.FindElement(
                    By.XPath("/html/body/div[1]/div[2]/div/div[1]/div/div[2]/form/div[1]/div/input"));
            elementUsername.SendKeys(username);
            wait.Until(d =>
                d.FindElement(
                    By.XPath("/html/body/div[1]/div[2]/div/div[1]/div/div[2]/form/div[2]/div/input")));
            var elementPwd =
                driver.FindElement(
                    By.XPath("/html/body/div[1]/div[2]/div/div[1]/div/div[2]/form/div[2]/div/input"));
            elementPwd.SendKeys("passwort");
            wait.Until(d =>
                d.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/div/div[2]/form/div[3]/button[1]")));
            var elementButton =
                driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/div/div[2]/form/div[3]/button[1]"));
            elementButton.Click();
                
            wait.Until(d =>
                d.FindElement(By.Id("edit-button")));
        }
    }
}