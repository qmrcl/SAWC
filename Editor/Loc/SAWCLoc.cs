using System;

namespace SAWC.Editor.Localization
{
    public static class SAWCLoc
    {
        public static readonly LocalizationSystem System = new();

        public static event Action OnLanguageChanged
        {
            add => System.OnLanguageChanged += value;
            remove => System.OnLanguageChanged -= value;
        }

        public static LanguageAsset Current => System.CurrentAsset;
    }
}