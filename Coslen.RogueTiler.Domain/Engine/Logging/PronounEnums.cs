namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    public static class Pronouns
    {
        // See http://en.wikipedia.org/wiki/English_personal_pronouns.
        public static Pronoun You = new Pronoun {Nominative = "you", Objective = "you", PossessiveDeterminer = "your", PossessivePronoun = "your", Reflective = "yourself"};
        public static Pronoun It = new Pronoun {Nominative = "it", Objective = "it", PossessiveDeterminer = "its", PossessivePronoun = "its", Reflective = "itself"};
        public static Pronoun He = new Pronoun {Nominative = "he", Objective = "him", PossessiveDeterminer = "his", PossessivePronoun = "his", Reflective = "himself"};
        public static Pronoun She = new Pronoun {Nominative = "she", Objective = "her", PossessiveDeterminer = "her", PossessivePronoun = "hers", Reflective = "herself"};
        public static Pronoun They = new Pronoun {Nominative = "they", Objective = "them", PossessiveDeterminer = "their", PossessivePronoun = "theirs", Reflective = "themselves"};
    }
}