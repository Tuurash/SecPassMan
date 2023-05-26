using System;
using System.Net;
using System.Net.Mail;

namespace SecPassMan.Services;

public class MailService
{
    private string smtpServer;
    private int smtpPort;
    private string smtpUsername;
    private string smtpPassword;

    public MailService(string server, int port, string username, string password)
    {
        smtpServer = server;
        smtpPort = port;
        smtpUsername = username;
        smtpPassword = password;
    }

    public void SendEmail(string to, string subject, string body)
    {
        try
        {
            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.To.Clear();
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(to);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    smtpClient.Send(mailMessage);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions here
            Console.WriteLine(ex.Message);
        }
    }
}