using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace upAlarm
{
    class Email:System.Net.Mail.SmtpClient
    {
        public Email()
        {
            this.SendAsync("upAlarm@localhost", "r3d__@hotmail.com", "ip down", "The up is down", new object());
        }

    }
}
