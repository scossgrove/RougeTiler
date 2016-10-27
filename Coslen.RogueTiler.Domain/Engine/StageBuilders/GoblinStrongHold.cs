using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public class GoblinStronghold : RoomDecorator
    {
        public GoblinStronghold()
        {
            // From Source
            //numRoomTries = 140;
            numRoomTries = 20;
            windingPercent = 70;
            roomExtraSize = 1;
        }

        public override void OnDecorateRoom(Rect room)
        {
            if (Rng.Instance.OneIn(2) && decorateRoundedCorners(room)) return;
            if (Rng.Instance.OneIn(5) && decorateTable(room)) return;
            if (Rng.Instance.OneIn(10) && decorateInnerRoom(room)) return;
        }
    }
}
