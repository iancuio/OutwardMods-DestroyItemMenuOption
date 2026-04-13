using BepInEx;
using HarmonyLib;
using System.Collections.Generic;

namespace DestroyItemMenuOptionMod
{
    [BepInPlugin("com.iancula.destroyitemmenuoption", "Destroy Item Menu Option Mod", "1.0.1")]
    public class DestroyItemMenuOptionPlugin : BaseUnityPlugin
    {
        internal void Awake()
        {
            var harmony = new Harmony("com.iancula.destroyitemmenuoption");
            harmony.PatchAll();
            Logger.LogInfo("Destroy Item Menu Option Mod loaded successfully!");
        }
    }

    [HarmonyPatch(typeof(ItemDisplayOptionPanel), "GetActiveActions")]
    public class ItemDisplayOptionPanel_GetActiveActions_Patch
    {
        private const int DestroyActionID = 631;

        [HarmonyPostfix]
        public static void Postfix(ref List<int> __result)
        {
            if (__result != null)
                __result.Add(DestroyActionID);
        }
    }

    [HarmonyPatch(typeof(ItemDisplayOptionPanel), "GetActionText")]
    public class ItemDisplayOptionPanel_GetActionText_Patch
    {
        private const int DestroyActionID = 631;

        [HarmonyPrefix]
        public static bool Prefix(int _actionID, ref string __result)
        {
            if (_actionID == DestroyActionID)
            {
                __result = "Destroy Item";
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemDisplayOptionPanel), "ActionHasBeenPressed")]
    public class ItemDisplayOptionPanel_ActionHasBeenPressed_Patch
    {
        private const int DestroyActionID = 631;

        [HarmonyPrefix]
        public static bool Prefix(ItemDisplayOptionPanel __instance, int _actionID)
        {
            if (_actionID != DestroyActionID)
                return true;

            Item targetItem = Traverse.Create(__instance)
                                      .Field("m_pendingItem")
                                      .GetValue<Item>();

            CharacterUI characterUI = Traverse.Create(__instance)
                                              .Field("m_characterUI")
                                              .GetValue<CharacterUI>();

            if (targetItem == null || characterUI == null)
                return false;

            characterUI.ContextMenu.Hide();

            characterUI.MessagePanel.Show(
                _text: $"Are you sure you want to destroy \"{targetItem.Name}\"? This action is permanent.",
                _caption: "Destroy Item",
                _acceptCallback: () =>
                {
                    if (targetItem != null && !string.IsNullOrEmpty(targetItem.UID))
                    {
                        ItemManager.Instance.DestroyItem(targetItem.UID);
                        characterUI.ContextMenu.Hide();
                    }
                },
                _cancelCallback: () => { },
                _yesNo: true
            );

            return false;
        }
    }
}