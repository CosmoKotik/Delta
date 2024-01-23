using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta.Tools.Chat
{
    public class Chat
    {
        public string text { get; set; }
        public bool bold { get; set; }

        public Chat(string msg)
        { 
            this.text = msg;
        }
    }
}
