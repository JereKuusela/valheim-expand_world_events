using BepInEx;
namespace ExpandWorld;

[BepInPlugin(GUID, NAME, VERSION)]
public class EWE : BaseUnityPlugin
{
  public const string GUID = "expand_world_events";
  public const string NAME = "Expand World Events";
  public const string VERSION = "1.19";

  public void Awake()
  {
    Logger.LogWarning("Expand World Events is deprecated and no longer functional. Event functionality has moved to Expand World Data.");
  }
}
