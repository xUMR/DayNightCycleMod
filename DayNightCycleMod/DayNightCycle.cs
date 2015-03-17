using System;
using System.IO;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace DayNightCycleMod
{
    // based on
    // http://answers.unity3d.com/questions/622459/set-directionallight-rotation-for-daynight-per-tim.html
    public class DayNightCycle : MonoBehaviour
    {
        private Light _sunLight;
        private SimulationManager _simulation;
        private IThreading _threading;

        private readonly Color _daytimeSkyColor = new Color(0.31f, 0.88f, 1f);
        private readonly Color _middaySkyColor = new Color(0.58f, 0.88f, 1f);
        private readonly Color _nighttimeSkyColor = new Color(0.04f, 0.19f, 0.27f);

        private readonly Vector3 _midpoint = new Vector3(0, 0, 0);

        private float _dayTimeFactor = 5f;
        private float _duskDawnFactor = 1f;
        private float _nightTimeFactor = 4f;

        private float _timeScale;
        private float TimeScale
        {
            get { return _timeScale; }
            set
            {
                _timeScale = value;
                _daytimeRlSeconds = _dayTimeFactor * _timeScale;
                _duskRlSeconds = _duskDawnFactor * 0.5f * _timeScale;
                _nighttimeRlSeconds = _nightTimeFactor * _timeScale;
                _sunsetRlSeconds = _duskDawnFactor * 0.5f * _timeScale;
                _gameDayRlSeconds = _daytimeRlSeconds + _duskRlSeconds + _nighttimeRlSeconds + _sunsetRlSeconds;

                var totalHours = _threading.simulationTime.Ticks / TimeSpan.TicksPerHour;
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
        private float _sunsetRlSeconds;
        private float _gameDayRlSeconds;

        private float _startOfDusk;
        private float _startOfNighttime;
        private float _startOfSunset;

        private float _timeRt;
        private const float Radius = 8;

        private DayNightCycle() { }

        private Color CalculateSkyColor()
        {
            var time = TimeOfDay;

            if (time <= 0.25f)
                return Color.Lerp(_daytimeSkyColor, _middaySkyColor, time / 0.25f);
            if (time <= 0.5f)
                return Color.Lerp(_middaySkyColor, _daytimeSkyColor, (time - 0.25f) / 0.25f);
            if (time <= _startOfNighttime)
                return Color.Lerp(_daytimeSkyColor, _nighttimeSkyColor, (time - _startOfDusk) / (_startOfNighttime - _startOfDusk));
            if (time <= _startOfSunset)
                return _nighttimeSkyColor;

            return Color.Lerp(_nighttimeSkyColor, _daytimeSkyColor, (time - _startOfSunset) / (1.0f - _startOfSunset));
        }

        public void Init(Light sunLight, IThreading threading)
        {
            _threading = threading;
            _sunLight = sunLight;
            _simulation = Singleton<SimulationManager>.instance;

            _sunLight.color = new Color(1f, 1f, 0.7f);

            DayNightCycleData data;

            try
            {
                data = DayNightCycleData.Deserialize();
            }
            catch (FileNotFoundException)
            {
                data = new DayNightCycleData();
            }

            _dayTimeFactor = data.DayTimeFactor;
            _duskDawnFactor = data.DuskDawnFactor;
            _nightTimeFactor = data.NightTimeFactor;

            if (data.TimeScale < 1)
                TimeScale = 1;
            else if (data.TimeScale > 8640)
                TimeScale = 8640;
            else
                TimeScale = data.TimeScale;
            
            _startOfDusk = _daytimeRlSeconds / _gameDayRlSeconds;
            _startOfNighttime = _startOfDusk + _duskRlSeconds / _gameDayRlSeconds;
            _startOfSunset = _startOfNighttime + _nighttimeRlSeconds / _gameDayRlSeconds;
        }

        public DayNightCycleData GetOptions()
        {
            return new DayNightCycleData(_dayTimeFactor, _duskDawnFactor, _nightTimeFactor, TimeScale);
        }

        void Update()
        {
            if (_simulation.SimulationPaused)
                return;

            _timeRt = (_timeRt + _simulation.m_simulationTimeDelta) % _gameDayRlSeconds;
            Camera.main.backgroundColor = CalculateSkyColor();
            var sunangle = TimeOfDay * 360;
            gameObject.transform.position = _midpoint + Quaternion.Euler(0, 0, sunangle) * (Radius * Vector3.right);
            gameObject.transform.LookAt(_midpoint);
        }
    }
}
