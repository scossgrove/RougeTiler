using System;

namespace Coslen.RogueTiler.Domain.Engine.Environment
{
    [Serializable]
    public class Shadow
    {
        private bool inShadow;

        public bool IsInShadow
        {
            get { return !inShadow; }
        }

        public bool IsActive { get; private set; }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }

        public void SetShadow(bool inShadow)
        {
            this.inShadow = inShadow;
        }
    }
}