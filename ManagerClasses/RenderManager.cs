﻿using Boss_Timer_Overlay.ManagerClasses;
using System;
using System.IO;
using System.Threading;
using System.Timers;

namespace Boss_Timer_Overlay.RenderCode
{
    public static class RenderManager
    {
        private static OverlayLoop _overlayLoop;

        public static Thread _renderThread;
        public static bool RendererIsRunning = false;
        public static System.Timers.Timer UpdateTimer { get; set; }

        static RenderManager()
        {
            _overlayLoop = new OverlayLoop();
            UpdateTimer = new System.Timers.Timer()
            {
                Interval = GetTimerInterval(),
                AutoReset = false
            };
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private static double GetTimerInterval()
        {
            DateTime now = DateTime.Now;
            return ((60 - now.Second) * 1000 - now.Millisecond);
        }

        private static void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Update();
            UpdateTimer.Interval = GetTimerInterval();
            UpdateTimer.Start();
        }

        public static void Update()
        {
            if (RendererIsRunning == false)
                return;

            var bossData = GeneralManager.GetBossData("https://mmotimer.com/bdo/streamwidget/stream.php?server=eu");

            var bossInfoString = GeneralManager.BossDataToInfoString(bossData);

            var bossImagePath = GeneralManager.DownloadImage(bossData.ImageUrl, $"{bossData.Name}");

            if (File.Exists(bossImagePath))
            {
                SetBitmapImage(bossImagePath);
            }

            SetRenderFont("Georgia", 12);
            UpdateRenderString(bossInfoString);
        }

        public static void StartRenderer()
        {
            if (_overlayLoop is null)
                return;

            if (_renderThread != null && _renderThread.IsAlive)
                return;

            _renderThread = new Thread(new ThreadStart(_overlayLoop.RunOverlay));
            _renderThread.Start();

            UpdateTimer.Start();

            RendererIsRunning = true;
        }

        public static void StopRenderer()
        {
            if (_overlayLoop is null)
                return;

            if (_renderThread is null)
                return;

            if (_renderThread.IsAlive == false)
                return;

            _overlayLoop.StopOverlay();

            _renderThread.Abort();

            UpdateTimer.Stop();

            RendererIsRunning = false;
        }

        public static void UpdateRenderString(string renderString)
        {
            _overlayLoop.UpdateRenderString(renderString);
        }

        public static void SetRenderFont(string fontName, int fontSize)
        {
            _overlayLoop.SetFont(fontName, fontSize);
        }

        public static void SetBitmapImage(string filePath)
        {
            _overlayLoop.SetBitmap(filePath);
        }
    }
}
