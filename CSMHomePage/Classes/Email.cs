using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace CSMHomePage.Classes
{
    public static class Email
    {
        public static void SendEmail(MailMessage mail)
        {
            var smtpHost = "smtp.wrberkley.com";
            int port = 25;
            using (SmtpClient client = new SmtpClient(smtpHost))
            {
                if (string.IsNullOrEmpty(smtpHost)) return;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = smtpHost;
                client.Port = port;
                try
                {
                    client.Send(mail);
                }
                catch (SmtpException smtpEx)
                {
                    //write logging code here and capture smtpEx.Message
                }
            }
        }
        public static void EmailTheException(Exception ex)
        {
            MailMessage mail = new MailMessage("csmsupport@clermont.wrberkley.com", "nogola@clermont.wrberkley.com");
            //  mail.To.Add("nogola@clermont.wrberkley.com");
            //mail.From. = "csmsupport@clermont.wrberkley.com";
            mail.Subject = "CSM Home Page: An Application Exception has Occurred.";
            mail.Body = ex.ToString();

            SendEmail(mail);
        }
    }
}