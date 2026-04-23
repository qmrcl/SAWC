using UnityEngine;

namespace SAWC.Core
{
    public class InputReader : MonoBehaviour
    {
        private static InputController _actions;
        public static InputController Actions
        {
            get
            {
                if (_actions == null) _actions = new InputController();
                return _actions;
            }
        }

        private void Awake()
        {
            var objs = FindObjectsByType<InputReader>(FindObjectsSortMode.None);
            if (objs.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Actions.Enable();
        }

        public static void SetMap(bool gameplay)
        {
            if (gameplay)
            {
                Actions.UI.Disable();
                Actions.Player.Enable();
            }
            else
            {
                Actions.Player.Disable();
                Actions.UI.Enable();
            }
        }
    }
}