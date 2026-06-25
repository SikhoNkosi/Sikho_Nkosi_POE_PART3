using System.Collections.Generic;
using System.Linq;

namespace Chatbot_GUI
{
    public class KeywordResponder
    {
        private readonly Dictionary<string, string> _simple = new Dictionary<string, string>
        {
            { "help", "Type a question or click 'Topics' to see available topics." },
            { "hello", "Hi! How can I help you?" }
        };

        public bool TryRespond(string input, out string response)
        {
            response = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            var key = input.Trim().ToLowerInvariant();
            if (_simple.TryGetValue(key, out response)) return true;
            var found = _simple.Keys.FirstOrDefault(k => key.Contains(k));
            if (found != null)
            {
                response = _simple[found];
                return true;
            }
            return false;
        }
    }
}
