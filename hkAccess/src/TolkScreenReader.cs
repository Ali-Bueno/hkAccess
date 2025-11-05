using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BepInEx.Logging;

namespace HKAccessibility
{
    /// <summary>
    /// Wrapper around the Tolk screen reader integration library
    /// Provides singleton access, debouncing, and text cleanup
    /// </summary>
    public class TolkScreenReader
    {
        private static TolkScreenReader instance;
        private bool isInitialized = false;
        private string lastSpokenText = "";
        private DateTime lastSpokenTime = DateTime.MinValue;

        public static TolkScreenReader Instance
        {
            get
            {
                if (instance == null)
                    instance = new TolkScreenReader();
                return instance;
            }
        }

        public bool IsInitialized => isInitialized;

        public bool Initialize()
        {
            try
            {
                Tolk.TrySAPI(true);  // Allow SAPI as fallback
                Tolk.PreferSAPI(false);  // Prefer real screen readers

                Tolk.Load();
                isInitialized = Tolk.IsLoaded();

                return isInitialized;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to initialize Tolk: {ex}");
                isInitialized = false;
                return false;
            }
        }

        public string DetectScreenReader()
        {
            if (!isInitialized) return null;
            return Tolk.DetectScreenReader();
        }

        public bool HasSpeech()
        {
            if (!isInitialized) return false;
            return Tolk.HasSpeech();
        }

        public bool HasBraille()
        {
            if (!isInitialized) return false;
            return Tolk.HasBraille();
        }

        private string StripHtmlTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Basic HTML tag stripping
            text = Regex.Replace(text, @"<[^>]+>", "");

            return text;
        }

        public bool Speak(string text, bool interrupt = false)
        {
            if (!isInitialized || string.IsNullOrEmpty(text)) return false;

            // Debounce logic - don't speak the same text again so quickly
            // UNLESS interrupt is true (for menu navigation where same values can be re-selected)
            if (!interrupt && text == lastSpokenText && (DateTime.Now - lastSpokenTime).TotalMilliseconds < 500)
            {
                return false;
            }

            text = StripHtmlTags(text);
            bool success = Tolk.Output(text, interrupt);

            if (success)
            {
                lastSpokenText = text;
                lastSpokenTime = DateTime.Now;
            }

            return success;
        }

        public bool IsSpeaking()
        {
            if (!isInitialized) return false;
            return Tolk.IsSpeaking();
        }

        public bool Silence()
        {
            if (!isInitialized) return false;
            return Tolk.Silence();
        }

        public void Shutdown()
        {
            if (isInitialized)
            {
                Tolk.Unload();
                isInitialized = false;
            }
        }

        public void Cleanup()
        {
            Shutdown();
        }
    }

    /// <summary>
    /// Official Tolk .NET wrapper class - P/Invoke bindings
    /// </summary>
    public sealed class Tolk
    {
        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        private static extern void Tolk_Load();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_IsLoaded();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        private static extern void Tolk_Unload();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        private static extern void Tolk_TrySAPI(
            [MarshalAs(UnmanagedType.I1)]bool trySAPI);

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        private static extern void Tolk_PreferSAPI(
            [MarshalAs(UnmanagedType.I1)]bool preferSAPI);

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr Tolk_DetectScreenReader();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_HasSpeech();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_HasBraille();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Output(
            [MarshalAs(UnmanagedType.LPWStr)]String str,
            [MarshalAs(UnmanagedType.I1)]bool interrupt);

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Speak(
            [MarshalAs(UnmanagedType.LPWStr)]String str,
            [MarshalAs(UnmanagedType.I1)]bool interrupt);

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Braille(
            [MarshalAs(UnmanagedType.LPWStr)]String str);

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_IsSpeaking();

        [DllImport("Tolk.dll", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Tolk_Silence();

        // Prevent construction
        private Tolk() {}

        public static void Load() { Tolk_Load(); }
        public static bool IsLoaded() { return Tolk_IsLoaded(); }
        public static void Unload() { Tolk_Unload(); }
        public static void TrySAPI(bool trySAPI) { Tolk_TrySAPI(trySAPI); }
        public static void PreferSAPI(bool preferSAPI) { Tolk_PreferSAPI(preferSAPI); }

        // Prevent the marshaller from freeing the unmanaged string
        public static String DetectScreenReader()
        {
            IntPtr ptr = Tolk_DetectScreenReader();
            return ptr != IntPtr.Zero ? Marshal.PtrToStringUni(ptr) : string.Empty;
        }

        public static bool HasSpeech() { return Tolk_HasSpeech(); }
        public static bool HasBraille() { return Tolk_HasBraille(); }
        public static bool Output(String str, bool interrupt = false) { return Tolk_Output(str, interrupt); }
        public static bool Speak(String str, bool interrupt = false) { return Tolk_Speak(str, interrupt); }
        public static bool Braille(String str) { return Tolk_Braille(str); }
        public static bool IsSpeaking() { return Tolk_IsSpeaking(); }
        public static bool Silence() { return Tolk_Silence(); }
    }
}
