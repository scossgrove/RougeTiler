using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    /// The message log.
    public class Log
    {
        private const int MaxMessages = 6;
        public Queue<Message> Messages { get; set; } = new Queue<Message>();

        /// Parses strings that have singular and plural options and selects one of
        /// the two. Examples:
        /// 
        /// parsePlural("nothing", isPlural: false)     // "nothing"
        /// parsePlural("nothing", isPlural: true)      // "nothing"
        /// parsePlural("run[s]", isPlural: false)      // "run"
        /// parsePlural("run[s]", isPlural: true)       // "runs"
        /// parsePlural("bunn[y|ies]", isPlural: false) // "bunny"
        /// parsePlural("bunn[y|ies]", isPlural: true)  // "bunnies"
        /// 
        /// If [forcePlural] is `true`, then a trailing "s" will be added to the end
        /// if [isPlural] is `true` and [text] doesn't have any formatting.
        public static string ParsePlural(string text, bool isPlural = false, bool forcePlural = false)
        {
            var optionalSuffix = new Regex(@"(\w+)\[(\w+?)\]");
            var irregular = new Regex(@"(.*)\[([^|]+)\|([^\]]+)\]");
            // Attempts:
            // 1. (\w+)\[([^|]+)\|([^\]]+)\]
            // 2. (.*)\[([^|]+)\|([^\]]+)\]

            // If it's a regular plural word, just add an "s".
            if (forcePlural && isPlural && !text.Contains("["))
            {
                return text + "s";
            }

            // Handle verbs with optional suffixes like `close[s]`.
            while (true)
            {
                var match = optionalSuffix.Match(text);
                if (match.Success == false)
                {
                    break;
                }

                var before = text.Substring(0, match.Index);
                var after = text.Substring(match.Index + match.Length);
                if (isPlural)
                {
                    // Include the optional part.
                    text = before + match.Groups[1].Value + match.Groups[2].Value + after;
                }
                else
                {
                    // Omit the optional part.
                    text = before + match.Groups[1].Value + after;
                }

                break;
            }

            // Handle irregular verbs like `[are|is]`.
            while (true)
            {
                var match = irregular.Match(text);
                if (match.Success == false)
                {
                    break;
                }

                var before = text.Substring(0, match.Index);
                var after = text.Substring(match.Index + match.Length);

                if (isPlural)
                {
                    // Include the optional part.
                    text = before + match.Groups[1].Value + match.Groups[3].Value + after;
                }
                else
                {
                    // Omit the optional part.
                    text = before + match.Groups[1].Value + match.Groups[2].Value + after;
                }
            }

            return text;
        }

        public static string MakeVerbsAgree(string text, Pronoun pronoun)
        {
            var isPlural = pronoun != Pronouns.You && pronoun != Pronouns.They;
            return ParsePlural(text, isPlural);
        }

        public void Message(string message, params Noun[] nouns)
        {
            Add(LogType.Message, message, nouns);
        }

        public void Error(string message, params Noun[] nouns)
        {
            Add(LogType.Error, message, nouns);
        }

        public void Quest(string message, params Noun[] nouns)
        {
            Add(LogType.Quest, message, nouns);
        }

        public void Gain(string message, params Noun[] nouns)
        {
            Add(LogType.Gain, message, nouns);
        }

        public void Help(string message, params Noun[] nouns)
        {
            Add(LogType.Help, message, nouns);
        }

        public void Add(LogType type, string message, params Noun[] nouns)
        {
            message = FormatSentence(message, nouns);

            // See if it's a repeat of the last message.
            if (Messages.Count > 0)
            {
                var last = Messages.Last();

                if (last.Text == message)
                {
                    // It is, so just repeat the count.
                    last.Count++;
                    return;
                }
            }

            // It's a new message.
            Messages.Enqueue(new Message(type, message));

            if (Messages.Count > MaxMessages)
            {
                Messages.Dequeue();
            }
        }

        /// The same message can apply to a variety of subjects and objects, and it
        /// may use pronouns of various forms. For example, a hit action may want to
        /// be able to say:
        /// 
        /// * You hit the troll with your sword.
        /// * The troll hits you with its club.
        /// * The mermaid hits you with her fin.
        /// 
        /// To avoid handling all of these cases at each message site, we use a simple
        /// formatting DSL that can handle pronouns, subject/verb agreement, etc.
        /// This function takes a format string and a series of nouns (numbered from
        /// 1 through 3 and creates an appropriately cases and tensed string.
        /// 
        /// The following formatting is applied:
        /// 
        /// ### Nouns: `{#}`
        /// 
        /// A number inside curly braces expands to the name of that noun. For
        /// example, if noun 1 is a bat then `{1}` expands to `the bat`.
        /// 
        /// ### Subjective pronouns: `{# he}`
        /// 
        /// A number in curly brackets followed by `he` (with a space between)
        /// expands to the subjective pronoun for that noun. It takes into account
        /// the noun's person and gender. For example, if noun 2 is a mermaid then
        /// `{2 he}` expands to `she`.
        /// 
        /// ### Objective pronouns: `{# him}`
        /// 
        /// A number in curly brackets followed by `him` (with a space between)
        /// expands to the *objective* pronoun for that noun. It takes into account
        /// the noun's person and gender. For example, if noun 2 is a jelly then
        /// `{2 him}` expands to `it`.
        /// 
        /// ### Possessive pronouns: `{# his}`
        /// 
        /// A number in curly brackets followed by `his` (with a space between)
        /// expands to the possessive pronoun for that noun. It takes into account
        /// the noun's person and gender. For example, if noun 2 is a mermaid then
        /// `{2 his}` expands to `her`.
        /// 
        /// ### Regular verbs: `[suffix]`
        /// 
        /// A series of letters enclosed in square brackets defines an optional verb
        /// suffix. If noun 1 is second person, then the contents will be included.
        /// Otherwise they are omitted. For example, `open[s]` will result in `opens`
        /// if noun 1 is second-person (i.e. the [Hero]) or `open` if third-person.
        /// 
        /// ### Irregular verbs: `[second|third]`
        /// 
        /// Two words in square brackets separated by a pipe (`|`) defines an
        /// irregular verb. If noun 1 is second person that the first word is used,
        /// otherwise the second is. For example `[are|is]` will result in `are` if
        /// noun 1 is second-person (i.e. the [Hero]) or `is` if third-person.
        /// 
        /// ### Sentence case
        /// 
        /// Finally, the first letter in the result will be capitalized to properly
        /// sentence case it.
        private string FormatSentence(string text, params Noun[] nouns)
        {
            var result = text;
            if (nouns.Any())
            {
                var nounIndex = 0;
                foreach (var noun in nouns)
                {
                    nounIndex++;

                    if (noun != null)
                    {
                        result = result.Replace("{" + nounIndex + "}", noun.NounText);

                        // Handle pronouns.
                        result = result.Replace("{" + nounIndex + " he}", noun.Pronoun.Nominative);
                        result = result.Replace("{" + nounIndex + " him}", noun.Pronoun.Objective);
                        result = result.Replace("{" + nounIndex + " his}", noun.Pronoun.PossessiveDeterminer);
                    }
                }

                // Make the verb match the subject (which is assumed to be the first noun).
                if (nouns[0] != null)
                {
                    result = MakeVerbsAgree(result, nouns[0].Pronoun);
                }
            }
            // Sentence case it by capitalizing the first letter.
            return string.Format("{0}{1}", result[0].ToString().ToUpper(), result.Substring(1));
        }
    }
}