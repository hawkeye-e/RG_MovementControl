using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.IL2CPP;
using RG.Scene;
using RG.Scene.Action.Core;
using RG.Scripts;
using Il2CppSystem.Collections.Generic;

namespace MovementControl
{
    [BepInProcess("RoomGirl")]
    [BepInPlugin(GUID, PluginName, Version)]
    public class MovementControlPlugin : BasePlugin
    {
        public const string PluginName = "MovementControl";
        public const string GUID = "hawk.RG.MovementControl";
        public const string Version = "0.1.0";

        internal static new ManualLogSource Log;

        public override void Load()
        {
            Log = base.Log;

            MovementControl.Config.Init(this);

            if (MovementControl.Config.Enabled)
            {
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
            }

            StateManager.Instance = new StateManager();
        }

        private static class Hooks
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Actor), nameof(Actor.ChangeState), new[] { typeof(RG.Define.StateID), typeof(bool) })]
            private static void ChangeState1Pre(Actor __instance, ref RG.Define.StateID stateID, bool forceReset)
            {
                if (!IsPluginEnabled()) return;

                if (stateID == RG.Define.StateID.SummonOutsider)
                    StateManager.Instance.isSummoningOutsider = true;
                else if (stateID == RG.Define.StateID.SummonEnd)
                    StateManager.Instance.isSummoningOutsider = false;

                if (!IsAllowMovement(__instance, stateID))
                {
                    stateID = RG.Define.StateID.Idle;
                    __instance.CompleteAction();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Actor), nameof(Actor.ChangeState), new[] { typeof(RG.Define.StateID), typeof(bool) })]
            private static void ChangeState1Post(Actor __instance, RG.Define.StateID stateID, bool forceReset)
            {
                if (!IsPluginEnabled()) return;

                if (__instance == StateManager.Instance.enteringActor && stateID == RG.Define.StateID.GoToDestination)
                    StateManager.Instance.enteringActor = null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Actor), nameof(Actor.ChangeState), new[] { typeof(int), typeof(bool) })]
            private static void ChangeState2Pre(Actor __instance, ref int stateType, bool forceReset)
            {
                if (!IsPluginEnabled()) return;

                if (stateType == 15)
                    StateManager.Instance.isSummoningOutsider = true;
                else if (stateType == 17)
                    StateManager.Instance.isSummoningOutsider = false;
                
                if (!IsAllowMovement(__instance, stateType))
                {
                    stateType = 0;
                    __instance.CompleteAction();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Actor), nameof(Actor.ChangeState), new[] { typeof(int), typeof(bool) })]
            private static void ChangeState2(Actor __instance, int stateType, bool forceReset)
            {
                if (!IsPluginEnabled()) return;

                if (__instance == StateManager.Instance.enteringActor && stateType == 2)
                    StateManager.Instance.enteringActor = null;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.RegisterActor))]
            private static void RegisterActor(Actor actor)
            {
                if (!IsPluginEnabled()) return;

                StateManager.Instance.enteringActor = actor;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.StartTurnSequence))]
            private static void StartTurnSequencePre(Actor target, int stateID, Actor partner, int subStateID, bool resetForce, bool isADV)
            {
                if (!IsPluginEnabled()) return;

                StateManager.Instance.actionActor = target;
                StateManager.Instance.partnerActor = partner;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActionScene), nameof(ActionScene.RefreshNextMove))]
            private static void RefreshNextMovePost(IReadOnlyList<Actor> actors)
            {
                if (!IsPluginEnabled()) return;

                StateManager.Instance.actionActor = null;
                StateManager.Instance.partnerActor = null;
            }



            private static bool IsAllowMovement(Actor actor, RG.Define.StateID stateID)
            {
                int stateType = -999;

                if (stateID == RG.Define.StateID.GoToDestination) stateType = 2;
                else if (stateID == RG.Define.StateID.GoSideCharacter) stateType = 4;
                else if (stateID == RG.Define.StateID.Follow) stateType = 3;

                if (stateType != -999)
                {
                    return IsAllowMovement(actor, stateType);
                }

                return true;
            }

            private static bool IsAllowMovement(Actor actor, int stateID)
            {
                if (ActionScene.Instance._mainActor == null ||                                  //have not select any character
                    (actor.InstanceID != ActionScene.Instance._mainActor.InstanceID               //the actor in question is not the selected actor
                    && actor != StateManager.Instance.enteringActor                             //the actor in question is not entering the room
                    && actor != StateManager.Instance.actionActor
                    && actor != StateManager.Instance.partnerActor
                    )
                    )
                {
                    if (stateID == 2 || stateID == 4 || stateID == 3)
                    {
                        return false;
                    }
                }

                return true;
            }

            private static bool IsPluginEnabled()
            {
                return MovementControl.Config.Enabled;
            }
        }
    }

}
