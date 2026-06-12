using System.Collections.Generic;
using UnityEngine;

namespace SAWC.Editor.Localization
{
    [CreateAssetMenu(fileName = "SAWCEditorKeys", menuName = "SAWC/Editor/Editor Keys Config")]
    public class SAWCEditorKeys : ScriptableObject
    {
        public List<string> Keys = new() { "Editor_Hello", "Editor_DocMessage", "Editor_ActiveLangLabel", "Editor_InstalledPacksLabel" };
    }
}