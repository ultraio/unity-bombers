/*
 * This will scale the screen to the appropriate resolution based on the monitor the app is running on.
 * App is Windowed by default and will use an appropriate resolution that doesn't match or is greater than the monitor's resolution.
 * When Fullscreened (Alt + Enter), the app will use one of the matching supported resolutions, or the closest one that isn't greater than the monitor's resolution.
 *
 * Supported resolutions: 7680x4320 (8K), 5120x2880 (5K), 3840x2160 (4K), 2560x1440 (QHD), 1920x1080 (FHD), 1280x720 (HD), 800x600 (Standard, Default)
 */

using System.Collections.Generic;
using UnityEngine;

namespace Gameframework
{
    public class ScreenResolver : BaseBehaviour
    {
        [SerializeField] private bool LaunchAppInFullScreen = false;

#if UNITY_EDITOR || !UNITY_STANDALONE
        private void Awake()
        {
            Destroy(gameObject);
        }
#elif UNITY_STANDALONE // This should only run on Standalone devices
        private readonly struct RenderSizes
        {
            public readonly int Width;
            public readonly int Height;

            public RenderSizes(int width, int height) { Width = width; Height = height; }
        }

        #region Static Helpers
        private const int STANDARD  = 600;
        private const int HD_720P   = 720;
        private const int FHD_1080P = 1080;
        private const int QHD_1440P = 1440;
        private const int UHD_2160P = 2160;
        private const int UHD_2880P = 2880;
        private const int UHD_4320P = 4320;

        private static readonly List<int> ResolutionList = new List<int>
        {
            STANDARD, HD_720P, FHD_1080P, QHD_1440P, UHD_2160P, UHD_2880P, UHD_4320P
        };

        private static readonly Dictionary<int, RenderSizes> ResolutionConfigs = new Dictionary<int, RenderSizes>
        {
            { STANDARD,  new RenderSizes(800,  600) },
            { HD_720P,   new RenderSizes(1280, 720) },  { FHD_1080P, new RenderSizes(1920, 1080) },
            { QHD_1440P, new RenderSizes(2560, 1440) }, { UHD_2160P, new RenderSizes(3840, 2160) },
            { UHD_2880P, new RenderSizes(5120, 2880) }, { UHD_4320P, new RenderSizes(7680, 4320) }
        };
        #endregion

        private bool isFullScreen = false;
        private RenderSizes windowedConfig;
        private RenderSizes fullscreenConfig;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Resolution resolutions = Screen.resolutions[0];
            foreach(Resolution supported in Screen.resolutions)
            {
                resolutions = resolutions.height < supported.height ? supported : resolutions;
            }

            Debug.Log($"Max Resolution: {resolutions.width}x{resolutions.height}");

            int windowed = ResolutionList[0]; int fullscreen = ResolutionList[0];
            int screenHeight = resolutions.height;
            foreach (int resolution in ResolutionList)
            {
                int compare = screenHeight - resolution;
                Debug.Log($"Comparing Screen Height: {screenHeight} with Resolution: {resolution}");

                if (compare >= 0)
                {
                    if (resolution < screenHeight)
                    {
                        windowed = resolution;
                        Debug.Log($"Accepted Windowed Height: {windowed}");
                    }

                    if (resolution <= screenHeight)
                    {
                        fullscreen = resolution;
                        Debug.Log($"Accepted Fullscreen Height: {fullscreen}");
                    }

                    //windowed = resolution < screenHeight ? resolution : windowed;
                    //fullscreen = resolution <= screenHeight ? resolution : fullscreen;
                }
            }

            windowedConfig = ResolutionConfigs[windowed];
            fullscreenConfig = ResolutionConfigs[fullscreen];

            if (LaunchAppInFullScreen)
            {
                isFullScreen = true;

#if !UNITY_STANDALONE_OSX
                Screen.SetResolution(fullscreenConfig.Width, fullscreenConfig.Height, FullScreenMode.FullScreenWindow);
#else
                Screen.SetResolution(fullscreenConfig.Width, fullscreenConfig.Height, FullScreenMode.MaximizedWindow);
#endif
                Debug.Log($"Setting Fullscreen Resolution to: {fullscreenConfig.Width}x{fullscreenConfig.Height}");
            }
            else
            {
                isFullScreen = false;

                Screen.SetResolution(windowedConfig.Width, windowedConfig.Height, FullScreenMode.Windowed);

                Debug.Log($"Setting Windowed Resolution to: {windowedConfig.Width}x{windowedConfig.Height}");
            }
        }

        private void Update()
        {
            if(isFullScreen && !Screen.fullScreen)
            {
                isFullScreen = false;

                Screen.SetResolution(windowedConfig.Width, windowedConfig.Height, FullScreenMode.Windowed);

                Debug.Log($"Setting Windowed Resolution to: {windowedConfig.Width}x{windowedConfig.Height}");
            }
            else if (!isFullScreen && Screen.fullScreen)
            {
                isFullScreen = true;

#if !UNITY_STANDALONE_OSX
                Screen.SetResolution(fullscreenConfig.Width, fullscreenConfig.Height, FullScreenMode.FullScreenWindow);
#else
                Screen.SetResolution(fullscreenConfig.Width, fullscreenConfig.Height, FullScreenMode.MaximizedWindow);
#endif
                Debug.Log($"Setting Fullscreen Resolution to: {fullscreenConfig.Width}x{fullscreenConfig.Height}");
            }
        }
#endif
    }
}
