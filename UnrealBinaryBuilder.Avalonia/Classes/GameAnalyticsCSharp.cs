using GameAnalyticsSDK.Net;
using System;

namespace UnrealBinaryBuilder.Avalonia.Classes
{
	public static class GameAnalyticsCSharp
	{
		// Please DO NOT change this. If you don't want Analytics, set both GAME_KEY and SECRET_KEY to null ///////////////////
		private static readonly string? GAME_KEY = null;
		private static readonly string? SECRET_KEY = null;
		///////////////////////////////////////////////////////////////////////////////////////////////

		private static Action<string>? logCallback = null;
        private static bool isInitialized = false;

		public static void InitializeGameAnalytics(string InProductVersion, Action<string> InLogCallback)
        {
            if (GAME_KEY == null || SECRET_KEY == null) return;

            GameAnalytics.ConfigureBuild($"Unreal Binary Builder {InProductVersion}");

            // https://gameanalytics.com/docs/item/c-sharp-sdk#initializing
            GameAnalytics.Initialize(GAME_KEY, SECRET_KEY);
            logCallback = InLogCallback;
            isInitialized = true;
            logCallback?.Invoke("Unreal Binary Builder Analytics Initialized.");
#if DEBUG
			GameAnalytics.AddDesignEvent("Program:Start:Debug");
#else
            GameAnalytics.AddDesignEvent("Program:Start:Release");
#endif
        }

		public static void EndSession()
        {
            if (!isInitialized) return;
			
			GameAnalytics.EndSession();
		}

		public static void AddDesignEvent(string InMessage)
        {
            if (!isInitialized) return;
			
			GameAnalytics.AddDesignEvent(InMessage);
#if DEBUG
			logCallback?.Invoke($"Unreal Binary Builder Analytics (Design): {InMessage}");
#endif
		}

		public static void AddProgressStart(string InProgression01)
        {
            if (!isInitialized) return;
			
			GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, InProgression01);
#if DEBUG
			logCallback?.Invoke($"Unreal Binary Builder Analytics (Progress Start): {InProgression01}");
#endif
		}

		public static void AddProgressStart(string InProgression01, string InProgression02)
		{
			if (!isInitialized) return;

			GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, InProgression01, InProgression02);
#if DEBUG
			logCallback?.Invoke($"Unreal Binary Builder Analytics (Progress Start): {InProgression01}::{InProgression02}");
#endif
		}

		public static void AddProgressEnd(string InProgression01, bool bIsFail = false)
        {
            if (!isInitialized) return;

			GameAnalytics.AddProgressionEvent(bIsFail ? EGAProgressionStatus.Fail : EGAProgressionStatus.Complete, InProgression01);
#if DEBUG
			logCallback?.Invoke($"Unreal Binary Builder Analytics (Progress End): {InProgression01}");
#endif
		}

		public static void AddProgressEnd(string InProgression01, string InProgression02, bool bIsFail = false)
        {
            if (!isInitialized) return;
			
			GameAnalytics.AddProgressionEvent(bIsFail ? EGAProgressionStatus.Fail : EGAProgressionStatus.Complete, InProgression01, InProgression02);
#if DEBUG
			logCallback?.Invoke($"Unreal Binary Builder Analytics (Progress End): {InProgression01}::{InProgression02}");
#endif
		}

		public static void LogEvent(string InMessage, EGAErrorSeverity InLogLevel)
        {
            if (!isInitialized) return;
			
			GameAnalytics.AddErrorEvent(InLogLevel, InMessage);
#if DEBUG
			logCallback?.Invoke($"Unreal Binary Builder Analytics (Log): {InMessage}");
#endif
		}
	}
}
