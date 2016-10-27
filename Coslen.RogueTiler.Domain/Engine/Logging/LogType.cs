namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    public class LogType
    {
        /// Normal log messages.
        public static LogType Message = new LogType("message");

        /// Messages when the player tries an invalid action.
        public static LogType Error = new LogType("error");

        /// Messages related to the hero's quest.
        public static LogType Quest = new LogType("quest");

        /// Messages when the hero levels up or gains powers.
        public static LogType Gain = new LogType("gain");

        /// Help or tutorial messages.
        public static LogType Help = new LogType("help");

        public LogType(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}