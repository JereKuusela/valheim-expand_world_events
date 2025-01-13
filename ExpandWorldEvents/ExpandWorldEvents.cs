using System;
using System.Linq;
using BepInEx;
using ExpandWorld.Event;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", "1.50")]
public class EWE : BaseUnityPlugin
{
  public const string GUID = "expand_world_events";
  public const string NAME = "Expand World Events";
  public const string VERSION = "1.13";

  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public void Awake()
  {
    ConfigWrapper wrapper = new("expand_events_config", Config, ConfigSync, () => { });
    Configuration.Init(wrapper);
    new Harmony(GUID).PatchAll();
    try
    {
      Yaml.SetupWatcher(Config);
      if (ExpandWorldData.Configuration.DataReload)
      {
        Manager.SetupWatcher();
      }
    }
    catch (Exception e)
    {
      Log.Error(e.StackTrace);
    }
  }

  public static RandomEvent GetCurrentRandomEvent(Vector3 pos)
  {
    if (Configuration.MultipleEvents)
      return MultipleEvents.Events.OrderBy(x => Utils.DistanceXZ(x.Event.m_pos, pos)).FirstOrDefault()?.Event!;
    return RandEventSystem.instance.GetCurrentRandomEvent();
  }
#pragma warning disable IDE0051
  private void OnDestroy()
  {
    Config.Save();
  }
#pragma warning restore IDE0051
}
