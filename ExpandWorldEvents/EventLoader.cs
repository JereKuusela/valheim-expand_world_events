using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
namespace ExpandWorld.Event;

public class Loader
{
  public static Dictionary<RandomEvent, ExtraData> ExtraData = [];

  public static RandomEvent FromData(Data data)
  {
    RandomEvent random = new()
    {
      m_name = data.name,
      m_spawn = data.spawns.Select(Spawn.Loader.FromData).Where(s => s.m_prefab).ToList(),
      m_enabled = data.enabled,
      m_random = data.random,
      m_duration = data.duration,
      m_nearBaseOnly = data.nearBaseOnly != "false",
      m_pauseIfNoPlayerInArea = data.pauseIfNoPlayerInArea,
      m_biome = DataManager.ToBiomes(data.biome),
      m_requiredGlobalKeys = DataManager.ToList(data.requiredGlobalKeys),
      m_notRequiredGlobalKeys = DataManager.ToList(data.notRequiredGlobalKeys),
      m_altRequiredPlayerKeysAny = DataManager.ToList(data.requiredPlayerKeys),
      m_altRequiredPlayerKeysAll = DataManager.ToList(data.requiredPlayerKeysAll),
      m_altNotRequiredPlayerKeys = DataManager.ToList(data.notRequiredPlayerKeys),
      m_altRequiredKnownItems = DataManager.ToItemList(data.requiredKnownItems),
      m_altRequiredNotKnownItems = DataManager.ToItemList(data.notRequiredKnownItems),
      m_startMessage = data.startMessage,
      m_endMessage = data.endMessage,
      m_forceMusic = data.forceMusic,
      m_forceEnvironment = data.forceEnvironment,
      m_eventRange = data.radius,
      m_standaloneChance = data.customChance,
      m_standaloneInterval = data.customInterval,
      m_spawnerDelay = data.spawnerDelay,
    };
    ExtraData[random] = new(data);
    return random;
  }
  public static Data ToData(RandomEvent random)
  {
    Data data = new()
    {
      name = random.m_name,
      spawns = random.m_spawn.Select(Spawn.Loader.ToData).ToArray(),
      enabled = random.m_enabled,
      random = random.m_random,
      duration = random.m_duration,
      nearBaseOnly = random.m_nearBaseOnly ? "true" : "false",
      pauseIfNoPlayerInArea = random.m_pauseIfNoPlayerInArea,
      biome = DataManager.FromBiomes(random.m_biome),
      requiredGlobalKeys = DataManager.FromList(random.m_requiredGlobalKeys),
      notRequiredGlobalKeys = DataManager.FromList(random.m_notRequiredGlobalKeys),
      requiredPlayerKeys = DataManager.FromList(random.m_altRequiredPlayerKeysAny),
      requiredPlayerKeysAll = DataManager.FromList(random.m_altRequiredPlayerKeysAll),
      notRequiredPlayerKeys = DataManager.FromList(random.m_altNotRequiredPlayerKeys),
      requiredKnownItems = DataManager.FromList(random.m_altRequiredKnownItems),
      notRequiredKnownItems = DataManager.FromList(random.m_altRequiredNotKnownItems),
      startMessage = random.m_startMessage,
      endMessage = random.m_endMessage,
      forceMusic = random.m_forceMusic,
      forceEnvironment = random.m_forceEnvironment,
      radius = random.m_eventRange,
      customChance = random.m_standaloneChance,
      customInterval = random.m_standaloneInterval,
      spawnerDelay = random.m_spawnerDelay,
    };
    return data;
  }
}


[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.SetRandomEvent)), HarmonyPriority(Priority.First)]
public class MultipleConfigurationsPerEvent
{
  static void Prefix(RandEventSystem __instance, ref RandomEvent ev)
  {
    // Clients automatically get the first event by name.
    // So this is for the server (self host) if multiple configurations are used.
    ev = __instance.GetEvent(ev?.m_name);
  }
}
