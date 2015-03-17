using System;
using System.IO;
using System.Xml.Serialization;

namespace DayNightCycleMod
{
    public class DayNightCycleData
    {
        private const string Path = "day-night.settings.xml";

        public float DayTimeFactor;
        public float DuskDawnFactor;
        public float NightTimeFactor;

        public float TimeScale;

        public DayNightCycleData(float dayTimeFactor, float duskDawnFactor, float nightTimeFactor, float timeScale)
        {
            DayTimeFactor = dayTimeFactor;
            DuskDawnFactor = duskDawnFactor;
            NightTimeFactor = nightTimeFactor;

            ScaleDayPhases();
            TimeScale = timeScale;
        }

        public DayNightCycleData()
        {
            DayTimeFactor = 5;
            DuskDawnFactor = 1;
            NightTimeFactor = 4;
            TimeScale = 14;
        }

        private void ScaleDayPhases()
        {
            // scale to make their sum 10
            var total = DayTimeFactor + DuskDawnFactor + NightTimeFactor;
            if (Math.Abs(total - 10f) > float.Epsilon)
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
