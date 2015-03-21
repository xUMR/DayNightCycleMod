namespace DayNightCycleMod
{
    public class ModUIData
    {
        public bool Paused;
        public bool Shrunk;

        public float X;
        public float Y;

        public float Time;

        public ModUIData(bool paused, bool shrunk, float x, float y, float time)
        {
            Paused = paused;
            Shrunk = shrunk;
            X = x;
            Y = y;
            Time = time;
        }

        public ModUIData() 
            : this(false, false, 60, 20, 0) { }
    }
}
