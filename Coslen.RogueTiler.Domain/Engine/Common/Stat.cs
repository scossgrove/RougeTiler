namespace Coslen.RogueTiler.Domain.Engine.Common
{
    public class Stat
    {
        private double current;

        private double max;

        public Stat(double value)
        {
            current = value;
            max = value;
        }

        public Stat(double current, double max)
        {
            this.current = current;
            this.max = max;
        }

        public double Current
        {
            get { return current; }
            set
            {
                current = value;
                if (current < 0)
                {
                    current = 0;
                }
                if (current > max)
                {
                    current = max;
                }
            }
        }

        public double Max
        {
            get { return max; }
            set
            {
                max = value;
                if (max < 0)
                {
                    max = 0;
                }
            }
        }

        public bool IsMax
        {
            get { return current == max; }
        }
    }
}