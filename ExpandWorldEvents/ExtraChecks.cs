using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using Service;
using UnityEngine;

namespace ExpandWorld.Event;

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.InValidBiome))]
public class ExtraChecks
{
  static bool Postfix(bool result, RandomEvent ev, Vector3 point)
  {
    if (!result) return false;
    if (!Loader.ExtraData.TryGetValue(ev, out var data)) return true;
    if (!EnvCheck(point, data.RequiredEnvironments)) return false;
    if (!PlayerCheck(point, data.PlayerLimit, data.PlayerDistance)) return false;
    if (!EventCheck(point, data.EventLimit)) return false;
    return true;
  }
  private static bool EnvCheck(Vector3 pos, List<string> required)
  {
    if (required.Count == 0) return true;
    var biome = WorldGenerator.instance.GetBiome(pos);
    var em = EnvMan.instance;
    var availableEnvironments = em.GetAvailableEnvironments(biome);
    if (availableEnvironments == null || availableEnvironments.Count == 0) return false;
    Random.State state = Random.state;
    var num = (long)ZNet.instance.GetTimeSeconds() / em.m_environmentDuration;
    Random.InitState((int)num);
    var env = em.SelectWeightedEnvironment(availableEnvironments);
    Random.state = state;
    return required.Contains(env.m_name.ToLower());
  }
  private static bool PlayerCheck(Vector3 pos, Range<int>? limit, float distance)
  {
    if (limit == null) return true;
    var within = RandEventSystem.s_playerEventDatas.Where(p => Utils.DistanceXZ(pos, p.position) <= distance).Count();
    return limit.Min <= within && within <= limit.Max;
  }
  private static bool EventCheck(Vector3 pos, Range<int>? limit)
  {
    if (limit == null) return true;
    if (!Configuration.MultipleEvents) return true;
    var within = MultipleEvents.Events.Where(p => Utils.DistanceXZ(pos, p.Event.m_pos) <= Configuration.EventMinimumDistance).Sum(p => p.Count);
    return limit.Min <= within && within <= limit.Max;
  }
}

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.CheckBase))]
public class CheckBase
{
  static bool Prefix(RandomEvent ev, RandEventSystem.PlayerEventData player, ref bool __result)
  {
    if (!Loader.ExtraData.TryGetValue(ev, out var data)) return true;
    __result = player.baseValue >= data.BaseValue;
    return false;
  }
}