using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EDSCSR
{
    class Email
    {
        public static void SendEmail(string emailFrom, string password, string emailTo, string server, int port, bool ssl, List<string> attachments)
        {
            MailAddress from = new MailAddress(emailFrom, emailFrom);
            MailAddress to = new MailAddress(emailTo);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "EDSCSR";

            foreach (var attachment in attachments)
            {
                m.Attachments.Add(new Attachment(attachment));
            }

            SmtpClient smtp = new SmtpClient(server, port);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;

            smtp.Credentials = new NetworkCredential(emailFrom, password);
            smtp.EnableSsl = ssl;
            smtp.Send(m);
        }

        public static int DownloadNewEmails(string userEmail, string password, string server, int port, bool ssl)
        {
            int count = 0;

            List<string> seenUids = new List<string>();

            if (File.Exists(userEmail + "-seenUids"))
            {
                string[] lines = File.ReadAllLines(userEmail + "-seenUids");

                foreach(string line in lines)
                {
                    seenUids.Add(line);
                }
            }

            Pop3Client client = new Pop3Client();
            
            client.Connect(server, port, ssl);
            client.Authenticate(userEmail, password);

            List<string> uids = client.GetMessageUids();
            List<Message> newMessages = new List<Message>();

            for (int i = 0; i < uids.Count; i++)
            {
                string currentUidOnServer = uids[i];
                if (!seenUids.Contains(currentUidOnServer))
                {
                    MessageHeader headers = client.GetMessageHeaders(i+1);
                    
                    if(headers.Subject == "EDSCSR")
                    {
                        Message unseenMessage = client.GetMessage(i + 1);
                        seenUids.Add(currentUidOnServer);

                        SaveFullMessage(unseenMessage, currentUidOnServer, userEmail);

                        count++;
                    }
                }
            }

            File.WriteAllLines(userEmail + "-seenUids", seenUids);
            return count;
        }

        private static void SaveFullMessage(Message message, string uid, string userEmail)
        {
            string path = "Messages";

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += "\\" + userEmail;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += "\\new";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += "\\" + uid;

            Directory.CreateDirectory(path);

            foreach (MessagePart attachment in message.FindAllAttachments())
            {
                File.WriteAllBytes(path + "\\" + attachment.FileName, attachment.Body);
            }
        }
    }
}
