using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Chatbot_GUI
{
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Choices { get; set; }
        public int CorrectIndex { get; set; }
        public bool IsTrueFalse { get; set; }
    }

    public class QuizManager
    {
        private readonly List<QuizQuestion> _questions;

        public QuizManager()
        {
            _questions = BuildQuestions();
        }

        // Return questions for inline quiz handling
        public List<QuizQuestion> RunQuestions()
        {
            return new List<QuizQuestion>(_questions);
        }

        private List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion { Question = "What should you do if you receive an email asking for your password?", Choices = new[]{"Reply with your password","Delete email","Report the email as phishing","Ignore it"}, CorrectIndex = 2 },
                new QuizQuestion { Question = "True or False: Using the same password for multiple accounts is safe.", Choices = new[]{"True","False"}, CorrectIndex = 1, IsTrueFalse = true },
                new QuizQuestion { Question = "What is two-factor authentication (2FA)?", Choices = new[]{"A second password","A second form of verification","An antivirus","A backup system"}, CorrectIndex = 1 },
                new QuizQuestion { Question = "True or False: Public Wi-Fi is always safe to use for banking.", Choices = new[]{"True","False"}, CorrectIndex = 1, IsTrueFalse = true },
                new QuizQuestion { Question = "Which of these is a sign of phishing?", Choices = new[]{"Unexpected urgency","Perfect grammar","Known sender","Custom greeting with your name"}, CorrectIndex = 0 },
                new QuizQuestion { Question = "What should you use to store many complex passwords?", Choices = new[]{"A text file","Your browser without protection","A password manager","Email yourself"}, CorrectIndex = 2 },
                new QuizQuestion { Question = "True or False: You should click links from unknown senders to see where they go.", Choices = new[]{"True","False"}, CorrectIndex = 1, IsTrueFalse = true },
                new QuizQuestion { Question = "What is ransomware?", Choices = new[]{"A security update","Malware that encrypts files for ransom","A firewall","An authentication method"}, CorrectIndex = 1 },
                new QuizQuestion { Question = "Why enable multi-factor authentication?", Choices = new[]{"It is annoying","It prevents all attacks","It adds a second layer of defence","It replaces passwords"}, CorrectIndex = 2 },
                new QuizQuestion { Question = "True or False: You should regularly back up important data.", Choices = new[]{"True","False"}, CorrectIndex = 0, IsTrueFalse = true }
            };
        }

        // Legacy modal runner left for backward compatibility; prefer RunQuestions for inline chat quiz
        public void RunQuiz(Form owner)
        {
            // fallback to original behavior
            var questions = RunQuestions();
            int score = 0;
            foreach (var q in questions)
            {
                // simple modal yes/no for true/false else pick first choice
                if (q.IsTrueFalse)
                {
                    var ans = MessageBox.Show(q.Question, "Question", MessageBoxButtons.YesNo);
                    int selected = (ans == DialogResult.Yes) ? 0 : 1;
                    if (selected == q.CorrectIndex) score++;
                }
                else
                {
                    // treat as single attempt modal (first choice)
                    // keep behavior minimal
                }
            }
        }
    }
}
