using System.Collections.Generic;
using System.ComponentModel;
using ExpandWorldData;
namespace ExpandWorld.Event;

public class Data
{
  public string name = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue(60f)]
  public float duration = 60f;
  [DefaultValue("3")]
  public string nearBaseOnly = "3";
  [DefaultValue("")]
  public string biome = "";
  [DefaultValue("")]
  public string requiredGlobalKeys = "";
  [DefaultValue("")]
  public string notRequiredGlobalKeys = "";
  [DefaultValue("")]
  public string requiredPlayerKeys = "";
  [DefaultValue("")]
  public string notRequiredPlayerKeys = "";
  [DefaultValue("")]
  public string requiredKnownItems = "";
  [DefaultValue("")]
  public string notRequiredKnownItems = "";
  [DefaultValue("")]
  public string requiredEnvironments = "";
  [DefaultValue("")]
  public string startMessage = "";
  [DefaultValue("")]
  public string endMessage = "";
  public string forceMusic = "";
  [DefaultValue("")]
  public string forceEnvironment = "";
  public Spawn.Data[] spawns = [];
  [DefaultValue(true)]
  public bool pauseIfNoPlayerInArea = true;
  [DefaultValue(true)]
  public bool random = true;
  [DefaultValue(100f)]
  public float playerDistance = 100f;
  [DefaultValue("")]
  public string playerLimit = "";
  [DefaultValue("")]
  public string eventLimit = "";

  [DefaultValue(null)]
  public string[]? startCommands = null;

  [DefaultValue(null)]
  public string[]? endCommands = null;
}
public class ExtraData
{
  public List<string> RequiredEnvironments = [];
  public float PlayerDistance = 100f;
  public Range<int>? PlayerLimit;
  public int BaseValue = 3;
  public Range<int>? EventLimit;
  public string[]? StartCommands;
  public string[]? EndCommands;

  public ExtraData(Data data)
  {
    RequiredEnvironments = DataManager.ToList(data.requiredEnvironments);
    PlayerDistance = data.playerDistance;
    if (data.playerLimit != "")
      PlayerLimit = Parse.IntRange(data.playerLimit);
    if (data.eventLimit != "")
      EventLimit = Parse.IntRange(data.eventLimit);
    if (data.nearBaseOnly != "true")
      BaseValue = Parse.Int(data.nearBaseOnly, 0);
    StartCommands = data.startCommands;
    EndCommands = data.endCommands;
  }
}
