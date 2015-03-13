using UnityEngine;

namespace DayNightCycleMod
{
    public class DayNightCycle : MonoBehaviour
    {
        private readonly GameObject _sun;
        private readonly Light _sunLight;

        private static readonly Color DaytimeSkyColor = new Color(0.31f, 0.88f, 1f);
        private static readonly Color MiddaySkyColor = new Color(0.58f, 0.88f, 1f);
        private static readonly Color NighttimeSkyColor = new Color(0.04f, 0.19f, 0.27f);

        private readonly float _timeScale;

        private readonly float _daytimeRlSeconds = 5.0f;
        private readonly float _duskRlSeconds = 0.5f;
        private readonly float _nighttimeRlSeconds = 4.0f;
        private readonly float _sunsetRlSeconds = 0.5f;
        private readonly float _gameDayRlSeconds;

        private readonly float _startOfDusk;
        private readonly float _startOfNighttime;
        private readonly float _startOfSunset;

        private float _timeRt;
        private const float Radius = 8;

        private readonly Vector3 _midpoint = new Vector3(0, 0, 0);

        public float TimeOfDay
        {
            get { return _timeRt / _gameDayRlSeconds; }
            set { _timeRt = value * _gameDayRlSeconds; }
        }

        private static DayNightCycle _instance;
        public static DayNightCycle Instance
        {
            get { return _instance ?? (_instance = new DayNightCycle()); }
        }

        public DayNightCycle()
        {
            _sunLight = FindObjectOfType<Light>();
            _sun = _sunLight.gameObject;

            _sunLight.color = new Color(1f, 1f, 0.7f);

            // 86400
            _timeScale = 1;
            _daytimeRlSeconds = 5.0f * _timeScale;
            _duskRlSeconds = 0.5f * _timeScale;
            _nighttimeRlSeconds = 4.0f * _timeScale;
            _sunsetRlSeconds = 0.5f * _timeScale;
            _gameDayRlSeconds = _daytimeRlSeconds + _duskRlSeconds + _nighttimeRlSeconds + _sunsetRlSeconds;

            _startOfDusk = _daytimeRlSeconds / _gameDayRlSeconds;
            _startOfNighttime = _startOfDusk + _duskRlSeconds / _gameDayRlSeconds;
            _startOfSunset = _startOfNighttime + _nighttimeRlSeconds / _gameDayRlSeconds;
        }

        Color CalculateSkyColor()
        {
            var time = TimeOfDay;

            if (time <= 0.25f)
                return Color.Lerp(DaytimeSkyColor, MiddaySkyColor, time / 0.25f);
            if (time <= 0.5f)
                return Color.Lerp(MiddaySkyColor, DaytimeSkyColor, (time - 0.25f) / 0.25f);
            if (time <= _startOfNighttime)
                return Color.Lerp(DaytimeSkyColor, NighttimeSkyColor, (time - _startOfDusk) / (_startOfNighttime - _startOfDusk));
            if (time <= _startOfSunset)
                return NighttimeSkyColor;

            return Color.Lerp(NighttimeSkyColor, DaytimeSkyColor, (time - _startOfSunset) / (1.0f - _startOfSunset));
        }

        public void Update(float deltaTime)
        {
            _timeRt = (_timeRt + deltaTime) % _gameDayRlSeconds;
            Camera.main.backgroundColor = CalculateSkyColor();
            var sunangle = TimeOfDay * 360;
            _sun.transform.position = _midpoint + Quaternion.Euler(0, 0, sunangle) * (Radius * Vector3.right);
            _sun.transform.LookAt(_midpoint);
        }
    }
}
