using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAWC.Editor
{
    public class SAWCPanel : EditorWindow
    {
        private const string UiAssetRoot = "Packages/com.qmrcl.sawc/Editor/UI/";

        private Label _greetingLabel;
        private Label _docLabel;
        private Button _docButton;

        [MenuItem("Window/SAWC Central Panel")]
        public static void Open()
        {
            var window = GetWindow<SAWCPanel>("SAWC Hub");
            var fixedSize = new Vector2(440, 250);
            window.minSize = fixedSize;
            window.maxSize = fixedSize;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UiAssetRoot + nameof(SAWCPanel) + ".uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UiAssetRoot + nameof(SAWCPanel) + ".uss");

            if (uxml != null && uss != null)
            {
                uxml.CloneTree(root);
                root.styleSheets.Add(uss);
            }
            else
            {
                root.Add(new HelpBox($"UI Assets (UXML/USS) for {nameof(SAWCPanel)} not found.", HelpBoxMessageType.Error));
                return;
            }

            _greetingLabel = root.Q<Label>("greeting-label");
            _docLabel = root.Q<Label>("doc-label");
            _docButton = root.Q<Button>("doc-button");

            if (_greetingLabel != null) _greetingLabel.text = "SAWC Central Hub";
            if (_docLabel != null) _docLabel.text = "Asset configuration panel. Use the button below to view full documentation.";

            if (_docButton != null)
            {
                _docButton.text = "📖 OPEN DOCUMENTATION";
                _docButton.clicked += () => Application.OpenURL("https://qmrcl.github.io/SAWC-docs/");
            }
        }
    }
}