using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Xml.Linq;

namespace Chatbot_GUI
{
    public class ChatBot
    {
        private readonly TaskRepository _tasksRepo;
        private readonly Random _rand = new Random();
        private readonly List<string> _topicKeys;
        private readonly Dictionary<string, string> _answers;
        public readonly MemoryStore Memory = new MemoryStore();

        public ChatBot()
        {
            _topicKeys = new List<string>();
            _answers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // Initialize TaskRepository from AppDomain config file (without requiring System.Configuration reference)
            string cs = null;
            string provider = "MySql.Data.MySqlClient";
            try
            {
                var configPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                if (System.IO.File.Exists(configPath))
                {
                    var doc = XDocument.Load(configPath);
                    var add = doc.Descendants("connectionStrings").Descendants("add").FirstOrDefault(e => (string)e.Attribute("name") == "ChatbotTasksDb");
                    if (add != null)
                    {
                        cs = (string)add.Attribute("connectionString");
                        var prov = (string)add.Attribute("providerName");
                        if (!string.IsNullOrEmpty(prov)) provider = prov;
                    }
                }
            }
            catch { }
            if (!string.IsNullOrEmpty(cs))
            {
                try { _tasksRepo = new TaskRepository(cs, provider); } catch { _tasksRepo = null; }
            }

            InitializeTopics();
        }

        private const int TopicChunkLines = 3;

        private string StartTopicExplanation(string key)
        {
            Memory.LastTopic = key;
            Memory.TopicOffset = 0;
            return GetNextTopicChunk(key, includeHeader: true);
        }

        private string GetNextTopicChunk(string key, bool includeHeader = false)
        {
            if (!_answers.TryGetValue(key, out var full)) return "Sorry, I don't have more information about that.";
            var lines = full.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
            var start = Memory.TopicOffset;
            if (start >= lines.Length)
            {
                // nothing more
                Memory.LastTopic = null;
                Memory.TopicOffset = 0;
                return "I've already shared everything I have on that topic.";
            }
            var take = Math.Min(TopicChunkLines, lines.Length - start);
            var chunk = string.Join(Environment.NewLine, lines.Skip(start).Take(take));
            Memory.TopicOffset += take;
            var header = includeHeader ? $"{Memory.UserName}, here's more about {key}:\n" : string.Empty;
            var morePrompt = Memory.TopicOffset < lines.Length ? "\n\nType 'tell me more' to continue." : string.Empty;
            if (Memory.TopicOffset >= lines.Length)
            {
                // finished explanation, clear last topic
                Memory.LastTopic = null;
                Memory.TopicOffset = 0;
            }
            return header + chunk + morePrompt;
        }
        private void InitializeTopics()
        {
            void Add(string key, string answer)
            {
                _topicKeys.Add(key);
                _answers[key] = answer;
            }

            Add("phishing", @"Phishing is a type of social engineering attack where attackers craft deceptive messages to trick you into revealing personal information or clicking malicious links.
These messages often impersonate trusted organizations and use urgent language to provoke action.
Common indicators include unexpected requests, poor spelling/grammar, and mismatched URLs.
If you suspect a message is phishing, do not click links or open attachments; verify the sender by other channels.
Organizations should use email authentication (SPF, DKIM, DMARC) and security awareness training to reduce risk.
On a personal level, enable multi-factor authentication and use unique passwords to limit damage from credential theft.
Always report suspicious messages to your IT team or email provider so they can block the threat.");

            Add("password safety", @"Password safety means using strong, unique passwords for each account and changing them when compromise is suspected.
A strong password combines length and complexity — passphrases are often easier to remember and more secure.
Never reuse passwords across important services; reuse makes lateral compromise trivial for attackers.
Use a reputable password manager to generate and store credentials securely rather than writing them down.
Wherever possible, enable multi-factor authentication to add an additional layer beyond passwords.
Review account activity regularly and remove stale accounts to reduce the attack surface.
Consider periodic password audits and rotating credentials used by services or applications.");

            Add("two-factor authentication", @"Two-factor authentication (2FA) requires a second form of verification in addition to a password.
Common factors include SMS codes, authenticator apps, hardware tokens, or biometric checks.
Authenticator apps and hardware tokens are generally more secure than SMS, which can be intercepted.
2FA mitigates risks from stolen credentials because an attacker needs both the password and the second factor.
Organizations should make 2FA available for all sensitive accounts and prioritize hardware-based keys for admins.
Educate users about phishing attempts that try to capture 2FA codes and how to report suspicious prompts.
Implement account recovery procedures that are secure and avoid social-engineering weaknesses.");

            Add("malware", @"Malware is software designed to harm or exploit systems, including viruses, trojans, spyware, and worms.
It can steal data, encrypt files for ransom, or provide remote control to attackers.
Common infection vectors include malicious email attachments, compromised websites, and unpatched software.
Defenses include endpoint protection, regular patching, least-privilege accounts, and network segmentation.
Backups and incident response plans reduce the impact of malware like ransomware.
User education about suspicious downloads and attachments also significantly lowers risk.
Monitor systems for unusual activity and use threat intelligence to detect known indicators of compromise.");

            Add("ransomware", @"Ransomware is a form of malware that encrypts files and demands payment for the decryption key.
It often spreads through phishing, exposed remote services, or compromised remote desktop protocols.
Preventive measures include offline backups, patching, network segmentation, and restricting administrative privileges.
Organizations should practice incident response procedures and have an isolated recovery environment.
Paying a ransom does not guarantee data recovery and may fund further criminal activity.
Legal and regulatory considerations may affect the decision to pay; engage legal counsel and law enforcement.
Regularly test backups to ensure recovery works and perform tabletop exercises for ransomware scenarios.");

            Add("social engineering", @"Social engineering exploits human psychology rather than technical vulnerabilities to gain trust or sensitive information.
Attackers impersonate colleagues, create urgency, or use flattery to lower guard and prompt risky actions.
Security awareness training with realistic simulations reduces susceptibility to these manipulations.
Policies such as verifying identity before transactions and clear escalation paths help prevent fraud.
Multi-channel verification and strict data-handling procedures mitigate social engineering attacks.
Incident reporting and a culture that supports questions about suspicious requests are critical defenses.
Always verify unusual requests through a trusted, independent channel before acting.");

            Add("vpns", @"Virtual Private Networks (VPNs) create an encrypted tunnel between a device and a network, protecting traffic from eavesdroppers.
Use reputable VPN solutions with strong cryptography and proper authentication to avoid false security.
Be cautious with free VPN services that may log or sell data; enterprise solutions should be centrally managed.
VPNs help secure remote work, but do not replace endpoint security or network segmentation.
Always combine VPN usage with device hygiene, such as patches and anti-malware, to reduce risk.
Monitor VPN access and require multi-factor authentication for remote connections.
Ensure split-tunneling settings are configured according to security requirements to avoid data leakage.");

            Add("firewalls", @"Firewalls control incoming and outgoing network traffic based on rules and are a basic perimeter defense.
Next-generation firewalls add features like application awareness and threat inspection.
Use firewall rules that implement least privilege for network services and restrict unnecessary ports.
Combine perimeter firewalls with host-based firewalls for layered defense.
Log and monitor firewall activity to detect scans and unusual traffic patterns.
Keep firewall firmware updated and review rules periodically to remove stale exceptions.
Integrate firewalls into broader security monitoring and incident response workflows.");

            Add("secure wi-fi", @"Secure Wi-Fi involves using strong encryption (WPA3 where possible) and avoiding legacy insecure protocols.
Change default administration passwords on access points and hide management interfaces from public access.
Segment guest networks from internal resources and limit what guests can access.
Ensure SSIDs do not leak sensitive information and rotate pre-shared keys on a schedule when used.
Use enterprise authentication (e.g., 802.1X) for strong identity-based access control in corporate environments.
Conduct periodic wireless audits to find rogue access points and misconfigurations.
Educate users about connecting only to trusted networks and the risks of open Wi-Fi.");

            Add("encryption", @"Encryption protects data by making it unreadable without the correct keys; it is essential in transit and at rest.
Use TLS for web and service communications and strong algorithms with up-to-date libraries.
Encryption at rest protects stored data; combine with secure key management for effectiveness.
Beware of outdated algorithms and ensure cryptographic libraries are patched.
Use full-disk encryption for devices to reduce risk from lost or stolen hardware.
Plan for key rotation and revocation to limit exposure if keys are compromised.
Balance encryption with legal and regulatory requirements for data access and retention.");

            Add("backups", @"Backups are copies of data kept to enable recovery after data loss, corruption, or ransomware.
Follow the 3-2-1 rule: three copies, on two different media, with at least one offsite or air-gapped.
Test backups regularly to verify they can be restored and meet recovery time objectives.
Protect backups with access controls and encryption to prevent them from being targeted.
Automate backup schedules and retention policies aligned with business needs.
Document recovery procedures and perform tabletop exercises to validate plans.
Keep backups immutable or offline where possible to reduce the risk of ransomware encryption.");

            Add("software updates", @"Software updates and patching close known vulnerabilities that attackers can exploit.
Implement a patch management program that prioritizes critical and internet-facing systems.
Use automated patching where safe, and test updates in staging before production deployment.
Maintain an inventory of software assets to ensure nothing is left unpatched unintentionally.
Combine patching with compensating controls for systems that cannot be updated immediately.
Monitor vendor advisories and security bulletins for emergent threats that require urgent action.
Document rollback plans in case updates cause unexpected service disruptions.");

            Add("safe browsing", @"Safe browsing habits reduce exposure to drive-by downloads, malicious sites, and scams.
Keep browsers and plugins up to date, and disable or remove unneeded extensions.
Use content blockers and script controls to limit attack surface from web content.
Verify the authenticity of websites before entering credentials and prefer HTTPS connections.
Avoid downloading software from unknown sources and validate checksums where provided.
Consider using separate browser profiles for risky activities and isolate high-privilege tasks.
Educate users to recognize deceptive UI patterns and indicators of compromised sites.");

            Add("identity theft", @"Identity theft occurs when attackers steal personal data to impersonate or commit fraud.
Protect sensitive documents, shred physical records, and be cautious sharing personal details online.
Use credit monitoring and alerts to detect unusual activity on financial accounts.
Enable multi-factor authentication on important accounts and use strong, unique passwords.
Report suspicious activity to financial institutions and authorities promptly.
Limit data shared on social media to reduce profiling and targeted attacks.
Consider identity protection services if you handle high-value personal data or were compromised.");

            Add("data privacy", @"Data privacy is about controlling how personal data is collected, used, and shared.
Implement data minimization, collecting only what is necessary for a purpose.
Use access controls and encryption to protect sensitive datasets from unauthorized access.
Provide transparency to users about data practices and respect consent choices.
Comply with applicable regulations (e.g., GDPR, CCPA) and perform privacy impact assessments.
Anonymize or pseudonymize data when full identifiers are not required for processing.
Establish retention policies and secure deletion methods to reduce long-term exposure.");

            Add("network security", @"Network security ensures the confidentiality, integrity, and availability of networked systems.
Use segmentation, least-privilege routing, and access controls to limit lateral movement.
Deploy intrusion detection/prevention and continuous monitoring to spot anomalies.
Secure remote access with VPNs, zero trust models, and strong authentication.
Harden network devices by changing defaults, applying patches, and disabling unused services.
Encrypt sensitive traffic and establish logging for forensic capabilities in incidents.
Regularly assess the network through vulnerability scanning and penetration testing.");

            Add("iot security", @"IoT security covers connected devices like cameras, sensors, and appliances that often have limited defenses.
Change default credentials, segment IoT devices onto isolated networks, and monitor their behavior.
Keep firmware up to date and prefer vendors that support secure update mechanisms.
Limit the data IoT devices collect and store, and protect any sensitive streams with encryption.
Inventory IoT devices and remove or replace unmanaged devices to reduce attack surface.
Use strong authentication and consider network-level controls to restrict device capabilities.
Plan for lifecycle management since many IoT devices have long uptime and infrequent updates.");

            Add("mobile security", @"Mobile security focuses on protecting smartphones and tablets from malware and data loss.
Use device encryption, strong lock-screen authentication, and keep OS/apps updated.
Install apps only from trusted stores and review permissions before granting access.
Enable remote wipe and backup so you can recover or remove data from lost devices.
Be cautious with public Wi-Fi and use VPNs for sensitive communications.
Use separate work profiles for enterprise data and follow your organization's mobile policies.
Educate users about SMS phishing (smishing) and suspicious app behavior.");

            Add("cloud security", @"Cloud security encompasses protecting data, applications, and services hosted by cloud providers.
Understand the shared responsibility model: providers secure infrastructure, customers secure data/identity.
Use strong identity and access management, least privilege, and multi-factor authentication.
Encrypt data in transit and at rest and ensure keys are managed securely.
Monitor cloud logs, configure alerts, and use native provider tooling for threat detection.
Secure APIs and automate secure configuration to avoid misconfiguration risks.
Regularly review third-party integrations and conduct cloud-focused penetration tests.");

            Add("zero trust", @"Zero Trust is a security model that treats every access request as untrusted until verified.
It enforces strong authentication, authorization, and continuous validation for users and devices.
Micro-segmentation and least privilege reduce blast radius when breaches occur.
Adopt identity-centric controls and robust telemetry to make access decisions in real time.
Zero Trust is iterative: start with high-risk assets and expand controls progressively.
Automate policy enforcement and monitoring to maintain consistent security posture.
Measure outcomes with key security metrics and refine controls based on incidents and findings.");

            Add("incident response", @"Incident response is the coordinated approach to detect, contain, and recover from security incidents.
Establish an incident response plan with clear roles, communication channels, and escalation paths.
Practice through tabletop exercises and post-incident reviews to improve readiness.
Collect and preserve forensic evidence while balancing business continuity needs.
Engage legal, PR, and external specialists as needed for complex or public incidents.
Restore services from trusted backups and perform root-cause analysis to prevent recurrence.
Maintain documentation and lessons learned to strengthen defenses over time.");

            Add("physical security", @"Physical security protects facilities and hardware through controls like access badges and locks.
Restrict server room access, disable unused ports, and secure devices against theft.
Combine physical controls with environmental monitoring and surveillance for early detection.
Ensure disposal procedures securely erase data-bearing devices before disposal.
Train staff to recognize tailgating and unauthorized individuals.
Integrate physical security incidents into broader incident response processes.
Physical measures increase the difficulty and cost for attackers who need physical access.");
        }

        public string ShowCommands()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Choose one of the commands below or ask a question.");
            sb.AppendLine();
            sb.AppendLine("What you can do:");
            sb.AppendLine("  - See a list of topics: type 'topics' or 'show topics' or 'please show topics'");
            sb.AppendLine("  - Get a quick security tip: type 'tip' or 'give me a tip' or 'security tip'");
            sb.AppendLine("  - Ask for help: type 'help' or 'how do i' or 'what can i do'");
            sb.AppendLine("  - Quit: type 'exit'");
            sb.AppendLine();
            sb.AppendLine("Examples:");
            sb.AppendLine("  show topics");
            sb.AppendLine("  give me a tip");
            sb.AppendLine("  what is phishing?");
            sb.AppendLine();
            return sb.ToString();
        }

        public string GetAsciiArt()
        {
            return "#############################################################\n#                                                           #\n#    _____       _              _______                     #\n#   / ____|     | |            |__   __|                    #\n#  | |     _   _| |__   ___ _ __  | | ___  __ _ _ __ ___    #\n#  | |    | | | | '_ \\ / _ \\ '__| | |/ _ \\/ _` | '_ ` _ \\   #\n#  | |____| |_| | |_) |  __/ |    | |  __/ (_| | | | | | |  #\n#   \\_____| .__/|_.__/ \\___|_|    |_|\\___|\\__,_|_| |_| |_|  #\n#         | |                                               #\n#         |_|                                               #\n#                                                           #\n#############################################################\n        .----.   @   @\n       / .-'-.`.  \\v/\n       | | '\\ \\ \\_/ )\n     ,-\\ `-.' /.\\'  /\n    '---`----'----'\n        Cyber Avengers";
        }

        public IEnumerable<string> TopicKeys => _topicKeys;

        public string GetTopics() => string.Join(Environment.NewLine, _topicKeys.Select((k, i) => $"{i + 1}. {k}"));

        public string GetRandomTip()
        {
            var tips = new[]
            {
                "Use a password manager to create and store unique passwords.",
                "Enable multi-factor authentication on your important accounts.",
                "Think before you click — verify unexpected links and attachments.",
                "Keep your software and devices up to date with security patches.",
                "Back up important data and verify backups regularly.",
                "Use HTTPS and avoid entering credentials on unsecured web pages."
            };
            return tips[new Random().Next(tips.Length)];
        }

        public void VoiceGreeting()
        {
            try
            {
                // prefer the exact filename present in this GUI project ('greetings')
                string[] candidateNames = { "greetings.wav", "greeting.wav", "greet.wav" };
                string[] searchDirs = { AppDomain.CurrentDomain.BaseDirectory, Environment.CurrentDirectory };

                string foundPath = null;
                foreach (var dir in searchDirs)
                {
                    foreach (var name in candidateNames)
                    {
                        var p = Path.Combine(dir, name);
                        if (File.Exists(p))
                        {
                            foundPath = p;
                            break;
                        }
                    }
                    if (foundPath != null) break;
                }

                if (foundPath != null)
                {
                    using (var player = new SoundPlayer(foundPath))
                    {
                        player.Load();
                        player.Play();
                    }
                    return;
                }

                var asm = Assembly.GetExecutingAssembly();
                var resName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));
                if (resName != null)
                {
                    var stream = asm.GetManifestResourceStream(resName);
                    if (stream != null)
                    {
                        using (stream)
                        using (var player = new SoundPlayer(stream))
                        {
                            player.Load();
                            player.Play();
                        }
                        return;
                    }
                }
            }
            catch
            {
                // don't crash the UI on audio problems
            }
        }

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Please type a question or command.";

            var separators = new[] { '?', '.', '!', ';' };
            var rawParts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .Where(p => p.Length > 0)
                                .Take(3)
                                .ToList();

            if (rawParts.Count == 0) return "You didn’t type anything meaningful. Please ask a question.";

            var responses = new List<string>();
            foreach (var part in rawParts)
            {
                responses.Add(ProcessSingleQuery(part));
            }
            return string.Join(Environment.NewLine + Environment.NewLine, responses.Where(r => !string.IsNullOrEmpty(r)));
        }

        private string ProcessSingleQuery(string query)
        {
            var lower = query.Trim().ToLowerInvariant();
            // If the user asks for more details and we have a last topic, continue from where we left off
            if ((lower == "more" || lower == "tell me more" || lower == "say more" || lower == "more please" || lower == "say more please") && !string.IsNullOrEmpty(Memory.LastTopic))
            {
                return GetNextTopicChunk(Memory.LastTopic);
            }

            // Also handle requests like 'tell me more about phishing'
            if (lower.Contains("more") && TryExtractTopicFromExplainRequest(lower, out var explicitTopic))
            {
                return StartTopicExplanation(explicitTopic);
            }

            // Task management commands (if repository available)
            if (_tasksRepo != null)
            {
                // Start interactive add task flow
                if (lower.StartsWith("add task"))
                {
                    Memory.IsCreatingTask = true;
                    Memory.PendingTaskTitle = null;
                    Memory.AwaitingReminderSpecification = false;
                    return "Sure — what should I name the task?";
                }

                // If user is in task creation flow and hasn't provided a title yet
                if (Memory.IsCreatingTask && string.IsNullOrEmpty(Memory.PendingTaskTitle))
                {
                    var title = query.Trim();
                    if (string.IsNullOrWhiteSpace(title)) return "Please provide a non-empty task title.";
                    var id = _tasksRepo.AddTask(title, string.Empty, null);
                    Memory.PendingTaskId = id;
                    Memory.PendingTaskTitle = title;
                    Memory.AwaitingReminderSpecification = true;
                    return $"Task '{title}' saved. Would you like a reminder? If yes, say e.g. 'remind me in 3 days' or provide a date.";
                }

                // If awaiting reminder specification
                if (Memory.IsCreatingTask && Memory.AwaitingReminderSpecification)
                {
                    // parse phrases like 'remind me in 3 days' or 'remind me in 5 hours' or a date
                    var m = System.Text.RegularExpressions.Regex.Match(lower, "remind me in (\\d+) (day|days|hour|hours)");
                    DateTime? remindAt = null;
                    if (m.Success)
                    {
                        var n = int.Parse(m.Groups[1].Value);
                        var unit = m.Groups[2].Value;
                        if (unit.StartsWith("day")) remindAt = DateTime.Now.AddDays(n);
                        else remindAt = DateTime.Now.AddHours(n);
                    }
                    else
                    {
                        // try parse a date
                        if (DateTime.TryParse(query, out var dt)) remindAt = dt;
                    }

                    if (remindAt.HasValue)
                    {
                        _tasksRepo.UpdateTaskReminder(Memory.PendingTaskId, remindAt.Value);
                        var title = Memory.PendingTaskTitle;
                        Memory.IsCreatingTask = false;
                        Memory.AwaitingReminderSpecification = false;
                        Memory.PendingTaskId = 0;
                        Memory.PendingTaskTitle = null;
                        return $"Got it — I'll remind you about '{title}' on {remindAt.Value:G}. Don't worry, I'll send the reminder.";
                    }
                    else
                    {
                        // user said no or couldn't parse
                        if (lower.Contains("no") || lower.Contains("don't") || lower.Contains("dont"))
                        {
                            Memory.IsCreatingTask = false;
                            Memory.AwaitingReminderSpecification = false;
                            return "Okay, I won't set a reminder. The task is saved.";
                        }
                        return "I didn't understand the reminder time. Please say 'remind me in 3 days' or give a date like '2024-12-31'.";
                    }
                }

                if (lower.StartsWith("show tasks") || lower.StartsWith("view tasks") || lower == "tasks")
                {
                    var tasks = _tasksRepo.GetAllTasks();
                    if (tasks.Count == 0) return "You have no tasks.";
                    var sb = new System.Text.StringBuilder();
                    foreach (var t in tasks)
                    {
                        sb.AppendLine($"[{t.TaskID}] {(t.IsCompleted ? "(Done)" : "(Pending)")} {t.Title} - {t.Description} {(t.ReminderDate.HasValue ? "(Remind: " + t.ReminderDate.Value.ToShortDateString() + ")" : string.Empty)}");
                    }
                    return sb.ToString();
                }

                if (lower.StartsWith("complete task") || lower.StartsWith("mark task"))
                {
                    // expect "complete task 3"
                    var idStr = System.Text.RegularExpressions.Regex.Match(lower, "\\d+").Value;
                    if (int.TryParse(idStr, out int tid))
                    {
                        _tasksRepo.CompleteTask(tid);
                        return $"Marked task {tid} as completed.";
                    }
                    return "Please specify the task id to complete, e.g. 'complete task 3'";
                }

                if (lower.StartsWith("delete task") || lower.StartsWith("remove task"))
                {
                    var idStr = System.Text.RegularExpressions.Regex.Match(lower, "\\d+").Value;
                    if (int.TryParse(idStr, out int tid))
                    {
                        _tasksRepo.DeleteTask(tid);
                        return $"Deleted task {tid}.";
                    }
                    return "Please specify the task id to delete, e.g. 'delete task 3'";
                }
            }
            // Handle requests for topics in flexible forms
            if (IsTopicsRequest(lower))
            {
                return GetTopics();
            }

            // Handle tip requests
            if (IsTipRequest(lower))
            {
                return GetRandomTip();
            }

            // Detect if user expresses interest in a topic ("I am interested in X") and remember it
            var interest = ExtractFavouriteFromText(lower);
            if (!string.IsNullOrEmpty(interest))
            {
                Memory.FavouriteTopic = interest;
                return FormatPersonal($"Got it — I'll remember that you're interested in {interest}.");
            }

            // If the user mentions a known topic anywhere in the sentence, start explaining that topic in chunks.
            var topicFound = FindTopicInText(lower);
            if (!string.IsNullOrEmpty(topicFound))
            {
                return StartTopicExplanation(topicFound);
            }

            // Enhanced conversational intents
            var sadKeywords = new[] { "sad", "depressed", "lonely", "not okay", "upset", "down", "miserable", "unhappy" };
            var sickKeywords = new[] { "sick", "ill", "tired", "hurt", "unwell", "exhausted" };
            var scamKeywords = new[] { "scam", "scammed", "scamming", "scammed me", "scammed", "worried", "stressed", "anxious" };
            var happyKeywords = new[] { "happy", "excited", "celebrating", "great", "awesome", "yay" };
            var casualKeywords = new[] { "bored", "tell me a joke", "joke", "what's up", "whats up", "sup", "what\"s up" };

            if (MatchesAny(lower, sadKeywords))
            {
                var empathies = new[]
                {
                    "I'm really sorry you're feeling this way.",
                    "That sounds tough — I'm here for you.",
                    "I'm sorry that happened. You don't have to go through it alone."
                };
                var lightJokes = new[]
                {
                    "Why don't scientists trust atoms? Because they make up everything. (I know, it's cheesy — but maybe it'll get a smile.)",
                    "I would tell you a construction joke, but I'm still working on it."
                };
                var resp = PickRandom(empathies) + " " + PickRandom(lightJokes);
                return FormatPersonal(resp);
            }


            if (MatchesAny(lower, sickKeywords))
            {
                var responses = new[]
                {
                    "I'm sorry you're not feeling well. Rest is important — drink some fluids and take it easy.",
                    "That sounds rough. Want me to tell you a silly joke to distract you for a minute?",
                    "Take care of yourself — if symptoms persist consider seeking medical advice."
                };
                return FormatPersonal(PickRandom(responses));
            }

            if (MatchesAny(lower, scamKeywords))
            {
                var responses = new[]
                {
                    "I'm sorry that happened. First, stop communicating with the sender and don't click links. Consider reporting the scam and updating your passwords.",
                    "That must be stressful. If money was involved contact your bank and report it; change passwords and enable 2FA where possible.",
                    "Take a deep breath — document what happened, report the incident, and block the sender. I'm here to help with next steps."
                };
                return FormatPersonal(PickRandom(responses));
            }

            if (MatchesAny(lower, happyKeywords))
            {
                var responses = new[]
                {
                    "That's awesome — congrats! 🎉",
                    "So happy for you! Tell me more if you want to share.",
                    "Love to hear that — keep enjoying it!"
                };
                return FormatPersonal(PickRandom(responses));
            }

            if (MatchesAny(lower, casualKeywords))
            {
                if (lower.Contains("joke"))
                {
                    var jokes = new[]
                    {
                        "Why did the scarecrow win an award? Because he was outstanding in his field!",
                        "I told my computer I needed a break, and it said 'No problem, I'll go to sleep.'"
                    };
                    return PickRandom(jokes);
                }

                var casual = new[]
                {
                    "Not much, just hanging out in code. What about you?",
                    "I'm here! Want a fun fact or a joke?",
                    "Let's do something — I can show topics, give a tip, or tell a joke."
                };
                return PickRandom(casual);
            }

            // Sentiment categories requested: worried, curious, frustrated, happy
            var worriedKeywords = new[] { "worried", "scared", "anxious", "afraid" };
            var curiousKeywords = new[] { "curious", "wondering", "want to know", "interested in" };
            var frustratedKeywords = new[] { "frustrated", "annoyed", "confused", "irritated" };
            var happyKeywords2 = new[] { "great", "awesome", "love it", "fantastic" };

            if (MatchesAny(lower, worriedKeywords))
            {
                var replies = new[]
                {
                    "I'm sorry you're feeling worried. Take a deep breath — would you like some practical steps or just someone to listen?",
                    "That sounds stressful. I'm here to help — do you want tips to stay safe or ways to reduce stress?"
                };
                return FormatPersonal(PickRandom(replies));
            }

            if (MatchesAny(lower, curiousKeywords))
            {
                var replies = new[]
                {
                    "Great question — I can explain that. Which part are you most curious about?",
                    "I'd love to help you learn more — do you want a brief overview or a detailed explanation?"
                };
                return FormatPersonal(PickRandom(replies));
            }

            if (MatchesAny(lower, frustratedKeywords))
            {
                var replies = new[]
                {
                    "I get that — tech and security can be confusing. Tell me what's blocking you and I'll try to simplify it.",
                    "Sorry that you're feeling frustrated. Want a step-by-step guide or a quick summary to make it easier?"
                };
                return FormatPersonal(PickRandom(replies));
            }

            if (MatchesAny(lower, happyKeywords2))
            {
                var replies = new[]
                {
                    "That's wonderful to hear! Keep it up!",
                    "Yay — so happy for you! Want to tell me more?"
                };
                return FormatPersonal(PickRandom(replies));
            }

            // numeric shortcut like '1' or '2-topic'
            var dashParts = lower.Split(new[] { '-' }, 2);
            if (dashParts.Length > 0 && int.TryParse(dashParts[0].Trim(), out int idx))
            {
                if (idx >= 1 && idx <= _topicKeys.Count)
                {
                    var key = _topicKeys[idx - 1];
                    return FormatAnswer(key, _answers[key]);
                }
            }

            // Exact match or substring match against topics
            if (_answers.TryGetValue(lower, out var ans)) return FormatAnswer(lower, ans);
            var best = _topicKeys.FirstOrDefault(k => k.IndexOf(lower, StringComparison.OrdinalIgnoreCase) >= 0);
            if (best != null) return FormatAnswer(best, _answers[best]);

            // General conversational shortcuts
            if (lower.Contains("hi") || lower.Contains("hello") || lower.Contains("hey"))
            {
                return $"Hi {Memory.UserName}! How can I help you today?";
            }

            if (lower.Contains("thank") || lower.Contains("thanks"))
            {
                return "You're welcome! If you need anything else type 'topics' or ask a question.";
            }

            // If nothing matched, provide a helpful fallback
            return $"{Memory.UserName}, I didn’t quite understand that. Type 'topics' to see what I can explain, choose a number like '1' or type a topic name.";
        }

        private bool IsTopicsRequest(string lower)
        {
            // common phrasing variants
            if (lower.Contains("topic")) return true;
            if (lower.Contains("show") && lower.Contains("topic")) return true;
            if (lower.Contains("display") && lower.Contains("topic")) return true;
            if (lower.Contains("list") && lower.Contains("topic")) return true;
            if (lower.StartsWith("topics") || lower == "topics" || lower == "show topics") return true;
            return false;
        }

        private bool IsTipRequest(string lower)
        {
            if (lower.Contains("tip")) return true;
            if (lower.Contains("give me a tip") || lower.Contains("security tip") || lower.Contains("give me a tip")) return true;
            return false;
        }

        private bool ContainsSadness(string lower)
        {
            var sadKeywords = new[] { "sad", "unhappy", "depressed", "something bad", "bad happened", "sadder", "miserable", "down", "not good", "hurt", "lonely" };
            return sadKeywords.Any(k => lower.Contains(k));
        }

        private bool TryExtractTopicFromExplainRequest(string lower, out string topicKey)
        {
            topicKey = null;
            // common explain intents
            var patterns = new[] { "explain ", "explain:", "tell me about ", "what is ", "what's ", "define ", "describe " };
            foreach (var p in patterns)
            {
                if (lower.StartsWith(p))
                {
                    var candidate = lower.Substring(p.Length).Trim(' ', '?', '.');
                    // try exact match in topics
                    var found = _topicKeys.FirstOrDefault(k => k.Equals(candidate, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        topicKey = found;
                        return true;
                    }
                    // try substring match
                    found = _topicKeys.FirstOrDefault(k => k.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (found != null)
                    {
                        topicKey = found;
                        return true;
                    }
                }
            }

            // also handle phrases that contain the explain intent anywhere
            foreach (var p in patterns)
            {
                var idx = lower.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var candidate = lower.Substring(idx + p.Length).Trim(' ', '?', '.');
                    var found = _topicKeys.FirstOrDefault(k => k.Equals(candidate, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                    {
                        topicKey = found;
                        return true;
                    }
                    found = _topicKeys.FirstOrDefault(k => k.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (found != null)
                    {
                        topicKey = found;
                        return true;
                    }
                }
            }

            return false;
        }

        private string FormatAnswer(string key, string answer)
        {
            var prefix = string.Empty;
            if (!string.IsNullOrEmpty(Memory.FavouriteTopic))
            {
                prefix = $"As someone interested in {Memory.FavouriteTopic}, ";
            }
            return $"{Memory.UserName}, {prefix}here's what I found about {key}:\n{answer}";
        }

        private string ExtractFavouriteFromText(string lower)
        {
            // phrases like "i am interested in X" or "i'm interested in X" or "i am interested in phishing"
            var markers = new[] { "i am interested in ", "i'm interested in ", "im interested in ", "i am interested in:", "i'm interested in:" };
            foreach (var m in markers)
            {
                var idx = lower.IndexOf(m, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var candidate = lower.Substring(idx + m.Length).Trim(' ', '.', '?', '!');
                    // try find a topic key that matches candidate
                    var found = _topicKeys.FirstOrDefault(k => k.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (found != null) return found;
                    // if not exact, return the candidate text as provided
                    return candidate;
                }
            }
            return null;
        }

        private string FindTopicInText(string lower)
        {
            if (string.IsNullOrEmpty(lower)) return null;
            // Look for exact topic key words first
            foreach (var k in _topicKeys)
            {
                if (lower.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return k;
                }
                // also try splitting on spaces and punctuation to match multi-word keys
                var words = k.Split(' ');
                if (words.All(w => lower.Contains(w)))
                {
                    return k;
                }
            }
            return null;
        }

        private bool MatchesAny(string lower, string[] keywords)
        {
            return keywords.Any(k => lower.Contains(k));
        }

        private string PickRandom(string[] arr)
        {
            if (arr == null || arr.Length == 0) return string.Empty;
            return arr[_rand.Next(arr.Length)];
        }

        private string FormatPersonal(string text)
        {
            if (string.IsNullOrEmpty(Memory.UserName) || Memory.UserName == "Guest") return text;
            return $"{Memory.UserName}, {text}";
        }
    }
}
