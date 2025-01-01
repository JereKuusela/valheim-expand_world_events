using System.Collections.Generic;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Event;

public class MultiEvent(RandomEvent ev, int count)
{
  public RandomEvent Event = ev;
  public int Count = count;
}

[HarmonyPatch(typeof(RandEventSystem))]
public class MultipleEvents
{
  public static readonly List<MultiEvent> Events = [];

  [HarmonyPatch(nameof(RandEventSystem.FixedUpdate)), HarmonyPrefix]
  static bool FixedUpdate(RandEventSystem __instance)
  {
    if (Helper.IsClient() || !Configuration.MultipleEvents) return true;
    var dt = Time.fixedDeltaTime;
    var obj = __instance;
    obj.UpdateForcedEvents(dt);
    obj.UpdateRandomEvent(dt);
    obj.m_forcedEvent?.Update(true, true, true, dt);
    var expired = Events.Where(x =>
    {
      var playerInArea = __instance.IsAnyPlayerInEventArea(x.Event);
      return x.Event.Update(true, true, playerInArea, Time.fixedDeltaTime);
    }).ToList();
    expired.ForEach(x => x.Event.OnStop());
    Events.RemoveAll(expired.Contains);
    if (obj.m_forcedEvent != null)
      obj.SetActiveEvent(obj.m_forcedEvent);
    else if (Player.m_localPlayer)
    {
      // This is simply for testing in single player.
      // Dedicated server doesn't need m_randomEvent for anything.
      var randomEvent = Events.OrderBy(x => Utils.DistanceXZ(x.Event.m_pos, Player.m_localPlayer.transform.position)).FirstOrDefault()?.Event;
      obj.m_randomEvent = randomEvent;
      if (randomEvent != null && obj.IsInsideRandomEventArea(randomEvent, Player.m_localPlayer.transform.position))
        __instance.SetActiveEvent(randomEvent);
      else
        obj.SetActiveEvent(null);
    }
    else
      obj.SetActiveEvent(null);
    return false;
  }

  [HarmonyPatch(nameof(RandEventSystem.SetRandomEvent)), HarmonyPrefix]
  static bool SetRandomEvent(RandEventSystem __instance, RandomEvent ev, Vector3 pos)
  {
    if (Helper.IsClient() || !Configuration.MultipleEvents) return true;
    if (ev == null)
    {
      // For stopevent command.
      // Copy because OnStop() can start new events.
      var toStop = Events.ToList();
      toStop.ForEach(x => x.Event.OnStop());
      Events.RemoveAll(toStop.Contains);
      return false;
    }
    var nearbyEvents = Events.Where(x => Utils.DistanceXZ(x.Event.m_pos, pos) < Configuration.EventMinimumDistance).ToList();
    nearbyEvents.ForEach(x => x.Event.OnStop());
    Events.RemoveAll(nearbyEvents.Contains);

    var randomEvent = ev.Clone();
    randomEvent.m_pos = pos;
    randomEvent.OnStart();
    var count = nearbyEvents.Sum(x => x.Count) + 1;
    Events.Add(new(randomEvent, count));
    __instance.SendCurrentRandomEvent();
    return false;
  }

  [HarmonyPatch(nameof(RandEventSystem.SendCurrentRandomEvent)), HarmonyPrefix]
  static bool SendCurrentRandomEvent()
  {
    if (Helper.IsClient() || !Configuration.MultipleEvents || Events.Count == 0) return true;
    if (Events.Count == 1)
    {
      var randomEvent = Events.First().Event;
      ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SetEvent", [randomEvent.m_name, randomEvent.m_time, randomEvent.m_pos]);
      return false;
    }
    var peers = ZNet.instance.GetPeers();
    peers.ForEach(peer =>
    {
      if (peer.m_rpc == null) return;
      var randomEvent = Events.OrderBy(x => Utils.DistanceXZ(x.Event.m_pos, peer.m_refPos)).First().Event;
      ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "SetEvent", [randomEvent.m_name, randomEvent.m_time, randomEvent.m_pos]);
    });
    return false;
  }
}
