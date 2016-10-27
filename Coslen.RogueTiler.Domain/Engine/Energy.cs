namespace Coslen.RogueTiler.Domain.Engine
{
    /// Energy is used to control the rate that actors move relative to other
    /// actors. Each game turn, every actor will accumulate energy based on their
    /// speed. When it reaches a threshold, that actor can take a turn.
    public class Energy
    {
        public static int MinSpeed = 0;
        public static int NormalSpeed = 6;
        public static int MaxSpeed = 12;

        public static int ActionCost = 240;

        // How much energy is gained each game turn for each speed.
        public static int[] Gains = {15, // 1/4 normal speed
            20, // 1/3 normal speed
            25, 30, // 1/2 normal speed
            40, 50, 60, // normal speed
            80, 100, 120, // 2x normal speed
            150, 180, // 3x normal speed
            240 // 4x normal speed
        };

        public int CurrentEnergy { get; set; }

        public static float TicksAtSpeed(int speed)
        {
            return ActionCost/Gains[NormalSpeed + speed];
        }

        public bool CanTakeTurn()
        {
            return CurrentEnergy >= ActionCost;
        }

        /// Advances one game turn and gains an appropriate amount of energy. Returns
        /// `true` if there is enough energy to take a turn.
        public bool Gain(int speed)
        {
            CurrentEnergy += Gains[speed];
            return CanTakeTurn();
        }

        /// Spends a turn's worth of energy.
        public void Spend()
        {
            CurrentEnergy -= ActionCost;
        }
    }
}