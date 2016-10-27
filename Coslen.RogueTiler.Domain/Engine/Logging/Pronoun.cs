using System;

namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    [Serializable]
    public class Pronoun
    {
        public string Nominative { get; set; }
        public string Objective { get; set; }
        public string PossessiveDeterminer { get; set; }
        public string PossessivePronoun { get; set; }
        public string Reflective { get; set; }
    }
}