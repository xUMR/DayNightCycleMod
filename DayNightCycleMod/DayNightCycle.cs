using System;
using System.IO;
using ColossalFramework;
using UnityEngine;

namespace DayNightCycleMod
{
    // based on
    // http://answers.unity3d.com/questions/622459/set-directionallight-rotation-for-daynight-per-tim.html
    public class DayNightCycle : MonoBehaviour
    {
        private GameObject _moon;
        private Light _moonLight;
        private Light _sunLight;
        private SimulationManager _simulation;
        private ModUI _ui;

        private Color _daytimeSkyColor;
        private Color _middaySkyColor;
        private Color _nighttimeSkyColor;

        private readonly Color _sunLightColor = new Color(1f, 1f, 0.7f);
        private readonly Color _moonLightColor = new Color(0.17f, 0.16f, 0.27f);

        private float _dayTimeFactor = 5f;
        private float _duskDawnFactor = 1f;
        private float _nightTimeFactor = 4f;

        private float _timeScale;
        private float TimeScale
        {
            get { return _timeScale; }
            set
            {
                _timeScale = value; // defaults (when timescale is 1)
                _daytimeRlSeconds = _dayTimeFactor * _timeScale; // 5
                _duskRlSeconds = _duskDawnFactor * 0.5f * _timeScale; // 1
                _nighttimeRlSeconds = _nightTimeFactor * _timeScale; // 4
                _dawnRlSeconds = _duskDawnFactor * 0.5f * _timeScale; // 1
                _gameDayRlSeconds = _daytimeRlSeconds + _duskRlSeconds + _nighttimeRlSeconds + _dawnRlSeconds;

                var totalHours = _simulation.m_currentGameTime.Ticks / TimeSpan.TicksPerHour;
                TimeOfDay = (totalHours % 24) / (24 * TimeScale);
            }
        }

        private float TimeOfDay
        {
            get { return _timeRt / _gameDayRlSeconds; }
            set { _timeRt = value * _gameDayRlSeconds; }
        }

        private float _daytimeRlSeconds;
        private float _duskRlSeconds;
        private float _nighttimeRlSeconds;
        private float _dawnRlSeconds;
        private float _gameDayRlSeconds;

        private float _startOfNoon;
        private float _startOfDusk;
        private float _startOfNighttime;
        private float _startOfDawn;

        private float _timeRt;

        private DayNightCycleData _data;

        private DayNightCycle() { }

        private Color CalculateSkyColor()
        {
            var time = TimeOfDay;

            if (time <= _startOfNoon)
                return Color.Lerp(_daytimeSkyColor, _middaySkyColor, time / _startOfNoon);
            if (time <= _startOfDusk)
                return Color.Lerp(_middaySkyColor, _daytimeSkyColor, (time - _startOfNoon) / (_startOfDusk - _startOfNoon));
            if (time <= _startOfNighttime)
                return Color.Lerp(_daytimeSkyColor, _nighttimeSkyColor, (time - _startOfDusk) / (_startOfNighttime - _startOfDusk));

            return Color.Lerp(_nighttimeSkyColor, _daytimeSkyColor, (time - _startOfDawn) / (1.0f - _startOfDawn));
        }

        public void Init(Light sunLight, Light moonLight)
        {
            _simulation = Singleton<SimulationManager>.instance;

            _sunLight = sunLight;
            _moonLight = moonLight;

            _sunLight.color = _sunLightColor;
            _moonLight.color = _moonLightColor;
            _moon = _moonLight.gameObject;

            _moonLight.type = sunLight.type;
            _moonLight.range = sunLight.range;
            _moonLight.shadows = sunLight.shadows;
            _moonLight.spotAngle = sunLight.spotAngle;
            _moon.transform.position = new Vector3(2000, 20000, 0);
            _moon.transform.LookAt(Vector3.zero);
            _moonLight.intensity = sunLight.intensity *  0.75f;

            _ui = gameObject.AddComponent<ModUI>();

            try
            {
                _data = DayNightCycleData.Deserialize();
            }
            catch (FileNotFoundException)
            {
                _data = new DayNightCycleData();
            }

            var modData = _data.UIData;
            _ui.Init(modData.Paused, modData.Shrunk, modData.X, modData.Y, modData.Time);

            _dayTimeFactor = _data.DayTimeFactor;
            _duskDawnFactor = _data.DuskDawnFactor;
            _nightTimeFactor = _data.NightTimeFactor;

            _daytimeSkyColor = _data.DaytimeSkyColor;
            _middaySkyColor = _data.MiddaySkyColor;
            _nighttimeSkyColor = _data.NighttimeSkyColor;

            TimeScale = Mathf.Clamp(TimeScale, 1, 8640);

            _startOfDusk = _daytimeRlSeconds / _gameDayRlSeconds;
            _startOfNighttime = _startOfDusk + _duskRlSeconds / _gameDayRlSeconds;
            _startOfDawn = _startOfNighttime + _nighttimeRlSeconds / _gameDayRlSeconds;

            _startOfNoon = _startOfDusk / 2;
        }

        public DayNightCycleData GetOptions()
        {
            if (_data == null) return new DayNightCycleData();

            _data.UIData.Paused = _ui.Paused;
            _data.UIData.Shrunk = _ui.Shrunk;
            _data.UIData.X = _ui.X;
            _data.UIData.Y = _ui.Y;
            _data.UIData.Time = _ui.SliderValue;
            
            return _data;
        }

        void Update()
        {
            TimeOfDay = _ui.SliderValue;

            Camera.main.backgroundColor = CalculateSkyColor();

            var sunangle = TimeOfDay * 360;
            gameObject.transform.position = Vector3.zero + Quaternion.Euler(0, 0, sunangle) * (8 * Vector3.right);
            gameObject.transform.LookAt(Vector3.zero);

            var date = _simulation.m_currentGameTime;
            if (date.Day == 12 && date.DayOfWeek == DayOfWeek.Thursday)
            {
                _moonLight.color = Color.Lerp(_moonLightColor, Color.red, date.Hour / 24f);
                _sunLight.color = Color.Lerp(_sunLightColor, Color.black, date.Hour / 24f);
            }
            else if (date.Day == 14 && date.DayOfWeek == DayOfWeek.Saturday)
            {
                _moonLight.color = Color.Lerp(Color.red, _moonLightColor, date.Hour / 24f);
                _sunLight.color = Color.Lerp(Color.black, _sunLightColor, date.Hour / 24f);
            }

            if (_ui.Paused) return;

            _timeRt = (_timeRt + _simulation.m_simulationTimeDelta) % _gameDayRlSeconds;
            _ui.SliderValue = TimeOfDay;
        }
    }
}
