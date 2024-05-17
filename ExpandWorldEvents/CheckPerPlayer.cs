using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Event;

[HarmonyPatch(typeof(RandEventSystem))]
public class CheckPerPlayer
{

  [HarmonyPatch(nameof(RandEventSystem.UpdateRandomEvent)), HarmonyPrefix]
  static void FixedUpdate(RandEventSystem __instance, float dt)
  {
    if (Helper.IsClient() || !Configuration.CheckPerPlayer) return;
    if (Game.m_eventRate == 0f) return;
    if (RandEventSystem.s_randomEventNeedsRefresh)
      RandEventSystem.RefreshPlayerEventData();
    CheckGlobalEvent(__instance, dt);
    CheckStandaloneEvents(__instance, dt);
  }

  private static void CheckGlobalEvent(RandEventSystem res, float dt)
  {
    // Event timer not increased because the original function runs.
    if (res.m_eventTimer + dt <= res.m_eventIntervalMin * 60f * Game.m_eventRate) return;
    res.m_eventTimer = -dt;
    foreach (var player in RandEventSystem.s_playerEventDatas)
    {
      if (Random.Range(0f, 100f) > res.m_eventChance / Game.m_eventRate) continue;
      var events = GetPossibleRandomEvents(res, player);
      if (events.Count == 0) continue;
      var ev = events[Random.Range(0, events.Count)];
      res.SetRandomEvent(ev.Key, ev.Value);
    }
  }
  private static void CheckStandaloneEvents(RandEventSystem res, float dt)
  {
    foreach (var player in RandEventSystem.s_playerEventDatas)
    {
      List<RandEventSystem.PlayerEventData> data = [player];
      foreach (var ev in res.m_events)
      {
        if (!ev.m_enabled || ev.m_standaloneChance == 0f || res.m_activeEvent == ev) continue;
        // Event timer not increased because the original function runs.
        if (ev.m_time + dt <= ev.m_standaloneInterval * Game.m_eventRate) continue;
        ev.m_time = -dt;
        if (Random.Range(0f, 100f) > ev.m_standaloneChance / Game.m_eventRate) continue;
        if (!res.HaveGlobalKeys(ev, data)) continue;
        var validEventPoints = res.GetValidEventPoints(ev, data);
        if (validEventPoints.Count == 0) continue;
        res.SetRandomEvent(ev, validEventPoints[Random.Range(0, validEventPoints.Count)]);
      }
    }
  }

  private static List<KeyValuePair<RandomEvent, Vector3>> GetPossibleRandomEvents(RandEventSystem obj, RandEventSystem.PlayerEventData player)
  {
    obj.m_lastPossibleEvents.Clear();
    foreach (RandomEvent randomEvent in obj.m_events)
    {
      List<RandEventSystem.PlayerEventData> data = [player];
      if (!randomEvent.m_enabled || !randomEvent.m_random || !obj.HaveGlobalKeys(randomEvent, data))
        continue;

      var validEventPoints = obj.GetValidEventPoints(randomEvent, data);
      if (validEventPoints.Count == 0)
        continue;

      var value = validEventPoints[Random.Range(0, validEventPoints.Count)];
      obj.m_lastPossibleEvents.Add(new(randomEvent, value));

    }
    return obj.m_lastPossibleEvents;
  }

  private static bool IsValidEvent(RandEventSystem obj, RandomEvent ev, RandEventSystem.PlayerEventData player)
  {
    List<RandEventSystem.PlayerEventData> data = [player];
    if (!ev.m_enabled || !ev.m_random || !obj.HaveGlobalKeys(ev, data))
      return false;

    var validEventPoints = obj.GetValidEventPoints(ev, data);
    return validEventPoints.Count > 0;
  }
}
