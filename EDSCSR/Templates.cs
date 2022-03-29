using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSCSR
{
    class Templates
    {
        public struct Template
        {
            public string userEmail;
            public string userPassword;

            public string smtpServer;
            public int smtpPort;
            public bool smtpSsl;

            public string popServer;
            public int popPort;
            public bool popSsl;

            public string receiver;
        }

        public static Template template = new Template();

        public static void LoadFromTemplate1()
        {
            template.userEmail = "yanchenkov.nikita98@mail.ru";
            template.userPassword = "SsXTvyEtR1wheLFNEv0W";

            template.smtpServer = "smtp.mail.ru";
            template.smtpPort = 587;
            template.smtpSsl = true;

            template.popServer = "pop.mail.ru";
            template.popPort = 995;
            template.popSsl = true;

            template.receiver = "nikita.yeremeev97@mail.ru";
        }

        public static void LoadFromTemplate2()
        {
            template.userEmail = "nikita.yeremeev97@mail.ru";
            template.userPassword = "bLGVVxiZmkpvm3RMTbhN";

            template.smtpServer = "smtp.mail.ru";
            template.smtpPort = 587;
            template.smtpSsl = true;

            template.popServer = "pop.mail.ru";
            template.popPort = 995;
            template.popSsl = true;

            template.receiver = "yanchenkov.nikita98@mail.ru";
        }
    }
}
