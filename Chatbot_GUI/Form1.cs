using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Chatbot_GUI
{
    public partial class Form1 : Form
    {
        private ChatBot bot;
        private TaskRepository taskRepo = new TaskRepository();
        private enum TaskFlowState { None, AwaitingTaskName, AwaitingReminder }
        private TaskFlowState taskState = TaskFlowState.None;
        private string pendingTaskTitle;
        private bool awaitingName = true;

        // Quiz state
        private QuizManager activeQuizManager;
        private List<QuizQuestion> activeQuizQuestions;
        private int quizIndex = 0;
        private int quizScore = 0;
        private bool quizActive = false;

        // NLP keywords
        private readonly HashSet<string> _addKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "add task", "add a task", "new task", "set a reminder", "remind me", "remind" };
        private readonly HashSet<string> _showKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "show my tasks", "show tasks", "list tasks", "my tasks" };
        private readonly HashSet<string> _completeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "complete task", "mark complete", "complete" };
        private readonly HashSet<string> _deleteKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "delete task", "remove task", "delete" };
        private readonly HashSet<string> _implicitAddKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "password", "update password", "change password" };
        private readonly HashSet<string> _activityKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "show activity log", "activity log", "what have you done", "what have you done for me", "what have you done for me?", "show log" };

        // Activity log entries (most recent first)
        private class ActivityEntry { public DateTime Time; public string Description; }
        private readonly List<ActivityEntry> _activityLog = new List<ActivityEntry>();
        private bool _lastActivityViewFull = false;

        private void LogActivity(string description)
        {
            try
            {
                _activityLog.Insert(0, new ActivityEntry { Time = DateTime.Now, Description = description });
                // keep reasonable cap for memory (optional)
                if (_activityLog.Count > 500) _activityLog.RemoveRange(500, _activityLog.Count - 500);
            }
            catch { }
        }

        private void ShowActivityLog(bool showAll = false)
        {
            if (_activityLog.Count == 0)
            {
                AddChatBubble("No recent activity to show.", false);
                return;
            }

            int take = showAll ? _activityLog.Count : Math.Min(10, _activityLog.Count);
            var lines = new List<string>();
            lines.Add(showAll ? "Activity log (full):" : "Recent activity:");
            for (int i = 0; i < take; i++)
            {
                var e = _activityLog[i];
                lines.Add($"{i + 1}. {e.Time:g}: {e.Description}");
            }
            if (!showAll && _activityLog.Count > take) lines.Add("Type 'show more' to view the full activity history.");
            AddChatBubble(string.Join("\n", lines), false);
            _lastActivityViewFull = showAll;
        }

        public Form1()
        {
            InitializeComponent();
            bot = new ChatBot();
            bot.Memory.UserName = "Guest";
        }

        private int ExtractFirstInt(string input)
        {
            var m = Regex.Match(input, "\\d+");
            if (m.Success && int.TryParse(m.Value, out int v)) return v;
            return -1;
        }

        private string ExtractTitleFromPhrase(string input)
        {
            // Try to remove common verbs like 'remind me to' or 'remind me' or 'please' and return remainder as title
            var s = input.Trim();
            // patterns: remind me to X, remind me X, can you remind me to X, please remind me to X
            var m = Regex.Match(s, "remind me to (.+)$", RegexOptions.IgnoreCase);
            if (m.Success) return CapitalizeFirst(m.Groups[1].Value.Trim());
            m = Regex.Match(s, "remind me (to )?(.+)$", RegexOptions.IgnoreCase);
            if (m.Success) return CapitalizeFirst(m.Groups[m.Groups.Count - 1].Value.Trim());
            // look for 'to' phrase
            m = Regex.Match(s, "to (update|change|reset) (your )?(password|passwd|pass)", RegexOptions.IgnoreCase);
            if (m.Success) return "Update password";
            // fallback: no extracted title
            return null;
        }

        private string CapitalizeFirst(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        private void TestYourselfButton_Click(object sender, EventArgs e)
        {
            // Start quiz inline in chat UI
            activeQuizManager = new QuizManager();
            activeQuizQuestions = new List<QuizQuestion>(activeQuizManager.RunQuestions());
            quizIndex = 0;
            quizScore = 0;
            quizActive = true;
            // send first question
            SendNextQuizQuestion();
            LogActivity("Quiz started");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // On load, prompt for name
            AddChatBubble(" greeting please enter your name", false);

            // Play greeting audio asynchronously (non-blocking)
            Task.Run(() => bot.VoiceGreeting());

            // Populate the static ASCII art label on the right side
            try
            {
                var art = bot.GetAsciiArt();
                if (!string.IsNullOrEmpty(art) && this.AsciiLabel != null)
                {
                    // Extract only the line that contains the visible title "Cyber Avengers"
                    var lines = art.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var titleIndex = Array.FindIndex(lines, l => l.IndexOf("Cyber Avengers", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (titleIndex >= 0)
                    {
                        // Keep the original title line in AsciiLabel
                        var titleLine = lines[titleIndex].Trim();
                        this.AsciiLabel.Text = titleLine;

                        // Place a small robot-face snippet to the right in AsciiMini
                        int faceStart = Math.Max(0, titleIndex - 5);
                        int faceEnd = Math.Min(titleIndex, faceStart + 4);
                        var faceLines = lines.Skip(faceStart).Take(faceEnd - faceStart).Select(l => l.Trim());
                        if (this.AsciiMini != null)
                        {
                            this.AsciiMini.Text = string.Join(Environment.NewLine, faceLines);
                        }
                    }
                    else
                    {
                        this.AsciiLabel.Text = "Cyber Avengers";
                    }
                }
            }
            catch { }
        }

        private void AddChatBubble(string text, bool isUser)
        {
            if (this.ChatPanel.InvokeRequired)
            {
                this.ChatPanel.Invoke(new Action(() => AddChatBubble(text, isUser)));
                return;
            }

            var bubble = new ChatBubble()
            {
                Message = text,
                IsUser = isUser,
            };

            // Ensure max width respects panel size
            bubble.Width = Math.Min( (int)(this.ChatPanel.ClientSize.Width * 0.65), 600);
            bubble.Update();

            // Container panel to allow left/right alignment
            var container = new Panel()
            {
                Width = this.ChatPanel.ClientSize.Width - 20,
                Height = bubble.Height + 6,
                Margin = new Padding(0),
            };

            if (isUser)
            {
                bubble.Left = container.Width - bubble.Width - 10;
            }
            else
            {
                bubble.Left = 10;
            }

            bubble.Top = 3;
            container.Controls.Add(bubble);
            this.ChatPanel.Controls.Add(container);
            this.ChatPanel.ScrollControlIntoView(container);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            var input = this.UserInput.Text?.Trim();
            if (string.IsNullOrEmpty(input)) return;

            if (awaitingName)
            {
                bot.Memory.UserName = input;
                AddChatBubble(input, true);
                awaitingName = false;
                AddChatBubble($"Nice to meet you, {bot.Memory.UserName}! How can I help you today?", false);
                this.UserInput.Clear();
                LogActivity("Viewed tasks list");
                return;
            }

            AddChatBubble(input, true);

            // If a quiz is active, handle answers inline
            if (quizActive)
            {
                HandleQuizAnswer(input);
                this.UserInput.Clear();
                return;
            }

            // Task flow and NLP-based commands
            var lower = input.ToLowerInvariant();

            // show tasks
            if (_showKeywords.Any(k => lower.Contains(k)))
            {
                var all = taskRepo.GetAllTasks();
                if (all == null || all.Count == 0)
                {
                    AddChatBubble("You have no tasks.", false);
                }
                else
                {
                    var sb = "Here are your tasks:";
                    foreach (var t in all)
                    {
                        sb += $"\n{t.TaskID}. {t.Title} (Reminder: {(t.ReminderDate.HasValue ? t.ReminderDate.Value.ToString() : "None")}, Completed: {t.IsCompleted})";
                    }
                    AddChatBubble(sb, false);
                }
                this.UserInput.Clear();
                return;
            }

            // complete tasks e.g. "complete task 1" or "mark complete 1"
            if (_completeKeywords.Any(k => lower.Contains(k)))
            {
                var id = ExtractFirstInt(input);
                if (id > 0)
                {
                    taskRepo.CompleteTask(id);
                    AddChatBubble($"Task {id} marked as completed.", false);
                    LogActivity($"Task {id} marked as completed");
                }
                else
                {
                    AddChatBubble("Please specify the task ID to mark complete. Example: 'complete task 1'", false);
                }
                this.UserInput.Clear();
                return;
            }

            // delete tasks
            if (_deleteKeywords.Any(k => lower.Contains(k)))
            {
                var id = ExtractFirstInt(input);
                if (id > 0)
                {
                    taskRepo.DeleteTask(id);
                    AddChatBubble($"Task {id} deleted.", false);
                    LogActivity($"Task {id} deleted");
                }
                else
                {
                    AddChatBubble("Please specify the task ID to delete. Example: 'delete task 1'", false);
                }
                this.UserInput.Clear();
                return;
            }

            // add task triggers (explicit or implicit)
            if (_addKeywords.Any(k => lower.Contains(k)) || _implicitAddKeywords.Any(k => lower.Contains(k)))
            {
                // if user already included a suggested title (e.g., "remind me to update my password"), try to extract title
                var title = ExtractTitleFromPhrase(input);
                if (!string.IsNullOrEmpty(title))
                {
                    pendingTaskTitle = title;
                    taskState = TaskFlowState.AwaitingReminder;
                    AddChatBubble($"Task added: {pendingTaskTitle}. Would you like to set a reminder? When should I remind you?", false);
                    LogActivity($"Task added: {pendingTaskTitle} (pending reminder)");
                    this.UserInput.Clear();
                    return;
                }

                taskState = TaskFlowState.AwaitingTaskName;
                AddChatBubble("Sure — what should I name this task?", false);
                this.UserInput.Clear();
                return;
            }

            // handle task flow states
            if (taskState == TaskFlowState.AwaitingTaskName)
            {
                pendingTaskTitle = input;
                taskState = TaskFlowState.AwaitingReminder;
                AddChatBubble($"When should I remind you about '{pendingTaskTitle}'? You can say things like 'in 3 hours' or 'in 20 minutes' or provide a date/time.", false);
                this.UserInput.Clear();
                return;
            }

            if (taskState == TaskFlowState.AwaitingReminder)
            {
                var parsed = TryParseRelativeTime(input, out DateTime remindAt);
                if (!parsed)
                {
                    // try absolute parse
                    if (!DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.None, out remindAt))
                    {
                        AddChatBubble("Sorry, I couldn't understand that time. Try 'in 3 hours', 'in 20 minutes', 'tomorrow 9am', or a date like 2024-12-31 14:30.", false);
                        this.UserInput.Clear();
                        return;
                    }
                }

                int id = taskRepo.AddTask(pendingTaskTitle, string.Empty, remindAt);
                AddChatBubble($"Saved task '{pendingTaskTitle}' with reminder at {remindAt} (ID: {id}).", false);
                LogActivity($"Task added: {pendingTaskTitle} (Reminder set: {remindAt:g})");
                // reset state
                pendingTaskTitle = null;
                taskState = TaskFlowState.None;
                this.UserInput.Clear();
                return;
            }

            // default: normal bot response
            // activity log request
            if (_activityKeywords.Any(k => lower.Contains(k)))
            {
                // support 'show more' after a recent view
                if (lower.Contains("show more") || lower.Contains("show all") || lower.Contains("full"))
                {
                    ShowActivityLog(true);
                }
                else
                {
                    ShowActivityLog(false);
                }
                LogActivity("Viewed activity log");
                this.UserInput.Clear();
                return;
            }

            var resp = bot.GetResponse(input);
            AddChatBubble(resp, false);
            this.UserInput.Clear();
        }

        private void greetButton_Click(object sender, EventArgs e)
        {
            AddChatBubble("Playing greeting audio (if available)...", false);
            Task.Run(() => bot.VoiceGreeting());
        }

        private void SendNextQuizQuestion()
        {
            if (activeQuizQuestions == null || quizIndex >= activeQuizQuestions.Count)
            {
                quizActive = false;
                AddChatBubble($"Quiz finished. Score: {quizScore}/{activeQuizQuestions?.Count ?? 0}", false);
                activeQuizQuestions = null;
                return;
            }

            var q = activeQuizQuestions[quizIndex];
            if (q.IsTrueFalse)
            {
                AddChatBubble($"Question {quizIndex + 1}: {q.Question} (Reply 'true' or 'false')", false);
            }
            else
            {
                var text = $"Question {quizIndex + 1}: {q.Question}\n";
                for (int i = 0; i < q.Choices.Length; i++) text += $"{i + 1}. {q.Choices[i]}\n";
                AddChatBubble(text.TrimEnd('\n'), false);
            }
        }

        private void HandleQuizAnswer(string input)
        {
            if (activeQuizQuestions == null || quizIndex >= activeQuizQuestions.Count) return;
            var q = activeQuizQuestions[quizIndex];
            bool correct = false;

            if (q.IsTrueFalse)
            {
                var lowered = input.Trim().ToLowerInvariant();
                int sel = (lowered.StartsWith("t") || lowered.StartsWith("y")) ? 0 : 1; // true -> index 0, false -> index 1
                correct = sel == q.CorrectIndex;
            }
            else
            {
                // accept numeric choice or exact text
                if (int.TryParse(input.Trim(), out int num) && num >= 1 && num <= q.Choices.Length)
                {
                    correct = (num - 1) == q.CorrectIndex;
                }
                else
                {
                    // try matching text
                    var match = q.Choices.Select((c, idx) => new { c, idx }).FirstOrDefault(x => string.Equals(x.c, input.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (match != null) correct = match.idx == q.CorrectIndex;
                }
            }

            if (correct)
            {
                quizScore++;
                AddChatBubble("Correct!", false);
            }
            else
            {
                AddChatBubble($"Incorrect. Correct answer: {(q.IsTrueFalse ? (q.CorrectIndex == 0 ? "True" : "False") : q.Choices[q.CorrectIndex])}", false);
            }

            quizIndex++;
            SendNextQuizQuestion();
        }

        // Try to parse inputs like "in 3 hours", "in 20 minutes", "in 5 days"
        private bool TryParseRelativeTime(string input, out DateTime result)
        {
            result = DateTime.MinValue;
            var s = input.Trim().ToLowerInvariant();
            // match patterns like "in 3 hours", "in 20 minutes", "in 5 days"
            var m = Regex.Match(s, "in\\s+(\\d+)\\s*(second|seconds|minute|minutes|hour|hours|day|days|week|weeks)");
            if (!m.Success) return false;
            if (!int.TryParse(m.Groups[1].Value, out int amount)) return false;
            var unit = m.Groups[2].Value;
            var now = DateTime.Now;
            switch (unit)
            {
                case "second":
                case "seconds":
                    result = now.AddSeconds(amount);
                    return true;
                case "minute":
                case "minutes":
                    result = now.AddMinutes(amount);
                    return true;
                case "hour":
                case "hours":
                    result = now.AddHours(amount);
                    return true;
                case "day":
                case "days":
                    result = now.AddDays(amount);
                    return true;
                case "week":
                case "weeks":
                    result = now.AddDays(7 * amount);
                    return true;
            }
            return false;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch
            {
                Application.Exit();
            }
        }
    }
}
