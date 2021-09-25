using RapidGUI;
using UnityEngine;

namespace PrefsGUI.Example
{
    public class PrefsParent : PrefsGUIExampleBase
    {
        private WindowLaunchers windows;
        private Rect rect;

        private void Start()
        {
            windows = new WindowLaunchers();
            windows.isWindow = false;
            windows.Add("Part1", typeof(PrefsChild0));
        }

        protected override void DoGUI()
        {
            windows.DoGUI();
            base.DoGUI();
        }
    }
}