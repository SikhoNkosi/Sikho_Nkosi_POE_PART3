using System;
using System.Collections.Generic;
using System.Text;

namespace CyberSecurityChatbot
{
    // TopicManager holds the list of topics and their long answers.
    // It also contains the fuzzy matching helpers (Levenshtein, stop words)
    // moved here from Program.cs so the Chatbot class focuses on interaction.
    internal class TopicManager
    {
        // Public read-only list of topic keys (keeps insertion order so users can
        // select by number). Exposed so the UI code can enumerate topics.
        public List<string> TopicKeys { get; } = new List<string>();

        // Map a topic key to its multi-line answer. Case-insensitive for convenience.
        private readonly Dictionary<string, string> topicAnswers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public TopicManager()
        {
            InitializeTopics();
        }

        // Allows caller to get the answer for a key if it exists.
        public bool TryGetAnswer(string key, out string answer)
        {
            return topicAnswers.TryGetValue(key, out answer);
        }

        // Fuzzy match: find the best topic match for an input string.
        // Returns the matched key or null if none is close enough.
        public string FindBestTopicMatch(string input, out int bestDistance)
        {
            bestDistance = int.MaxValue;
            string bestKey = null;
            if (string.IsNullOrWhiteSpace(input)) return null;

            // create a cleaned version of the input by removing common question words
            string cleaned = RemoveStopWords(input.ToLowerInvariant());

            foreach (var key in TopicKeys)
            {
                string keyLower = key.ToLowerInvariant();

                // distance against cleaned input and full input
                int dClean = LevenshteinDistance(cleaned, keyLower);
                int dFull = LevenshteinDistance(input.ToLowerInvariant(), keyLower);
                int d = Math.Min(dClean, dFull);

                // also compute best per-word distance between cleaned input and key words
                int bestWordDist = int.MaxValue;
                var inputWords = cleaned.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var keyWords = keyLower.Split(' ');
                foreach (var iw in inputWords)
                {
                    foreach (var kw in keyWords)
                    {
                        int wd = LevenshteinDistance(iw, kw);
                        if (wd < bestWordDist) bestWordDist = wd;
                    }
                }

                // combine metrics: prefer small word-level matches
                int effective = Math.Min(d, bestWordDist + 1);

                if (effective < bestDistance)
                {
                    bestDistance = effective;
                    bestKey = key;
                }
            }

            // Heuristic: accept if distance small relative to key length or very small absolute
            if (bestKey != null)
            {
                int len = Math.Max(1, bestKey.Length);
                if (bestDistance <= 2 || bestDistance <= Math.Max(2, len / 4))
                {
                    return bestKey;
                }
            }

            return null;
        }

        // --- Helpers moved from Program.cs ---
        private string RemoveStopWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            string[] stopWords = new[] { "what", "is", "the", "a", "an", "please", "tell", "me", "about", "can", "you", "i", "want", "to", "explain", "define", "of", "for" };
            var parts = input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var kept = new List<string>();
            foreach (var p in parts)
            {
                var w = p.Trim(new char[] { '?', '.', '!', ',' }).ToLowerInvariant();
                if (w.Length == 0) continue;
                if (Array.IndexOf(stopWords, w) >= 0) continue;
                kept.Add(w);
            }
            return string.Join(' ', kept);
        }

        private int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
            if (string.IsNullOrEmpty(b)) return a.Length;

            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[a.Length, b.Length];
        }

        // Initialize the topic list and the long multi-line answers.
        private void InitializeTopics()
        {
            // Helper to add a topic key and its answer
            void Add(string key, string answer)
            {
                TopicKeys.Add(key);
                topicAnswers[key] = answer;
            }

            Add("phishing",
                "Phishing is a type of social engineering attack where attackers craft deceptive messages to trick you into revealing personal information or clicking malicious links.\n"
                + "These messages often impersonate trusted organizations and use urgent language to provoke action.\n"
                + "Common indicators include unexpected requests, poor spelling/grammar, and mismatched URLs.\n"
                + "If you suspect a message is phishing, do not click links or open attachments; verify the sender by other channels.\n"
                + "Organizations should use email authentication (SPF, DKIM, DMARC) and security awareness training to reduce risk.\n"
                + "On a personal level, enable multi-factor authentication and use unique passwords to limit damage from credential theft.\n"
                + "Always report suspicious messages to your IT team or email provider so they can block the threat.");

            Add("password safety",
                "Password safety means using strong, unique passwords for each account and changing them when compromise is suspected.\n"
                + "A strong password combines length and complexity — passphrases are often easier to remember and more secure.\n"
                + "Never reuse passwords across important services; reuse makes lateral compromise trivial for attackers.\n"
                + "Use a reputable password manager to generate and store credentials securely rather than writing them down.\n"
                + "Wherever possible, enable multi-factor authentication to add an additional layer beyond passwords.\n"
                + "Review account activity regularly and remove stale accounts to reduce the attack surface.\n"
                + "Consider periodic password audits and rotating credentials used by services or applications.");

            Add("two-factor authentication",
                "Two-factor authentication (2FA) requires a second form of verification in addition to a password.\n"
                + "Common factors include SMS codes, authenticator apps, hardware tokens, or biometric checks.\n"
                + "Authenticator apps and hardware tokens are generally more secure than SMS, which can be intercepted.\n"
                + "2FA mitigates risks from stolen credentials because an attacker needs both the password and the second factor.\n"
                + "Organizations should make 2FA available for all sensitive accounts and prioritize hardware-based keys for admins.\n"
                + "Educate users about phishing attempts that try to capture 2FA codes and how to report suspicious prompts.\n"
                + "Implement account recovery procedures that are secure and avoid social-engineering weaknesses.");

            Add("malware",
                "Malware is software designed to harm or exploit systems, including viruses, trojans, spyware, and worms.\n"
                + "It can steal data, encrypt files for ransom, or provide remote control to attackers.\n"
                + "Common infection vectors include malicious email attachments, compromised websites, and unpatched software.\n"
                + "Defenses include endpoint protection, regular patching, least-privilege accounts, and network segmentation.\n"
                + "Backups and incident response plans reduce the impact of malware like ransomware.\n"
                + "User education about suspicious downloads and attachments also significantly lowers risk.\n"
                + "Monitor systems for unusual activity and use threat intelligence to detect known indicators of compromise.");

            Add("ransomware",
                "Ransomware is a form of malware that encrypts files and demands payment for the decryption key.\n"
                + "It often spreads through phishing, exposed remote services, or compromised remote desktop protocols.\n"
                + "Preventive measures include offline backups, patching, network segmentation, and restricting administrative privileges.\n"
                + "Organizations should practice incident response procedures and have an isolated recovery environment.\n"
                + "Paying a ransom does not guarantee data recovery and may fund further criminal activity.\n"
                + "Legal and regulatory considerations may affect the decision to pay; engage legal counsel and law enforcement.\n"
                + "Regularly test backups to ensure recovery works and perform tabletop exercises for ransomware scenarios.");

            Add("social engineering",
                "Social engineering exploits human psychology rather than technical vulnerabilities to gain trust or sensitive information.\n"
                + "Attackers impersonate colleagues, create urgency, or use flattery to lower guard and prompt risky actions.\n"
                + "Security awareness training with realistic simulations reduces susceptibility to these manipulations.\n"
                + "Policies such as verifying identity before transactions and clear escalation paths help prevent fraud.\n"
                + "Multi-channel verification and strict data-handling procedures mitigate social engineering attacks.\n"
                + "Incident reporting and a culture that supports questions about suspicious requests are critical defenses.\n"
                + "Always verify unusual requests through a trusted, independent channel before acting.");

            Add("vpns",
                "Virtual Private Networks (VPNs) create an encrypted tunnel between a device and a network, protecting traffic from eavesdroppers.\n"
                + "Use reputable VPN solutions with strong cryptography and proper authentication to avoid false security.\n"
                + "Be cautious with free VPN services that may log or sell data; enterprise solutions should be centrally managed.\n"
                + "VPNs help secure remote work, but do not replace endpoint security or network segmentation.\n"
                + "Always combine VPN usage with device hygiene, such as patches and anti-malware, to reduce risk.\n"
                + "Monitor VPN access and require multi-factor authentication for remote connections.\n"
                + "Ensure split-tunneling settings are configured according to security requirements to avoid data leakage.");

            Add("firewalls",
                "Firewalls control incoming and outgoing network traffic based on rules and are a basic perimeter defense.\n"
                + "Next-generation firewalls add features like application awareness and threat inspection.\n"
                + "Use firewall rules that implement least privilege for network services and restrict unnecessary ports.\n"
                + "Combine perimeter firewalls with host-based firewalls for layered defense.\n"
                + "Log and monitor firewall activity to detect scans and unusual traffic patterns.\n"
                + "Keep firewall firmware updated and review rules periodically to remove stale exceptions.\n"
                + "Integrate firewalls into broader security monitoring and incident response workflows.");

            Add("secure wi-fi",
                "Secure Wi-Fi involves using strong encryption (WPA3 where possible) and avoiding legacy insecure protocols.\n"
                + "Change default administration passwords on access points and hide management interfaces from public access.\n"
                + "Segment guest networks from internal resources and limit what guests can access.\n"
                + "Ensure SSIDs do not leak sensitive information and rotate pre-shared keys on a schedule when used.\n"
                + "Use enterprise authentication (e.g., 802.1X) for strong identity-based access control in corporate environments.\n"
                + "Conduct periodic wireless audits to find rogue access points and misconfigurations.\n"
                + "Educate users about connecting only to trusted networks and the risks of open Wi-Fi.");

            Add("encryption",
                "Encryption protects data by making it unreadable without the correct keys; it is essential in transit and at rest.\n"
                + "Use TLS for web and service communications and strong algorithms with up-to-date libraries.\n"
                + "Encryption at rest protects stored data; combine with secure key management for effectiveness.\n"
                + "Beware of outdated algorithms and ensure cryptographic libraries are patched.\n"
                + "Use full-disk encryption for devices to reduce risk from lost or stolen hardware.\n"
                + "Plan for key rotation and revocation to limit exposure if keys are compromised.\n"
                + "Balance encryption with legal and regulatory requirements for data access and retention.");

            Add("backups",
                "Backups are copies of data kept to enable recovery after data loss, corruption, or ransomware.\n"
                + "Follow the 3-2-1 rule: three copies, on two different media, with at least one offsite or air-gapped.\n"
                + "Test backups regularly to verify they can be restored and meet recovery time objectives.\n"
                + "Protect backups with access controls and encryption to prevent them from being targeted.\n"
                + "Automate backup schedules and retention policies aligned with business needs.\n"
                + "Document recovery procedures and perform tabletop exercises to validate plans.\n"
                + "Keep backups immutable or offline where possible to reduce the risk of ransomware encryption.");

            Add("software updates",
                "Software updates and patching close known vulnerabilities that attackers can exploit.\n"
                + "Implement a patch management program that prioritizes critical and internet-facing systems.\n"
                + "Use automated patching where safe, and test updates in staging before production deployment.\n"
                + "Maintain an inventory of software assets to ensure nothing is left unpatched unintentionally.\n"
                + "Combine patching with compensating controls for systems that cannot be updated immediately.\n"
                + "Monitor vendor advisories and security bulletins for emergent threats that require urgent action.\n"
                + "Document rollback plans in case updates cause unexpected service disruptions.");

            Add("safe browsing",
                "Safe browsing habits reduce exposure to drive-by downloads, malicious sites, and scams.\n"
                + "Keep browsers and plugins up to date, and disable or remove unneeded extensions.\n"
                + "Use content blockers and script controls to limit attack surface from web content.\n"
                + "Verify the authenticity of websites before entering credentials and prefer HTTPS connections.\n"
                + "Avoid downloading software from unknown sources and validate checksums where provided.\n"
                + "Consider using separate browser profiles for risky activities and isolate high-privilege tasks.\n"
                + "Educate users to recognize deceptive UI patterns and indicators of compromised sites.");

            Add("identity theft",
                "Identity theft occurs when attackers steal personal data to impersonate or commit fraud.\n"
                + "Protect sensitive documents, shred physical records, and be cautious sharing personal details online.\n"
                + "Use credit monitoring and alerts to detect unusual activity on financial accounts.\n"
                + "Enable multi-factor authentication on important accounts and use strong, unique passwords.\n"
                + "Report suspicious activity to financial institutions and authorities promptly.\n"
                + "Limit data shared on social media to reduce profiling and targeted attacks.\n"
                + "Consider identity protection services if you handle high-value personal data or were compromised.");

            Add("data privacy",
                "Data privacy is about controlling how personal data is collected, used, and shared.\n"
                + "Implement data minimization, collecting only what is necessary for a purpose.\n"
                + "Use access controls and encryption to protect sensitive datasets from unauthorized access.\n"
                + "Provide transparency to users about data practices and respect consent choices.\n"
                + "Comply with applicable regulations (e.g., GDPR, CCPA) and perform privacy impact assessments.\n"
                + "Anonymize or pseudonymize data when full identifiers are not required for processing.\n"
                + "Establish retention policies and secure deletion methods to reduce long-term exposure.");

            Add("network security",
                "Network security ensures the confidentiality, integrity, and availability of networked systems.\n"
                + "Use segmentation, least-privilege routing, and access controls to limit lateral movement.\n"
                + "Deploy intrusion detection/prevention and continuous monitoring to spot anomalies.\n"
                + "Secure remote access with VPNs, zero trust models, and strong authentication.\n"
                + "Harden network devices by changing defaults, applying patches, and disabling unused services.\n"
                + "Encrypt sensitive traffic and establish logging for forensic capabilities in incidents.\n"
                + "Regularly assess the network through vulnerability scanning and penetration testing.");

            Add("iot security",
                "IoT security covers connected devices like cameras, sensors, and appliances that often have limited defenses.\n"
                + "Change default credentials, segment IoT devices onto isolated networks, and monitor their behavior.\n"
                + "Keep firmware up to date and prefer vendors that support secure update mechanisms.\n"
                + "Limit the data IoT devices collect and store, and protect any sensitive streams with encryption.\n"
                + "Inventory IoT devices and remove or replace unmanaged devices to reduce attack surface.\n"
                + "Use strong authentication and consider network-level controls to restrict device capabilities.\n"
                + "Plan for lifecycle management since many IoT devices have long uptime and infrequent updates.");

            Add("mobile security",
                "Mobile security focuses on protecting smartphones and tablets from malware and data loss.\n"
                + "Use device encryption, strong lock-screen authentication, and keep OS/apps updated.\n"
                + "Install apps only from trusted stores and review permissions before granting access.\n"
                + "Enable remote wipe and backup so you can recover or remove data from lost devices.\n"
                + "Be cautious with public Wi-Fi and use VPNs for sensitive communications.\n"
                + "Use separate work profiles for enterprise data and follow your organization's mobile policies.\n"
                + "Educate users about SMS phishing (smishing) and suspicious app behavior.");

            Add("cloud security",
                "Cloud security encompasses protecting data, applications, and services hosted by cloud providers.\n"
                + "Understand the shared responsibility model: providers secure infrastructure, customers secure data/identity.\n"
                + "Use strong identity and access management, least privilege, and multi-factor authentication.\n"
                + "Encrypt data in transit and at rest and ensure keys are managed securely.\n"
                + "Monitor cloud logs, configure alerts, and use native provider tooling for threat detection.\n"
                + "Secure APIs and automate secure configuration to avoid misconfiguration risks.\n"
                + "Regularly review third-party integrations and conduct cloud-focused penetration tests.");

            Add("zero trust",
                "Zero Trust is a security model that treats every access request as untrusted until verified.\n"
                + "It enforces strong authentication, authorization, and continuous validation for users and devices.\n"
                + "Micro-segmentation and least privilege reduce blast radius when breaches occur.\n"
                + "Adopt identity-centric controls and robust telemetry to make access decisions in real time.\n"
                + "Zero Trust is iterative: start with high-risk assets and expand controls progressively.\n"
                + "Automate policy enforcement and monitoring to maintain consistent security posture.\n"
                + "Measure outcomes with key security metrics and refine controls based on incidents and findings.");

            Add("incident response",
                "Incident response is the coordinated approach to detect, contain, and recover from security incidents.\n"
                + "Establish an incident response plan with clear roles, communication channels, and escalation paths.\n"
                + "Practice through tabletop exercises and post-incident reviews to improve readiness.\n"
                + "Collect and preserve forensic evidence while balancing business continuity needs.\n"
                + "Engage legal, PR, and external specialists as needed for complex or public incidents.\n"
                + "Restore services from trusted backups and perform root-cause analysis to prevent recurrence.\n"
                + "Maintain documentation and lessons learned to strengthen defenses over time.");

            Add("physical security",
                "Physical security protects facilities and hardware through controls like access badges and locks.\n"
                + "Restrict server room access, disable unused ports, and secure devices against theft.\n"
                + "Combine physical controls with environmental monitoring and surveillance for early detection.\n"
                + "Ensure disposal procedures securely erase data-bearing devices before disposal.\n"
                + "Train staff to recognize tailgating and unauthorized individuals.\n"
                + "Integrate physical security incidents into broader incident response processes.\n"
                + "Physical measures increase the difficulty and cost for attackers who need physical access.");
        }
    }
}
