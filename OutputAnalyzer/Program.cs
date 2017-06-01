using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using System.IO;

namespace OutputAnalyzer
{
    class Program
    {

        public static string filePath = "";
        public static string Emailbody = "";

        static void Main(string[] args)
        {
            if (args[0]=="-o")
            {
                filePath = args[1];
            }

        }
        private static void SendEmail(string to, string from, string subject, string emailServer)
        {
            
            try
            {
                SmtpClient client = new SmtpClient(emailServer);
                MailAddress fromAddr = new MailAddress(from);
                MailMessage message = new MailMessage();
                message.To.Add(to);
                message.From = fromAddr;
                message.Body += Emailbody;
                message.Body += Environment.NewLine;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject += subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                try
                {
                    client.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Email SendEmail() Error: " + e.Message.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Email SendEmail() Error: " + e.Message.ToString());
            }
        }

    }
}
