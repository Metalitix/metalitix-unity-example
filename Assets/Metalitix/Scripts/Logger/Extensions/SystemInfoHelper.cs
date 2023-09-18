using System.Runtime.InteropServices;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Enums;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Extensions
{
    public static class SystemInfoHelper
    {
        public static bool IsTablet { get; }
        public static bool IsMProcessor { get; }

        static SystemInfoHelper()
        {
            var isArm = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
            var isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            IsMProcessor = isArm && isOSX;
        }
        
        public static MetadataSystemInfo CollectSystemInfo()
        {
            var metadataSystemInfo = new MetadataSystemInfo(
                SystemInfo.deviceModel,
                GetDeviceType().ToString(),
                SystemInfo.operatingSystemFamily.ToString(),
                SystemInfo.operatingSystem
            );

            return metadataSystemInfo;
        }
        
        private static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        private static MetalitixDeviceType GetDeviceType()
        {
#if UNITY_IOS
            isTablet = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");

            if (isTablet)
            {
                return MetalitixDeviceType.tablet;
            }

            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");

            if (deviceIsIphone)
            {
                return MetalitixDeviceType.mobile;
            }
            
#elif UNITY_ANDROID
            float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
            bool isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);
 
            if (isTablet)
            {
                return MetalitixDeviceType.tablet;
            }
            else
            {
                return MetalitixDeviceType.mobile;
            }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
            return MetalitixDeviceType.desktop;
#endif
        }
    }
}