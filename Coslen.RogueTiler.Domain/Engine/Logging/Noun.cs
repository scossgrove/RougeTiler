using System;

namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    [Serializable]
    public class Noun
    {
        public Noun(string nounText)
        {
            NounText = nounText;
        }

        public string NounText { get; protected set; }

        public Pronoun Pronoun
        {
            get { return Pronouns.It; }
        }

        public override string ToString()
        {
            return NounText;
        }

        public static implicit operator Noun(string source)
        {
            return new Noun(source);
        }
    }
}