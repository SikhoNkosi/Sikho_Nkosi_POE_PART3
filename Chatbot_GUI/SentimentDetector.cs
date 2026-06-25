using System;

namespace Chatbot_GUI
{
    // Very small stub for sentiment detection. Returns Positive/Neutral/Negative
    public class SentimentDetector
    {
        public string Detect(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Neutral";
            var lower = text.ToLowerInvariant();
            if (lower.Contains("good") || lower.Contains("great") || lower.Contains("thanks")) return "Positive";
            if (lower.Contains("bad") || lower.Contains("hate") || lower.Contains("stupid")) return "Negative";
            return "Neutral";
        }
    }
}
