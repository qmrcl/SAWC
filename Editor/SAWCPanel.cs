using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAWC.Editor
{
    public class SAWCPanel : EditorWindow
    {
        [MenuItem("Tools/SAWC Panel")]
        public static void ShowWindow()
        {
            SAWCPanel wnd = GetWindow<SAWCPanel>();
            wnd.titleContent = new GUIContent("SAWC Panel");
            wnd.minSize = new Vector2(250, 150);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Label label = new Label("аерю");
            label.style.fontSize = 18;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.alignSelf = Align.Center;
            label.style.marginBottom = 10;
            root.Add(label);

            Button button = new Button();
            button.text = "реяр";
            button.style.height = 40;
            button.style.marginTop = 10;
            root.Add(button);
        }
    }
}