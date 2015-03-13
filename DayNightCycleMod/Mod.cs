using ICities;

namespace DayNightCycleMod
{
    public class Mod : IUserMod
    {
        public string Name { get { return "Day/Night"; } }
        public string Description { get { return "Adds day-night cycle"; } }
    }

    public class ModThreading : ThreadingExtensionBase
    {
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            DayNightCycle.Instance.Update(simulationTimeDelta);
        }
    }
}
