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
    ZDOVars.s_randomSkillFactor, HashDamage
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
          customData.Floats[hash] = DataValue.Simple(Parse.Float(kvp.Value, 0f));
        }
        else
        {
          // Component fields almost always start with m_.
          // So both formats have to be supported.
          var split = kvp.Key.Split('.');
          if (split.Length > 1)
          {
            componentFields[split[0]] ??= [];
            componentFields[split[0]][split[1]] = kvp.Value;
            componentFields[split[0]][$"m_{split[1]}"] = kvp.Value;

          }
          else
          {
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
      var f = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
      foreach (var info in f)
      {
        if (componentFields.TryGetValue(info.Name, out var fields) && fields.TryGetValue(info.Name, out var value))
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
      customData.Ints[key] = DataValue.Simple(Parse.Int(value, 0));
    }
    else if (info.FieldType == typeof(float))
    {
      customData.Floats ??= [];
      customData.Floats[key] = DataValue.Simple(Parse.Float(value, 0f));
    }
    else if (info.FieldType == typeof(bool))
    {
      customData.Ints ??= [];
      var b = value == "1" || value == "true";
      customData.Ints[key] = DataValue.Simple(b ? 1 : 0);
    }
    else if (info.FieldType == typeof(Vector3))
    {
      customData.Vecs ??= [];
      customData.Vecs[key] = DataValue.Simple(Parse.VectorXZY(value));
    }
    else if (info.FieldType == typeof(Quaternion))
    {
      customData.Quats ??= [];
      customData.Quats[key] = DataValue.Simple(Parse.AngleYXZ(value));
    }
    // Rest are considered strings to support possible custom types.
    else
    {
      customData.Strings ??= [];
      customData.Strings[key] = DataValue.Simple(value);
    }
  }
}