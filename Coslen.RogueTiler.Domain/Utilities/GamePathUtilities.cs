using System;

namespace Coslen.RogueTiler.Domain.Utilities
{
    public static class GamePathUtilities
    {
        public static string GetProjectRoot()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetStreamingAssetsPath()
        {
            // TODO: Replace when using Unity
            //return Application.streamingAssetsPath;
            return GetProjectRoot() + @"\App_Data\StreamingAssets";
        }

        public static string GetDebugFilePath()
        {
            return GetProjectRoot() + @"\App_Data\RogueTiler.log";
        }
    }
}