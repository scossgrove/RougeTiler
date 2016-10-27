using System;
using System.Runtime.InteropServices;

namespace Coslen.RogueTiler.Win.UI.InGame.Helpers
{
    public delegate bool IsInput(ConsoleKeyInfo key);
    public class Input
    {
        public Input(string name, IsInput inputCheck)
        {
            Name = name;
            this.InputCheck = inputCheck;
        }

        public IsInput InputCheck { get; set; }
        public string Name { get; set; }

        // Equality operator. Returns dbNull if either operand is dbNull, 
        // otherwise returns dbTrue or dbFalse:
        public static bool operator ==(Input x, ConsoleKeyInfo y)
        {
            return x.InputCheck(y);
        }

        // Inequality operator. Returns dbNull if either operand is
        // dbNull, otherwise returns dbTrue or dbFalse:
        public static bool operator !=(Input x, ConsoleKeyInfo y)
        {
            return !x.InputCheck(y);
        }

        // Equality operator. Returns dbNull if either operand is dbNull, 
        // otherwise returns dbTrue or dbFalse:
        public static bool operator ==(ConsoleKeyInfo x, Input y)
        {
            return y.InputCheck(x);
        }

        // Inequality operator. Returns dbNull if either operand is
        // dbNull, otherwise returns dbTrue or dbFalse:
        public static bool operator !=(ConsoleKeyInfo x, Input y)
        {
            return !y.InputCheck(x);
        }
    }

    /// Enum class defining the high-level inputs from the user.
    /// 
    /// Physical keys on the keyboard are mapped to these, which the user interface
    /// then interprets.
    public class Inputs
    {
        private static Inputs instance;

        private Inputs()
        {
            Initialise();
        }

        public static Inputs Instance
        {
            get
            {
                if (instance == null)
                    instance = new Inputs();
                return instance;
            }
        }

        /// Rests in the level, selects a menu item.
        public Input ok { get; private set; } //"ok"
        public Input tab { get; private set; } //"tab"
        public Input no{ get; private set; } //"no"
        public Input yes { get; private set; } //"yes"

        // TODO: Unify cancel, forfeit, and quit?

        public Input cancel { get; private set; } //"cancel"
        public Input forfeit { get; private set; } //"forfeit"

        /// Exit the successfully completed level.
        public Input quit { get; private set; } //"quit"

        /// Close nearby doors.
        public Input closeDoor { get; private set; } //"closeDoor"

        public Input drop { get; private set; } //"drop"
        public Input use { get; private set; } //"use"
        public Input pickUp { get; private set; } //"pickUp"
        public Input toss { get; private set; } //"toss"
        public Input swap { get; private set; } //"swap"

        public Input stats { get; private set; } //"stats"

        public Input selectCommand { get; private set; } //"selectCommand"

        /// Directional inputs.
        /// 
        /// These are used both for navigating in the level and menu screens.
        public Input n { get; private set; } //"n"
        public Input ne { get; private set; } //"ne"
        public Input e { get; private set; } //"e"
        public Input se { get; private set; } //"se"
        public Input s { get; private set; } //"s"
        public Input sw { get; private set; } //"sw"
        public Input w { get; private set; } //"w"
        public Input nw { get; private set; } //"nw"

        /// Rest repeatedly.
        public Input rest { get; private set; } //"rest"

        public Input runN { get; private set; } //"runN"
        public Input runNE { get; private set; } //"runNE"
        public Input runE { get; private set; } //"runE"
        public Input runSE { get; private set; } //"runSE"
        public Input runS { get; private set; } //"runS"
        public Input runSW { get; private set; } //"runSW"
        public Input runW { get; private set; } //"runW"
        public Input runNW { get; private set; } //"runNW"

        /// Fire the last selected skill.
        public Input fire { get; private set; } //"fire"

        public Input fireN { get; private set; } //"fireN"
        public Input fireNE { get; private set; } //"fireNE"
        public Input fireE { get; private set; } //"fireE"
        public Input fireSE { get; private set; } //"fireSE"
        public Input fireS { get; private set; } //"fireS"
        public Input fireSW { get; private set; } //"fireSW"
        public Input fireW { get; private set; } //"fireW"
        public Input fireNW { get; private set; } //"fireNW"

        private void Initialise()
        {
            // Rests in the level, selects a menu item.
            ok = new Input("ok", key => { return key.Key == ConsoleKey.Enter; });
            tab = new Input("tab", key => { return key.Key == ConsoleKey.Tab; });

            yes = new Input("yes", key => { return key.Key == ConsoleKey.Y; });
            no = new Input("no", key => { return key.Key == ConsoleKey.N; });
            // TODO: Unify cancel, forfeit, and quit?

            cancel = new Input("cancel", key => { return key.Key == ConsoleKey.Escape; });
            forfeit = new Input("forfeit", key => { return key.Key == ConsoleKey.F && (key.Modifiers & ConsoleModifiers.Shift) != 0; });

            // Exit the successfully completed level.
            quit = new Input("quit", key => { return key.Key == ConsoleKey.Q; });

            // Close nearby doors.
            closeDoor = new Input("closeDoor", key => { return key.Key == ConsoleKey.C; });

            drop = new Input("drop", key => { return key.Key == ConsoleKey.D; });
            use = new Input("use", key => { return key.Key == ConsoleKey.U; });
            pickUp = new Input("pickUp", key => { return key.Key == ConsoleKey.G; });
            toss = new Input("toss", key => { return key.Key == ConsoleKey.T; });
            swap = new Input("swap", key => { return key.Key == ConsoleKey.X; });

            stats = new Input("swap", key => { return key.Key == ConsoleKey.E; });

            selectCommand = new Input("selectCommand", key => { return key.Key == ConsoleKey.S; });

            // Directional inputs.
            //
            // These are used both for navigating in the level and menu screens.
            nw = new Input("nw", key => { return (key.Key == ConsoleKey.I || key.Key == ConsoleKey.NumPad7); });
            n = new Input("n", key => { return (key.Key == ConsoleKey.O || key.Key == ConsoleKey.NumPad8); });
            ne = new Input("ne", key => { return (key.Key == ConsoleKey.P || key.Key == ConsoleKey.NumPad9); });
            w = new Input("w", key => { return (key.Key == ConsoleKey.K || key.Key == ConsoleKey.NumPad4 ); });
            e = new Input("e", key => { return (key.Key == ConsoleKey.Oem1 || key.Key == ConsoleKey.NumPad6 ); });
            sw = new Input("se", key => { return (key.Key == ConsoleKey.OemComma || key.Key == ConsoleKey.NumPad1); });
            s = new Input("s", key => { return (key.Key == ConsoleKey.OemPeriod || key.Key == ConsoleKey.NumPad2); });
            se = new Input("sw", key => { return (key.Key == ConsoleKey.Oem2 || key.Key == ConsoleKey.NumPad3); });

            // Rest repeatedly.
            rest = new Input("rest", key => { return key.Key == ConsoleKey.L || key.Key == ConsoleKey.NumPad5; });

            //runNW = new Input("runNW", key => { return (key.Key == ConsoleKey.I || key.Key == ConsoleKey.NumPad7 || key.Key == ConsoleKey.Home) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runN = new Input("runN", key => { return (key.Key == ConsoleKey.O || key.Key == ConsoleKey.NumPad8) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runNE = new Input("runNE", key => { return (key.Key == ConsoleKey.P || key.Key == ConsoleKey.NumPad9 || key.Key == ConsoleKey.PageUp) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runW = new Input("runW", key => { return (key.Key == ConsoleKey.K || key.Key == ConsoleKey.NumPad4) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runE = new Input("runE", key => { return (key.Key == ConsoleKey.Oem1 || key.Key == ConsoleKey.NumPad6) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runSW = new Input("runSE", key => { return (key.Key == ConsoleKey.OemComma || key.Key == ConsoleKey.NumPad1) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runS = new Input("runS", key => { return (key.Key == ConsoleKey.OemPeriod || key.Key == ConsoleKey.NumPad2) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });
            //runSE = new Input("runSW", key => { return (key.Key == ConsoleKey.Oem2 || key.Key == ConsoleKey.NumPad3) && (key.Modifiers & ConsoleModifiers.Shift) != 0; });

            runNW = new Input("runNW", key => { return key.Key == ConsoleKey.Home || (key.Key == ConsoleKey.I && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runN = new Input("runN", key => { return key.Key == ConsoleKey.UpArrow || (key.Key == ConsoleKey.O && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runNE = new Input("runNE", key => { return key.Key == ConsoleKey.PageUp || (key.Key == ConsoleKey.P && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runW = new Input("runW", key => { return key.Key == ConsoleKey.LeftArrow || (key.Key == ConsoleKey.K && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runE = new Input("runE", key => { return key.Key == ConsoleKey.RightArrow || (key.Key == ConsoleKey.Oem1 && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runSW = new Input("runSE", key => { return key.Key == ConsoleKey.End || (key.Key == ConsoleKey.OemComma && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runS = new Input("runS", key => { return key.Key == ConsoleKey.DownArrow || (key.Key == ConsoleKey.OemPeriod && (key.Modifiers & ConsoleModifiers.Shift) != 0); });
            runSE = new Input("runSW", key => { return key.Key == ConsoleKey.PageDown || (key.Key == ConsoleKey.Oem2 && (key.Modifiers & ConsoleModifiers.Shift) != 0); });

            // Fire the last selected skill.
            //fire = new Input("fire", key => { return key.Key == ConsoleKey.; });

            fireNW = new Input("fireNW", key => { return key.Key == ConsoleKey.I && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireN = new Input("fireN", key => { return key.Key == ConsoleKey.O && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireNE = new Input("fireNE", key => { return key.Key == ConsoleKey.P && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireW = new Input("fireW", key => { return key.Key == ConsoleKey.K && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireE = new Input("fireE", key => { return key.Key == ConsoleKey.Oem1 && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireSE = new Input("fireSE", key => { return key.Key == ConsoleKey.OemComma && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireS = new Input("fireS", key => { return key.Key == ConsoleKey.OemPeriod && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
            fireSW = new Input("fireSW", key => { return key.Key == ConsoleKey.Oem2 && (key.Modifiers & ConsoleModifiers.Alt) != 0; });
        }
    }
}