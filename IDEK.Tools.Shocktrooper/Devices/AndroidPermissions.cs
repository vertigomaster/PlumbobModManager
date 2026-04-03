//Created by: Cayden Chancey
//Edited by: Julian Noel
using IDEK.Tools.Logging;

namespace IDEK.Tools.Devices
{
    public static class AndroidPermissions
    {
#if UNITY_ANDROID
        public static void TryAskForLocationPermissions() 
        {
            if (!UnityEngine.Input.location.isEnabledByUser)
            {
                ConsoleLog.LogError("Android Location not enabled");
                return;
            }

            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            }
        }

        public static void TryAskForCameraPermissions() 
        {
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
        }
#endif
    }
}
