using ICities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DayNightCycleMod
{
    public class Mod : IUserMod
    {
        public string Name { get { return "Day/Night"; } }
        public string Description { get { return "Adds day-night Cycle"; } }
    }

    public class ModLoading : LoadingExtensionBase
    {
        private DayNightCycle _cycle;
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
                return;

            var sunLight = Object.FindObjectOfType<Light>();
            var sun = sunLight.gameObject;
            _cycle = sun.AddComponent<DayNightCycle>();
            _cycle.Init(sunLight, managers.threading);
        }

        public override void OnLevelUnloading()
        {
            _cycle.enabled = false;
            _cycle.GetOptions().Serialize();
        }
    }
}
