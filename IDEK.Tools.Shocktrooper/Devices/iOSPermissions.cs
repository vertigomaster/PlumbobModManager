//Created by: Cayden Chancey
//Edited by: Julian Noel

using System;
using IDEK.Tools.ShocktroopUtils;

namespace IDEK.Tools.Devices
{
    public static class iOSPermissions
    {
#if UNITY_IOS
        public static bool TryGetLocationPermissions() 
        {
            if (!UnityEngine.Input.location.isEnabledByUser)
            {
                ConsoleLog.LogError("IOS Location not enabled");
                return false;
            }

            return true;
        }
#endif
    }
}
