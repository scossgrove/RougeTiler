using System;

namespace Coslen.RogueTiler.Domain.UIConnector
{
    public static class EventHub
    {
        private static TargetDialogHandler _TargetDialogRequested;

        public static event TargetDialogHandler TargetDialogRequested
        {
            add
            {
                if (_TargetDialogRequested == null)    // First listener...
                {
                    // TODO: If needed, add code to respond to the first event hook-up.
                }
                _TargetDialogRequested = (TargetDialogHandler)Delegate.Combine(_TargetDialogRequested, value);
            }
            remove
            {
                _TargetDialogRequested = (TargetDialogHandler)Delegate.Remove(_TargetDialogRequested, value);
                if (_TargetDialogRequested == null)  // No more listeners to this event
                {
                    // TODO: Add code to clean up if necessary.
                }
            }
        }

        public static void OnTargetDialogRequested(object sender, TargetDialogEventArgs ea)
        {
            if (_TargetDialogRequested != null)
                try
                {
                    // Traditional way to trigger an event:
                    //ColorChanged(sender, ea);
                    Delegate[] listeners = _TargetDialogRequested.GetInvocationList();
                    for (int i = 0; i < listeners.Length; i++)
                    {
                        TargetDialogHandler TargetDialogHandler = (TargetDialogHandler)listeners[i];
                        try
                        {
                            TargetDialogHandler(sender, ea);
                        }
                        catch
                        {
                            // Imagine really cool logging code here.
                        }
                    }
                }
                catch
                {
                    // Imagine really cool logging code here.
                }
        }
    }
}
