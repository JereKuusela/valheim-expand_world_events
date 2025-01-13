using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using Service;
namespace ExpandWorld.Event;

public class Manager
{
  public static string FileName = "expand_events.yaml";
  public static string FilePath = Path.Combine(Yaml.Directory, FileName);
  public static string Pattern = "expand_events*.yaml";
  public static List<RandomEvent> Originals = [];


  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(FilePath)) return;
    var yaml = Yaml.Serializer().Serialize(RandEventSystem.instance.m_events.Select(Loader.ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    Set(DataManager.Read<Data, RandomEvent>(Pattern, Loader.FromData));
    // No point to send duplicate events to clients.
    var uniqueEvents = RandEventSystem.instance.m_events.Distinct(new Comparer());
    Configuration.valueEventData.Value = Yaml.Serializer().Serialize(RandEventSystem.instance.m_events.Select(Loader.ToData).ToList());
  }
  public static bool LoadDelayed = false;
  public static void FromSetting(string yaml)
  {
    // First load is delayed because RRR uses ServerSync to create monsters (race condition).
    if (LoadDelayed) return;
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (Helper.IsServer() && Originals.Count == 0)
      Originals = [.. RandEventSystem.instance.m_events];
    Loader.ExtraData.Clear();
    if (string.IsNullOrEmpty(yaml)) return;
    try
    {
      var data = Yaml.Deserialize<Data>(yaml, "Events").Select(d => Loader.FromData(d, "Events")).ToList();
      if (data.Count == 0)
      {
        Log.Warning($"Failed to load any event data.");
        return;
      }
      if (ExpandWorldData.Configuration.DataMigration && Helper.IsServer() && AddMissingEntries(data))
      {
        // Watcher triggers reload.
        return;
      }
      Log.Info($"Reloading event data ({data.Count} entries).");
      RandEventSystem.instance.m_events = data;
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
    }
  }
  private static bool AddMissingEntries(List<RandomEvent> entries)
  {
    var missingKeys = Originals.Select(e => e.m_name).Distinct().ToHashSet();
    foreach (var item in entries)
      missingKeys.Remove(item.m_name);
    if (missingKeys.Count == 0) return false;
    var missing = Originals.Where(item => missingKeys.Contains(item.m_name)).ToList();
    Log.Warning($"Adding {missing.Count} missing events to the expand_events.yaml file.");
    foreach (var item in missing)
      Log.Warning(item.m_name);
    var yaml = File.ReadAllText(FilePath);
    var data = Yaml.Serializer().Serialize(missing.Select(Loader.ToData));
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    File.WriteAllText(FilePath, yaml);
    return true;
  }
  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(Pattern, FromFile);
  }
}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
public class DelayContentLoad
{
  static void Prefix()
  {
    Manager.LoadDelayed = true;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.Last)]
public class InitializeContent
{
  static void Postfix()
  {
    if (Helper.IsServer())
    {
      Manager.ToFile();
      Manager.FromFile();
    }
  }
}


[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
public class InitializeClientContent
{
  // Not the best place but same as Expand World Spawns.
  static void Postfix()
  {
    if (Manager.LoadDelayed)
    {
      Manager.LoadDelayed = false;
      Manager.FromSetting(Configuration.valueEventData.Value);
    }
  }

}

public class Comparer : IEqualityComparer<RandomEvent>
{
  public bool Equals(RandomEvent x, RandomEvent y) => x.m_name == y.m_name;

  public int GetHashCode(RandomEvent obj) => obj.m_name.GetStableHashCode();
}