using UnityEngine;

namespace DayNightCycleMod
{
    public class ModUI : MonoBehaviour
    {
        private const uint Size = 22;
        private const uint Width = 360;

        public float X { get; private set; }
        public float Y { get; private set; }

        private Rect _windowShrunkRect = new Rect(60, 20, Size*4, Size*2);
        private Rect _windowShownRect = new Rect(60, 20, Width, Size * 3);

        private Rect _windowHorizontalRect;
        private Rect _window;

        private readonly Rect _shrinkButtonRect = new Rect(0, 0, Size, Size);
        private readonly Rect _pauseButtonRect = new Rect(Size, 0, Size, Size);
        private readonly Rect _sliderRect = new Rect(Size / 2f, Size * 2, Width - Size, Size);

        private readonly string[] _shrinkButtonTextOptions = new[] { "ʌ", "v" };
        private readonly string[] _pauseButtonTextOptions = new[] { "❚❚", "►" };

        private string _shrinkButtonText;
        private string _pauseButtonText;

        private bool _ready;
        private bool _clicked;
        private bool _visible;
        public bool Shrunk { get; private set; }
        public bool Paused { get; private set; }

        private float _t0;

        private int _state;
        private const int AnimSpeed = 4;

        private const float Shift = 360;
        private string _timeValue;
        private float _sliderValue;
        public float SliderValue
        {
            get { return _sliderValue; }
            set
            {
                _sliderValue = (value);
                var minutes = (Shift + _sliderValue * 1440) % 1440;
                var hours = minutes / 60;
                _timeValue = string.Format("{0:00}:{1:00}", (int)hours, (int)minutes % 60);
            }
        }

        private GUIStyle _windowStyle;
        private CameraController _cameraController;

        void Update()
        {
            _visible = !_cameraController.m_freeCamera;
        }

        void OnGUI()
        {
            if (!_visible) return;
            
            if (_windowStyle == null)
            {
                if (!_ready)
                    Init();

                _windowStyle = GUI.skin.window;
                _windowStyle.alignment = TextAnchor.UpperRight;
            }

            _window = GUI.Window(0, _window, WindowFunc, _timeValue, _windowStyle);
            X = _window.x;
            Y = _window.y;
        }

        public void Init(bool paused = false, bool shrunk = false, float x = 60, float y = 20, float sliderValue = 0)
        {
            Shrunk = shrunk;
            Paused = paused;

            _shrinkButtonText = Shrunk ? _shrinkButtonTextOptions[1] : _shrinkButtonTextOptions[0];
            _pauseButtonText = Paused ? _pauseButtonTextOptions[1] : _pauseButtonTextOptions[0];

            X = x;
            Y = y;

            _windowShrunkRect = new Rect(X, Y, Size * 4, Size * 2);
            _windowShownRect = new Rect(X, Y, Width, Size * 3);

            _window = Shrunk ? _windowShrunkRect : _windowShownRect;
            _windowHorizontalRect = new Rect(_windowShownRect.x, _windowShownRect.y, _windowShownRect.width,
                                             _windowShrunkRect.height);

            SliderValue = sliderValue;

            var gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
                _cameraController = gameObject.GetComponent<CameraController>();
            }

            _ready = true;
        }

        void WindowFunc(int id)
        {
            Hide(ref _clicked);

            if (GUI.Button(_shrinkButtonRect, _shrinkButtonText))
            {
                Shrunk = !Shrunk;
                _clicked = true;
                _t0 = Time.time;
                _state = 0;

                _windowShownRect.x = _window.x;
                _windowShownRect.y = _window.y;
                _windowHorizontalRect.x = _window.x;
                _windowHorizontalRect.y = _window.y;
                _windowShrunkRect.x = _window.x;
                _windowShrunkRect.y = _window.y;
            }
            else if (GUI.Button(_pauseButtonRect, _pauseButtonText))
            {
                Pause();
            }

            SliderValue = GUI.HorizontalSlider(_sliderRect, _sliderValue, 0, 1);

            GUI.DragWindow();
        }

        void Hide(ref bool click)
        {
            if (!click) return;

            if (Shrunk)
            {
                _shrinkButtonText = _shrinkButtonTextOptions[1];

                if (_window == _windowHorizontalRect) _state = 1;
                else if (_window == _windowShrunkRect) _state = 2;

                if (_state == 0)
                    _window = Animate(_clicked, _windowShownRect, _windowHorizontalRect, _t0);
                else if (_state == 1)
                    _window = Animate(_clicked, _windowHorizontalRect, _windowShrunkRect, _t0 + 1.0f / AnimSpeed);
                else if (_state == 2)
                    click = false;
            }
            else
            {
                _shrinkButtonText = _shrinkButtonTextOptions[0];

                if (_window == _windowHorizontalRect) _state = 1;
                else if (_window == _windowShownRect) _state = 2;

                if (_state == 0)
                    _window = Animate(_clicked, _windowShrunkRect, _windowHorizontalRect, _t0);
                else if (_state == 1)
                    _window = Animate(_clicked, _windowHorizontalRect, _windowShownRect, _t0 + 1.0f / AnimSpeed);
                else if (_state == 2)
                    click = false;
            }
        }

        void Pause()
        {
            Paused = !Paused;
            _pauseButtonText = Paused ? _pauseButtonTextOptions[1] : _pauseButtonTextOptions[0];
        }

        static Rect Animate(bool input, Rect start, Rect end, float startTime)
        {
            if (!input) return start;

            var time = Time.time;
            var t = (time - startTime) * AnimSpeed;

            return new Rect(start.x, start.y, Mathf.Lerp(start.width, end.width, t),
                            Mathf.Lerp(start.height, end.height, t));
        }
    }
}
