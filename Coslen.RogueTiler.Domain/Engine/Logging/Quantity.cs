namespace Coslen.RogueTiler.Domain.Engine.Logging
{
    /// A [Noun] for a specific quantity of some thing.
    public class Quantity : Noun
    {
        public Quantity(int count, Quantifiable o) : base(string.Empty)
        {
            Count = count;
            Object = o;
        }

        public int Count { get; set; }
        public Quantifiable Object { get; set; }

        public new string NounText
        {
            get
            {
                // TODO: a/an.
                if (Count == 1)
                {
                    return $"a ${Object.Singular}";
                }

                string quantity;
                switch (Count)
                {
                    case 2:
                        quantity = "two";
                        break;
                    case 3:
                        quantity = "three";
                        break;
                    case 4:
                        quantity = "four";
                        break;
                    case 5:
                        quantity = "five";
                        break;
                    case 6:
                        quantity = "six";
                        break;
                    case 7:
                        quantity = "seven";
                        break;
                    case 8:
                        quantity = "eight";
                        break;
                    case 9:
                        quantity = "nine";
                        break;
                    case 10:
                        quantity = "ten";
                        break;
                    default:
                        quantity = Count.ToString();
                        break;
                }

                return "$quantity ${_object.plural}";
            }
        }

        public new Pronoun Pronoun
        {
            get { return Count == 1 ? Object.Pronoun : Pronouns.They; }
        }
    }
}