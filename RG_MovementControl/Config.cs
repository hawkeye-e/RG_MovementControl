using BepInEx.Configuration;
using BepInEx.IL2CPP;

namespace MovementControl
{
    internal class Config
    {
        private const string GENERAL = "General";

        internal static bool Enabled { get { return _enabled.Value; } }
        private static ConfigEntry<bool> _enabled;


        internal static void Init(BasePlugin plugin)
        {
            _enabled = plugin.Config.Bind(GENERAL, "Enable this plugin", false, "If true, the characters is not allowed to move to another location unless it is player's command.");
        }

    }
}
