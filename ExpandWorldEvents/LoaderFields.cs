using System.Collections.Generic;
using System.Reflection;
using Data;
using Service;
using UnityEngine;

namespace ExpandWorld.Spawn;

public class LoaderFields
{
  private static readonly int HashFaction = "faction".GetStableHashCode();
  private static readonly int HashDamage = "damage".GetStableHashCode();
  private static readonly HashSet<int> KnownFloats =
  [
    ZDOVars.s_randomSkillFactor,
    HashDamage,
    ZDOVars.s_health,
    ZDOVars.s_maxHealth,
    ZDOVars.s_noise,
    ZDOVars.s_scaleScalarHash,
  ];
  private static readonly HashSet<int> KnownInts =
  [
    ZDOVars.s_level,
    ZDOVars.s_seed,
    ZDOVars.s_lovePoints
  ];
  private static readonly HashSet<int> KnownLongs =
  [
    ZDOVars.s_spawnTime,
    ZDOVars.s_worldTimeHash,
    ZDOVars.s_pregnant
  ];
  private static readonly HashSet<int> KnownBools =
  [
    "bosscount".GetStableHashCode(),
    ZDOVars.s_isBlockingHash,
    ZDOVars.s_tamed,
    ZDOVars.s_aggravated,
    ZDOVars.s_alert,
    ZDOVars.s_shownAlertMessage,
    ZDOVars.s_huntPlayer,
    ZDOVars.s_patrol,
    ZDOVars.s_despawnInDay,
    ZDOVars.s_eventCreature,
    ZDOVars.s_sleeping,
    ZDOVars.s_haveSaddleHash
  ];
  private static readonly HashSet<int> KnownVecs =
  [
    ZDOVars.s_bodyVelocity,
    ZDOVars.s_spawnPoint,
    ZDOVars.s_patrolPoint,
    ZDOVars.s_scaleHash
  ];
  private static readonly HashSet<int> KnownStrings =
  [
    ZDOVars.s_tamedName,
    ZDOVars.s_tamedNameAuthor
  ];

  public static DataEntry? HandleCustomData(Data data, SpawnSystem.SpawnData spawn)
  {
    DataEntry? customData = null;
    if (data.faction != null)
    {
      customData ??= new();
      customData.Strings ??= [];
      customData.Strings[HashFaction] = DataValue.Simple(data.faction);
    }
    if (data.fields != null)
    {
      customData ??= new();
      Dictionary<string, string> otherFields = [];
      Dictionary<string, Dictionary<string, string>> componentFields = [];
      foreach (var kvp in data.fields)
      {
        var hash = kvp.Key.GetStableHashCode();
        if (KnownFloats.Contains(hash))
        {
          customData.Floats ??= [];
          if (hash == HashDamage)
            hash = ZDOVars.s_randomSkillFactor;
          customData.Floats[hash] = DataValue.Float(kvp.Value);
        }
        else if (KnownInts.Contains(hash))
        {
          customData.Ints ??= [];
          customData.Ints[hash] = DataValue.Int(kvp.Value);
        }
        else if (KnownLongs.Contains(hash))
        {
          customData.Longs ??= [];
          customData.Longs[hash] = DataValue.Long(kvp.Value);
        }
        else if (KnownBools.Contains(hash))
        {
          customData.Bools ??= [];
          customData.Bools[hash] = DataValue.Bool(kvp.Value);
        }
        else if (KnownVecs.Contains(hash))
        {
          customData.Vecs ??= [];
          customData.Vecs[hash] = DataValue.Vector3(kvp.Value);
        }
        else if (KnownStrings.Contains(hash))
        {
          customData.Strings ??= [];
          customData.Strings[hash] = DataValue.String(kvp.Value);
        }
        else
        {
          var split = kvp.Key.Split('.');
          if (split.Length > 1)
          {
            var c = split[0];
            if (c == "int")
            {
              customData.Ints ??= [];
              customData.Ints[split[1].GetStableHashCode()] = DataValue.Int(kvp.Value);
            }
            else if (c == "float")
            {
              customData.Floats ??= [];
              customData.Floats[split[1].GetStableHashCode()] = DataValue.Float(kvp.Value);
            }
            else if (c == "bool")
            {
              customData.Bools ??= [];
              customData.Bools[split[1].GetStableHashCode()] = DataValue.Bool(kvp.Value);
            }
            else if (c == "vec")
            {
              customData.Vecs ??= [];
              customData.Vecs[split[1].GetStableHashCode()] = DataValue.Vector3(kvp.Value);
            }
            else if (c == "quat")
            {
              customData.Quats ??= [];
              customData.Quats[split[1].GetStableHashCode()] = DataValue.Quaternion(kvp.Value);
            }
            else if (c == "string")
            {
              customData.Strings ??= [];
              customData.Strings[split[1].GetStableHashCode()] = DataValue.Simple(kvp.Value);
            }
            else
            {
              if (!componentFields.ContainsKey(c))
                componentFields[c] = [];
              componentFields[c][split[1]] = kvp.Value;
            }
          }
          else
          {
            // Component fields almost always start with m_.
            // So both formats have to be supported for easier usage.
            otherFields[kvp.Key] = kvp.Value;
            otherFields[$"m_{kvp.Key}"] = kvp.Value;
          }
        }
      }
      HandleFields(spawn, customData, componentFields, otherFields);
    }
    return customData;
  }
  private static void HandleFields(SpawnSystem.SpawnData spawn, DataEntry customData, Dictionary<string, Dictionary<string, string>> componentFields, Dictionary<string, string> otherFields)
  {
    // Need to check every component of the prefab to determine the component type.
    spawn.m_prefab.GetComponentsInChildren(ZNetView.m_tempComponents);
    foreach (var component in ZNetView.m_tempComponents)
    {
      var c = component.GetType();
      var f = c.GetFields(BindingFlags.Instance | BindingFlags.Public);
      foreach (var info in f)
      {
        if (componentFields.TryGetValue(c.Name, out var fields) && fields.TryGetValue(info.Name, out var value))
          InsertData(customData, component, info, value);
        if (otherFields.TryGetValue(info.Name, out var otherValue))
          InsertData(customData, component, info, otherValue);
      }
    }
    ZNetView.m_tempComponents.Clear();
  }
  private static void InsertData(DataEntry customData, Component component, FieldInfo info, string value)
  {
    var key = $"{component.GetType().Name}.{info.Name}".GetStableHashCode();
    customData.Ints ??= [];
    customData.Ints["HasFields".GetStableHashCode()] = DataValue.Simple(1);
    customData.Ints[$"HasFields{component.GetType().Name}".GetStableHashCode()] = DataValue.Simple(1);
    if (info.FieldType == typeof(int))
    {
      customData.Ints ??= [];
      customData.Ints[key] = DataValue.Int(value);
    }
    else if (info.FieldType == typeof(float))
    {
      customData.Floats ??= [];
      customData.Floats[key] = DataValue.Float(value);
    }
    else if (info.FieldType == typeof(bool))
    {
      customData.Bools ??= [];
      customData.Bools[key] = DataValue.Bool(value);
    }
    else if (info.FieldType == typeof(Vector3))
    {
      customData.Vecs ??= [];
      customData.Vecs[key] = DataValue.Vector3(value);
    }
    else if (info.FieldType == typeof(Quaternion))
    {
      customData.Quats ??= [];
      customData.Quats[key] = DataValue.Quaternion(value);
    }
    // Rest are considered strings to support possible custom types.
    else
    {
      customData.Strings ??= [];
      customData.Strings[key] = DataValue.Simple(value);
    }
  }
}