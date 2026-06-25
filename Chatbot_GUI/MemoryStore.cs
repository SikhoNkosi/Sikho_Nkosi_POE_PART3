namespace Chatbot_GUI
{
    // Simple in-memory memory for the GUI chatbot session.
    public class MemoryStore
    {
        public string UserName { get; set; } = "Guest";
        public string FavouriteTopic { get; set; }
        // The last topic the bot explained (key from topic list). Used to support 'tell me more' requests.
        public string LastTopic { get; set; }
        // Cursor into the last topic's lines for incremental 'more' responses.
        public int TopicOffset { get; set; }
        // Interactive task creation state
        public bool IsCreatingTask { get; set; }
        public int PendingTaskId { get; set; }
        public string PendingTaskTitle { get; set; }
        public bool AwaitingReminderSpecification { get; set; }
    }
}
