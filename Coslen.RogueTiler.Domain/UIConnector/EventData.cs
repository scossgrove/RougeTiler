using System;

namespace Coslen.RogueTiler.Domain.UIConnector
{
    // TODO: Declare the data we want to send to our event.
    public class TargetDialogEventArgs : EventArgs
    {
        public TargetOnSelectDelegate OnSelect { get; private set; }
        public int Range { get; set; }

        public TargetDialogEventArgs(int range, TargetOnSelectDelegate onSelect)
        {
            Range = range;
            OnSelect = onSelect;
        }
        
    }
}
