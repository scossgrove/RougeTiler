namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    /// A single log entry.
    public class Message
    {
        public Message(LogType type, string text)
        {
            Type = type;
            Text = text;
        }

        /// The number of times this message has been repeated.
        public int Count { get; set; } = 1;

        public string Text { get; private set; }
        public LogType Type { get; private set; }
    }
}