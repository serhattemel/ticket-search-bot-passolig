namespace Selenium_Ticket_App
{
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;
    using SeleniumExtras.WaitHelpers;
    using OpenQA.Selenium.Interactions;
    using System;
    using System.Net.Mail;
    using System.Net;

    class Program
    {
        //passo.com login info
        public static string email = "your_mail_address";
        public static string password = "your_password";
        
        //sometimes tickect transfers can be enabled or disabled you have to describe it
        public static bool ticketTransfer = false;
        
        //passo.com game url
        public static string gameUrl = "https://www.passo.com.tr/tr/etkinlik/futbol-besiktas-fenerbahce-mac-bileti-istanbul-besiktas-park/5785319";

        //passo.com ticket category. Most expensive category is 1 and goes to cheapest category
        public static string firstCategory = "12: Object"; //chech options value attribute from under the Kategori 
        public static string secondCategory = "11: Object"; //chech options value attribute from under the Kategori 
        
        static void Main(string[] args)
        {
            int counter = 1;
            var driver = new ChromeDriver();
            driver.Url = gameUrl;

            string buyBtnXpath = "//button[text()= 'SATIN AL']";
            string emailInputXpath = "//input[@placeholder='E-Posta Adresi']";
            string passwordInputXpath = "//input[@placeholder='Şifre']";
            string capchaInputXpath = "//input[@placeholder='KOD']";
            string categoryInputXpath = "/html/body/app-root/app-layout/app-seat/div/div[3]/div/div[2]/div[3]/div[3]/div/select";
            string cancelBtnXpath = "//button[text()= 'Tamam']";
            string quantityOptionXpath = "/html/body/app-root/app-layout/app-seat/div/div[3]/div/div[2]/div[4]/div[3]/div/div[2]/div/div/div/select"; // or //select[@class='form-control']
            string seatBlockOptionXpath = "//select[@id='blocks']";
            string selectBestSeatBtnXpath = "//button[@id='best_available_button']";
            string warningBlockButton = "//button[normalize-space()='Tamam']";


            //Maximaze the window
            driver.Manage().Window.Maximize();

            //wait until finds the buy button element 
            WaitUntilFindElement(driver, buyBtnXpath);

            //create an action object
            Actions actions = new(driver);

            //click the buy button until the page changes
            string currentUrl = driver.Title;
            do
            {
                //scrolls to window for the buy button to be visible
                var jsScrollScript = @"window.scrollBy(0,450)";
                driver.ExecuteScript(jsScrollScript);

                actions.Click(driver.FindElement(By.XPath(buyBtnXpath))).Build().Perform();
                Console.WriteLine(driver.Title);
            } while (currentUrl == driver.Title);

            //wait until finds the email input element and send the email address
            WaitUntilFindElement(driver, emailInputXpath);
            driver.FindElement(By.XPath(emailInputXpath)).SendKeys(email);

            //wait until finds the password input element and send the password
            WaitUntilFindElement(driver, passwordInputXpath);
            driver.FindElement(By.XPath(passwordInputXpath)).SendKeys(password);

            WaitUntilFindElement(driver, capchaInputXpath);
            driver.FindElement(By.XPath(capchaInputXpath)).SendKeys(Console.ReadLine());
            driver.FindElement(By.XPath(capchaInputXpath)).SendKeys(Keys.Enter);

            WaitUntilFindElement(driver, categoryInputXpath);
            bool TicketIsEnable = false;

            //Loop continues until find the ticket
            while (TicketIsEnable == false)
            {
                if (counter % 2 == 1)
                {
                    SelectTicket(driver, categoryInputXpath, firstCategory);
                }
                else
                {
                    SelectTicket(driver, categoryInputXpath, secondCategory);
                }
                
                TicketIsEnable = !IsElementPresent(driver, cancelBtnXpath, counter);
                if(TicketIsEnable==true)
                {
                    break;
                }
                pressKey(driver, "Keys.Escape");
                counter++;
                Console.WriteLine(counter);

            };

            Console.WriteLine("Ticket is available");
            
            //Selects quantity of ticket
            WaitUntilFindElement(driver, quantityOptionXpath);
            actions.Click(driver.FindElement(By.XPath(quantityOptionXpath))).Build().Perform();
            new SelectElement(driver.FindElement(By.XPath(quantityOptionXpath))).SelectByIndex(1);
            
            //closes Ticket Transfer Warning window
            if(ticketTransfer == true)
            {
                WaitUntilFindElement(driver, warningBlockButton);
                actions.Click(driver.FindElement(By.XPath(warningBlockButton))).Build().Perform();
            }

            //Selects ticket seat
            WaitUntilFindElement(driver, seatBlockOptionXpath);
            SelectTicket(driver, seatBlockOptionXpath, "1: Object");

            //Clicks the buy button
            WaitUntilFindElement(driver, selectBestSeatBtnXpath);
            actions.Click(driver.FindElement(By.XPath(selectBestSeatBtnXpath))).Build().Perform();

            SendEmail();
        }

        /// <summary>
        /// Waits until the element is clickable
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xPath"></param>
        public static void WaitUntilFindElement(ChromeDriver driver, string xPath)
        {
            new WebDriverWait(driver, TimeSpan.FromMinutes(1)).Until(ExpectedConditions.ElementToBeClickable(By.XPath(xPath)));
        }

        /// <summary>
        /// Selects ticket cathegory and ticket
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xPath"></param>
        /// <param name="value"></param>
        public static void SelectTicket(ChromeDriver driver, string xPath, string value)
        {
            Actions actions = new(driver);
            actions.Click(driver.FindElement(By.XPath(xPath))).Build().Perform();
            new SelectElement(driver.FindElement(By.XPath(xPath))).SelectByValue(value);
        }

        /// <summary>
        /// Checks if the element is present
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xPath"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        private static bool IsElementPresent(ChromeDriver driver, string xPath, int counter)
        {
            //waits for 2 seconds
            Thread.Sleep(2000);

            //if the element is present, returns true. When the element is not present, returns false and it breaks the loop.
            if (driver.FindElements(By.XPath(xPath)).Count != 0)
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.ElementToBeClickable(By.XPath(xPath)));

                //when the counter reach 50 it waits for 6 seconds
                if (counter % 50 == 0)
                    Thread.Sleep(6000);
                
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sends mail when the ticket find
        /// </summary>
        /// <param name="senderEmail"></param>
        /// <param name="senderPassword"></param>
        /// <param name="recipientEmail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        static void SendEmail()
        {
            // Google email and Password credentials
            string senderEmail = "";
            //You need get password for this app from .https://myaccount.google.com/apppasswords
            string senderPassword = "";

            // Receiver email address
            string recipientEmail = "";

            // The subject and body massages of mail
            string subject = "PASSOLİG BİLET";
            string body = "PASSOLİG'DE ARADIĞINIZ BİLET BULUNDU 10 DAKİKA SÜRENİZ BAŞLADI";

            try
            {
                //Creates smtp
                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    //sender email credentails
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;

                    //creates mail massages
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        // Gönderen, alıcı, başlık ve içerik bilgilerini ayarla
                        mailMessage.From = new MailAddress(senderEmail);
                        mailMessage.To.Add(recipientEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;

                        //sends mail
                        smtpClient.Send(mailMessage);

                        Console.WriteLine("E-posta başarıyla gönderildi.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("E-posta gönderme hatası: " + ex.Message);
            }
        }
        public static void pressKey(ChromeDriver driver, string key)
        {
            Actions actions = new(driver);
            actions.SendKeys(key).Build().Perform();
        }
        
    }
}
