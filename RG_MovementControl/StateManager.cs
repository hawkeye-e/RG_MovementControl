using RG.Scene.Action.Core;

namespace MovementControl
{
    internal class StateManager
    {
        public StateManager()
        {
        }

        internal static StateManager Instance;


        internal Actor enteringActor = null;
        internal bool isSummoningOutsider = false;

        internal Actor actionActor = null;
        internal Actor partnerActor = null;
    }
}