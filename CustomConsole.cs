using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{
    public static class CustomConsole
    {
        public static void WriteLine(string message)
        {
            Main main = Program.Form;

            if (!string.IsNullOrEmpty(message))
                main.UpdateRichTextBox(main.GetLog(message));
        }
    }
}
