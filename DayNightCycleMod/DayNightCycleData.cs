using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace DayNightCycleMod
{
    public class DayNightCycleData
    {
        private const string Path = "day-night.settings.xml";

        public float DayTimeFactor;
        public float DuskDawnFactor;
        public float NightTimeFactor;

        public float TimeScale;

        public float MoonLightIntensityMultiplier;

        public Color DaytimeSkyColor;
        public Color MiddaySkyColor;
        public Color NighttimeSkyColor;

        public Color SunColor;
        public Color MoonColor;

        public ModUIData UIData;

        public DayNightCycleData(Color moonColor, Color sunColor, Color nighttimeSkyColor, Color middaySkyColor, Color daytimeSkyColor, ModUIData data = null,
                float moonLightIntensityMultiplier = 1, float timeScale = 10, float nightTimeFactor = 4, float duskDawnFactor = 1, float dayTimeFactor = 5)
        {
            MoonColor = moonColor;
            SunColor = sunColor;
            NighttimeSkyColor = nighttimeSkyColor;
            MiddaySkyColor = middaySkyColor;
            DaytimeSkyColor = daytimeSkyColor;

            UIData = data ?? new ModUIData();

            MoonLightIntensityMultiplier = moonLightIntensityMultiplier;
            
            NightTimeFactor = nightTimeFactor;
            DuskDawnFactor = duskDawnFactor;
            DayTimeFactor = dayTimeFactor;

            ScaleDayPhases();
            TimeScale = timeScale;
        }

        public DayNightCycleData() 
            : this(new Color(0.56f, 0.54f, 0.62f), new Color(1f, 1f, 0.7f), new Color(0.04f, 0.19f, 0.27f), new Color(0.58f, 0.88f, 1f), new Color(0.31f, 0.88f, 1f)) { }

        private void ScaleDayPhases()
        {
            // scale to make their sum 10
            var total = DayTimeFactor + DuskDawnFactor + NightTimeFactor;
            if (!Mathf.Approximately(total, 10))
            {
                var scalingFactor = 10 / total;
                DayTimeFactor *= scalingFactor;
                DuskDawnFactor *= scalingFactor;
                NightTimeFactor *= scalingFactor;
            }
        }

        public void Serialize()
        {
            using (var stream = File.CreateText(Path))
            {
                var serializer = new XmlSerializer(typeof(DayNightCycleData));
                serializer.Serialize(stream, this);
            }
        }

        public static DayNightCycleData Deserialize()
        {
            DayNightCycleData data;

            using (var stream = File.OpenRead(Path))
            {
                var serializer = new XmlSerializer(typeof(DayNightCycleData));
                data = (DayNightCycleData)serializer.Deserialize(stream);
            }

            data.ScaleDayPhases();

            return data;
        }
    }
}
