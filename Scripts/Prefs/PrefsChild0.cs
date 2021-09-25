using System.Collections.Generic;
using RapidGUI;
using UnityEngine;

namespace PrefsGUI.Example
{
    public class PrefsChild0 : MonoBehaviour, IDoGUI
    {
        public NSEqPa NSScript0;
        public NSEqPa NSScript1;
        public NSEqPa NSScript2;
        #region Type Define

        public enum EnumSample
        {
            One,
            Two,
            Three
        }

        [System.Serializable]
        public class CustomClass
        {
            public string name;
            public int intValue;

            public CustomClass()
            {
            }

            public CustomClass(CustomClass other)
            {
                name = other.name;
                intValue = other.intValue;
            }
        }

        #endregion
        // define PrefsParams with key.
        public static PrefsBool enableMouse = new PrefsBool("Enable Mouse", false);
        public static PrefsFloat thresholdSpeed = new PrefsFloat("Threshold speed", 0.1f);
        public static PrefsFloat thresholdSpeedS = new PrefsFloat("Threshold speed S", 3.0f);
        public static PrefsFloat thresholdSpeedC = new PrefsFloat("Threshold speed C", 4.0f);

        public PrefsFloat speed0 = new PrefsFloat("speed 0", 1.0f);
        public PrefsFloat speed1 = new PrefsFloat("speed 1", 1.0f);
        public PrefsFloat speed2 = new PrefsFloat("speed 2", 1.0f);

        //public PrefsInt IniNumH0 = new PrefsInt("IniNumH 0", 20);
        //public PrefsInt IniNumH1 = new PrefsInt("IniNumH 1", 22);
        //public PrefsInt IniNumH2 = new PrefsInt("IniNumH 2", 25);

        //public PrefsInt prefsInt = new PrefsInt("PrefsInt");
        //public PrefsFloat prefsFloat = new PrefsFloat("PrefsFloat");
        //public PrefsString prefsString = new PrefsString("PrefsString");
        //public PrefsParam<EnumSample> prefsEnum = new PrefsParam<EnumSample>("PrefsEnum");
        //public PrefsColor prefsColor = new PrefsColor("PrefsColor");
        //public PrefsVector2 prefsVector2 = new PrefsVector2("PrefsVector2");
        //public PrefsVector3 prefsVector3 = new PrefsVector3("PrefsVector3");
        //public PrefsVector4 prefsVector4 = new PrefsVector4("PrefsVector4");
        //public PrefsAny<CustomClass> prefsClass = new PrefsAny<CustomClass>("PrefsClass");
        //public PrefsList<CustomClass> prefsList = new PrefsList<CustomClass>("PrefsList");

        public void DoGUI()
        {
            enableMouse.DoGUI();
            thresholdSpeed.DoGUI();
            thresholdSpeedS.DoGUI();
            thresholdSpeedC.DoGUI();

            speed0.DoGUI();
            speed1.DoGUI();
            speed2.DoGUI();

            //IniNumH0.DoGUI();
            //IniNumH1.DoGUI();
            //IniNumH2.DoGUI();

        }

        void Update()
        {
            NSScript0.speed = speed0;
            NSScript1.speed = speed1;
            NSScript2.speed = speed2;

            //NSScript0.IniNumH = IniNumH0;
            //NSScript1.IniNumH = IniNumH1;
            //NSScript2.IniNumH = IniNumH2;

        }


    }
}