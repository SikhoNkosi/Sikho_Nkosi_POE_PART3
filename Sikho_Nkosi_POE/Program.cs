using System;
using System.Media; // used to play .wav sound files on Windows
using System.Threading; // used for small delays when "typing" text
using System.IO; // file system access (checking for greeting audio)
using System.Diagnostics; // available for diagnostics if needed later
using System.Linq; // LINQ used for splitting and trimming input

namespace CyberSecurityChatbot
{
    // This small console application behaves like a simple chatbot
    // that teaches users about cybersecurity topics. It uses the
    // console for input/output and keeps everything in memory.
    class Program
    {
        static void Main(string[] args)
        {
            Chatbot bot = new Chatbot();

            // Step 1: Voice Greeting
            bot.VoiceGreeting();

            // Step 2: ASCII Art
            bot.ShowAsciiArt();

            // Step 3: Ask for Name
            string userName = bot.AskName();

            // Step 4: Welcome Message
            bot.WelcomeUser(userName);

            // Step 5: Start Chat Loop
            bot.ChatLoop();
        }
    }

    class Chatbot
    {
        // Simple in-memory state: the current user's name for this session.
        private string currentUserName = "Guest";

        // TopicManager encapsulates all topic data and fuzzy-matching helpers.
        private readonly TopicManager topicManager = new TopicManager();

        public Chatbot()
        {
            // TopicManager initializes topics in its constructor.
        }

        public void VoiceGreeting()
        {
            // Try to play a greeting.wav in the application folder. This is non-blocking
            // because we call LoadAsync()/Play() so the app continues to run even while audio plays.
            try
            {
                // attempt to locate a wav file in the application directory
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greetings.wav");

                if (File.Exists(path))
                {
                    SoundPlayer player = new SoundPlayer(path);
                    player.LoadAsync();
                    player.Play();
                }
                else
                {
                    // If there's no file, we just inform the user and continue.
                    Console.WriteLine("Greeting file not found at");
                }
            }
            catch
            {
                //graceful handling for audio playback issues
                // If something goes wrong while trying to play audio (platform differences,
                // missing permissions, etc.), don't crash — just continue without sound.
                Console.WriteLine("Unable to play greeting audio. Continuing without sound.");
            }
        }

        public void ShowAsciiArt()
        {
            // Print a friendly ASCII banner. We color different sections so the console
            // output looks nicer. This is purely cosmetic and helps the user know the app
            // they are interacting with.
            // Top frame in cyan
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("#############################################################");
            Console.WriteLine("#                                                           #");

            // Big ASCII "Cyber Team" in bright yellow
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("#    _____       _              _______                     #");
            Console.WriteLine("#   / ____|     | |            |__   __|                    #");
            Console.WriteLine("#  | |     _   _| |__   ___ _ __  | | ___  __ _ _ __ ___    #");
            Console.WriteLine("#  | |    | | | | '_ \\ / _ \\ '__| | |/ _ \\/ _` | '_ ` _ \\   #");
            Console.WriteLine("#  | |____| |_| | |_) |  __/ |    | |  __/ (_| | | | | | |  #");
            Console.WriteLine("#   \\_____| .__/|_.__/ \\___|_|    |_|\\___|\\__,_|_| |_| |_|  #");
            Console.WriteLine("#         | |                                               #");
            Console.WriteLine("#         |_|                                               #");

            // Bottom frame in cyan
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("#                                                           #");
            Console.WriteLine("#############################################################");

            // Robot face in green
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("        .----.   @   @");
            Console.WriteLine("       / .-'-.`.  \\v/");
            Console.WriteLine("       | | '\\ \\ \\_/ )");
            Console.WriteLine("     ,-\\ `-.' /.'  /");
            Console.WriteLine("    '---`----'----'");

            // Subtitle in magenta
            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine("        Cyber Avengers");

            // Reset to default
            Console.ResetColor();
        }

        private void ShowCommands()
        {
            // Show a short help block. This tells new users what the bot accepts
            // and gives examples to reduce confusion.
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("Choose one of the commands below or ask a question.");
            Console.WriteLine();
            Console.WriteLine("What you can do:");
            Console.WriteLine("  - See a list of topics: type 'topics' or 'show topics' or 'please show topics'");
            Console.WriteLine("  - Get a quick security tip: type 'tip' or 'give me a tip' or 'security tip'");
            Console.WriteLine("  - Ask for help: type 'help' or 'how do i' or 'what can i do'");
            Console.WriteLine("  - Quit: type 'exit'");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  show topics");
            Console.WriteLine("  give me a tip");
            Console.WriteLine("  what is phishing?");
            Console.WriteLine();
            Console.ResetColor();
        }

        private void ShowRandomTip()
        {
            // Keep a short list of quick, actionable security tips and pick one at random.
            // This is useful for showing a short reminder at startup or on demand.
            var tips = new[]
            {
                "Use a password manager to create and store unique passwords.",
                "Enable multi-factor authentication on your important accounts.",
                "Think before you click — verify unexpected links and attachments.",
                "Keep your software and devices up to date with security patches.",
                "Back up important data and verify your backups regularly.",
                "Use HTTPS and avoid entering credentials on unsecured web pages."
            };

            var rnd = new Random();
            string tip = tips[rnd.Next(tips.Length)];
            Console.ForegroundColor = ConsoleColor.Yellow;
            TypeWrite($"Tip: {tip}\n");
            Console.ResetColor();
        }

        public string AskName()
        {
            // Prompt the user for a display name. If they press Enter with no name,
            // default to "Guest". We trim whitespace to keep the stored name clean.
            Console.Write("Please enter your name: ");
            string name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Guest";
            }
            currentUserName = name.Trim();
            return currentUserName;
        }

        public void WelcomeUser(string userName)
        {
            // A friendly, conversational welcome that sets expectations and
            // shows the user how to interact with the chatbot.
            Console.ForegroundColor = ConsoleColor.Green;
            TypeWrite($"Hey {userName}! 👋 Great to meet you. I'm here to help you stay safe online. 🙂\n");
            Console.ResetColor();

            // Show basic commands and a short tip so the user knows where to start.
            ShowCommands();
            ShowRandomTip();

            // A short, plain-language prompt that nudges the user to either choose
            // a command or type a question — useful for people who are unsure what to do next.
            TypeWrite("You can choose one of the commands above or just ask me a question. For example, type 'topics' to see a list.\n");
        }

        public void ChatLoop()
        {
            bool chatting = true;

            while (chatting)
            {
                Console.Write("\nAsk me something (or type 'exit' to quit): ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    // If the user didn't type anything, remind them to input a question
                    // instead of treating an empty line as a command.
                    Console.WriteLine("You didn’t type anything. Please ask a question.");
                    continue;
                }

                input = input.Trim();
                string lower = input.ToLowerInvariant();

                if (lower == "exit")
                {
                    chatting = false;
                    // say goodbye using recorded name
                    Console.WriteLine($"Goodbye {currentUserName}");
                }
                else
                {
                    Respond(input);
                }
            }
        }

        public void Respond(string input)
        {
            // allow user to ask 2-3 questions at once by splitting on common delimiters
            var separators = new[] { '?', '.', '!', ';' };
            var rawParts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .Where(p => p.Length > 0)
                                .ToList();

            if (rawParts.Count == 0)
            {
                // If splitting produced no meaningful parts (e.g. input was punctuation), prompt again.
                Console.WriteLine("You didn’t type anything meaningful. Please ask a question.");
                return;
            }

            // Normalize the input once for simple keyword checks.
            string singleLower = input.Trim().ToLowerInvariant();

            // Detect keywords anywhere in the user's input so the bot is forgiving
            // and understands phrases like "please show topics". This makes the
            // chatbot easier to use for new or non-technical users.
            if (singleLower.Contains("topics") || singleLower.Contains("show topics") || singleLower.Contains("list topics") || singleLower.Contains("show the topics"))
            {
                ShowTopics();
                return;
            }

            if (singleLower.Contains("help") || singleLower.Contains("what can i do") || singleLower.Contains("how do i"))
            {
                ShowCommands();
                return;
            }

            if (singleLower.Contains("tip") || singleLower.Contains("security tip") || singleLower.Contains("give me a tip") || singleLower.Contains("show tip"))
            {
                ShowRandomTip();
                return;
            }

            // Friendly small talk shortcuts: respond to simple greetings and thanks
            // so the bot feels more human and approachable.
            if (singleLower == "hi" || singleLower == "hello" || singleLower == "hey")
            {
                TypeWrite($"Hi {currentUserName}! How can I help you today?\n");
                return;
            }

            if (singleLower == "thanks" || singleLower == "thank you")
            {
                TypeWrite("You're welcome! If you need anything else type 'help'.\n");
                return;
            }

            // If the input contains multiple short questions (e.g. "what is phishing? tips?"),
            // only process up to 3 to avoid overwhelming the user and to keep the flow simple.
            int toProcess = Math.Min(3, rawParts.Count);
            for (int i = 0; i < toProcess; i++)
            {
                ProcessSingleQuery(rawParts[i]);
            }
        }

        private void ProcessSingleQuery(string query)
        {
            // Normalize the query for matching.
            string lower = query.Trim().ToLowerInvariant();

            // 1) Numeric selection: if the user types a number (like "1"),
            //    we use the corresponding topic from topicKeys (1-based index).
            string[] parts = lower.Split('-', 2);
            if (int.TryParse(parts[0].Trim(), out int idx))
            {
                if (idx >= 1 && idx <= topicManager.TopicKeys.Count)
                {
                    string key = topicManager.TopicKeys[idx - 1];
                    if (topicManager.TryGetAnswer(key, out string answer))
                    {
                        PrintLongAnswer(answer);
                    }
                    return;
                }
            }

            // 2) Exact match by topic name: if the user typed a topic key exactly,
            //    return the associated long answer.
            if (topicManager.TryGetAnswer(lower, out string directAnswer))
            {
                PrintLongAnswer(directAnswer);
                return;
            }

            // 3) Fuzzy matching: try to find the closest topic key using a
            //    simple Levenshtein distance heuristic. If the best match is close
            //    enough, ask the user to confirm before showing the answer.
            string best = topicManager.FindBestTopicMatch(lower, out int distance);
            if (best != null)
            {
                // Ask for a short confirmation so we don't assume wrongly.
                Console.WriteLine($"You mean the {best}? yes or no?");
                string resp = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
                if (resp == "y" || resp == "yes" || resp == "1")
                {
                    if (topicManager.TryGetAnswer(best, out string matchedAnswer))
                    {
                        PrintLongAnswer(matchedAnswer);
                    }
                    return;
                }
                else
                {
                    Console.WriteLine("Okay, I won't assume that. Type 'topics' to see the list or rephrase your question.");
                    return;
                }
            }

            // special inquiry about when name was first entered — the bot does not
            // persist names between runs, so we tell the user that we only remember
            // the name for the current session.
            if (lower.Contains("when did i first") || lower.Contains("first entered") || lower.Contains("when did i enter"))
            {
                Console.WriteLine("I don't keep a permanent record of when names were entered. I only remember the name you provided for this session.");
                return;
            }

            // Common conversational phrases the bot understands. These give
            // short friendly responses instead of the full topic answers.
            switch (lower)
            {
                case "how are you":
                case "how are you?":
                    TypeWrite($"{currentUserName}, I’m just code, but I’m running smoothly!\n");
                    break;

                case "what is your purpose":
                case "what is your purpose?":
                case "what's your purpose":
                case "what's your purpose?":
                    TypeWrite($"{currentUserName}, I’m here to teach you about cybersecurity and help you find trusted information on common security topics.\n");
                    break;

                case "what can i ask you about":
                case "what can i ask you about?":
                case "what can i ask you":
                case "what can i ask you?":
                    TypeWrite($"{currentUserName}, you can ask me about many cybersecurity topics. Type 'topics' to see a numbered list or ask a topic name directly.\n");
                    break;

                default:
                    // If we reach this point, the input wasn't a command, topic, or recognized
                    // short conversation. Give a helpful hint about how the user can proceed.
                    TypeWrite($"{currentUserName}, I didn’t quite understand that. You can type 'topics' to see what I can explain, choose a number like '1' or type a topic name.\n");
                    break;
            }
        }

        private void PrintLongAnswer(string answer)
        {
            // Ensure the answer prints as multiple lines and appears as typed by the chatbot
            Console.WriteLine();
            TypeWrite($"{currentUserName}, here's what I found:\n");
            TypeWrite("Cyber Avengers: ");
            foreach (var line in answer.Split('\n'))
            {
                TypeWrite(line);
                Console.WriteLine();
            }
        }

        private void ShowTopics()
        {
            // draw a bordered box with topics
            int width = 60;
            string border = new string('=', width);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(border);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(CenterText("Cyber Avengers - Topics", width));
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(border);
            for (int i = 0; i < topicManager.TopicKeys.Count; i++)
            {
                string entry = $"{i + 1}. {topicManager.TopicKeys[i]}";
                // alternate colors for readability
                Console.ForegroundColor = (i % 2 == 0) ? ConsoleColor.Yellow : ConsoleColor.Cyan;
                Console.WriteLine("| " + entry.PadRight(width - 4) + " |");
            }
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(border);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(CenterText("Type a number (e.g. '1') or topic name, or ask a question.", width));
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(border);
            Console.ResetColor();
        }

        private void TypeWrite(string text, int delayMs = 5)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                try { Thread.Sleep(delayMs); } catch { }
            }
        }

        private string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            int left = (width - text.Length) / 2;
            return new string(' ', left) + text;
        }

        // user info persistence removed: currentUserName is session-only
    }
}
