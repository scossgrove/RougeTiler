using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.UIConnector
{
    // TODO: Declare our event delegate.
    public delegate void TargetDialogHandler(object sender, TargetDialogEventArgs ea);

    // A callback invoked when a target has been selected.
    public delegate void TargetOnSelectDelegate(VectorBase target);
}
