using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Delta.Tools.Chat
{
    internal class ChatComponent
    {
        public static string Build(Chat chat)
        { 
            return JsonConvert.SerializeObject(chat);
        }
    }
}
