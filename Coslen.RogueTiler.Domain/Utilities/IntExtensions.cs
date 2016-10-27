namespace Coslen.RogueTiler.Domain.Utilities
{
    public static class IntExtensions
    {
        public static int Clamp(this int value, int min, int max)
        {
            var result = value;
            if (result < min)
            {
                result = min;
            }

            if (result > max)
            {
                result = max;
            }

            return result;
        }
    }
}