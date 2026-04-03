//Created By: Julian Noel on 06/07/2024

using System.IO;
using IDEK.Tools.Logging;

namespace IDEK.Tools.ShocktroopUtils
{
    public static class FileIOHelper
    {
        ///////////////////// Constants /////////////////////
        #region Constants
        #endregion

        ////////////////////// Fields ///////////////////////
        #region Fields
        #endregion

        //////////////////// Properties /////////////////////
        #region Properties
        #endregion

        ////////////////// Public Methods ///////////////////
        #region Public Methods

        // Method to ensure a directory exists
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Reformats a full file path into a relative path that's suitable for internal asset lookup and returns it
        /// </summary>
        /// <param name="filePath"></param>
        public static string TruncateFullPathToAssetPath(this string filePath)
        {
            // Convert the system file path to a Unity asset path (and fix slashes as necessary)
            string path = filePath.Replace(Application.dataPath, "").Replace("\\", "/");
            ConsoleLog.Log("new path: " + path);
            return path;
        }
        #endif

        #endregion

        ////////////////// Private Methods //////////////////
        #region Private Methods
        #endregion

        /////////////////////// Debug ///////////////////////
        #region Debug
        #endregion
    }
}