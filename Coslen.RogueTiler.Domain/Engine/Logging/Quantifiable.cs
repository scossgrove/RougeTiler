namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    /// A noun-like thing that can be quantified.
    public abstract class Quantifiable
    {
        public string Singular { get; set; }
        public string Plural { get; set; }
        public Pronoun Pronoun { get; set; }
    }
}