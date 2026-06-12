using System;
using UnityEngine;

namespace SAWC.Editor.Localization
{
    [Serializable]
    public class TranslationEntry
    {
        public string DisplayName;
        [TextArea(1, 3)] public string Tooltip;
    }
}