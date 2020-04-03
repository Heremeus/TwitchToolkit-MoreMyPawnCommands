using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace TwitchToolkitMoreMyPawnCommands
{
    public class ModSettings : ModBase
    {
        public static ModSettings Singleton { get; private set; }
        public override string ModIdentifier => "TwitchToolkitMoreMyPawnCommands";

        private SettingHandle<bool> myPawnGearShowArmorValuesHandle;
        public bool MyPawnGearShowArmorValues => myPawnGearShowArmorValuesHandle.Value;
        private SettingHandle<bool> myPawnGearShowComfyTempHandle;
        public bool MyPawnGearShowComfyTemp => myPawnGearShowComfyTempHandle.Value;
        private SettingHandle<bool> myPawnGearShowWeaponsHandle;
        public bool MyPawnGearShowWeapons => myPawnGearShowWeaponsHandle.Value;
        private SettingHandle<bool> myPawnGearShowApparelHandle;
        public bool MyPawnGearShowApparel => myPawnGearShowApparelHandle.Value;
        private SettingHandle<bool> myPawnHealthShowSurgeriesHandle;
        public bool MyPawnHealthShowSurgeries => myPawnHealthShowSurgeriesHandle.Value;

        public ModSettings() : base()
        {
            Singleton = this;
        }

        public override void DefsLoaded()
        {
            Log.Message("TwitchToolkitMoreMyPawnCommands loaded");
            myPawnGearShowComfyTempHandle = Settings.GetHandle("myPawnGearShowComfyTempHandle", "!mypawngear show comfy temperature", "If enabled, !mypawngear will list the viewer's colonist's comfortable temperature range.", true);
            myPawnGearShowArmorValuesHandle = Settings.GetHandle("myPawnGearShowArmorValuesHandle", "!mypawngear show armor", "If enabled, !mypawngear will list the viewer's colonist's total armor percentages.", true);
            myPawnGearShowWeaponsHandle = Settings.GetHandle("myPawnGearShowWeaponsHandle", "!mypawngear show weapons", "If enabled, !mypawngear will list the viewer's colonist's weapons.", true);
            myPawnGearShowApparelHandle = Settings.GetHandle("myPawnGearShowApparelHandle", "!mypawngear show apparel", "If enabled, !mypawngear will list the viewer's colonist's apparel.", true);
            myPawnHealthShowSurgeriesHandle = Settings.GetHandle("myPawnHealthShowSurgeriesHandle", "!mypawnhealth show surgeries", "If enabled, !mypawnhealth will list the viewer's colonist's queued surgeries.", true);
        }
    }
}