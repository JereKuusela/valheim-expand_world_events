# Expand World Events

Allows configuring the random event system.

Install on all clients and the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install [Expand World Data](https://valheim.thunderstore.io/package/JereKuusela/Expand_World_Data/).

Some features only require server installation.

## Features

- Add new events and edit existing ones.
- Allows multiple events to be active at the same time.
- Change event frequency.

Server side mode can be enabled from Expand World Data config.

## Configuration

See the [wiki](https://valheim.fandom.com/wiki/Events) for more info about events.

The file `expand_events.cfg` is created when starting the game.

The file `expand_world/expand_events.yaml` is created when loading a world.

### expand_events.cfg

All settings are server side:

- **Random event interval**: How often the game tries to start a random event.
  - Default is `46` minutes.
- **Random event chance**: The chance to try starting a random event.
  - Default is `20` percent.
- **Multiple events**: If enabled, multiple events can be active at the same time.
  - Default is `false`.
  - When a new event starts, the previous one stays active (unless too close).
  - Clients receive the closest event.
  - Active events are not saved to the save file. Restarting the server removes active events.
- **Minimum distance between events**: When multiple events are enabled, new events cancel previous events within this distance.
  - Default is `100` meters.
- **Check per player**: If enabled, the event check is done separately for each player.
  - Default is `false`.
  - This makes events much more likely to happen.

### expand_events.yaml

Client side fields:

- **name**: Identifier.
  - Multiple events can have the same name. This allows creating multiple configurations.
  - Client side fields always come from the first event. Recommended to not include these in the extra configurations.
- **spawns**: List of spawned objects.
  - See [Expand World Spawns](https://github.com/JereKuusela/valheim-expand_world_spawns/#Configuration) for more info.
  - Usually these have lower spawn times and less restrictions compared to normal spawns.
- **startMessage**: Message shown on the screen during the event.
- **endMessage**: Message shown on the screen when the event ends.
- **forceMusic**: Event music.
- **forceEnvironment**: Event environment/weather.
- **requiredPlayerKeys**: Event becomes available if the player has any of these keys.
- **notRequiredPlayerKeys**: Event is not available if the player has any of these keys.
- **requiredKnownItems**: Event becomes available if the player knows any of these items.
- **notRequiredKnownItems**: Event is not available if the player knows any of these items.
- **radius**: Event area radius.
  - Default value is `96` meters.
- **spawnerDelay**: Delay before the event starts spawning.
  - Default value is `0` seconds.

Server side fields:

- **enabled**: Quick way to disable this entry.
  - Default value is `true`.
- **duration**: How long the event lasts.
  - Default value is `60` seconds.
- **nearBaseOnly**: How many player base items are required within 40 meters.
  - Default value is `3` items.
  - Value `true` counts as 3 items and `false` as 0 items.
- **pauseIfNoPlayerInArea**: The event timer pauses if no player in the area.
  - Default value is `true`.
- **biome**: List of required biomes.
  - Default value is any biome.
- **random**: The event can happen randomly (unlike boss events which happen when near a boss).
  - Default value is `true`.
- **requiredEnvironments**: List of valid environments/weathers.
  - Checked by the server so using `env` command in the client doesn't affect this.
- **requiredGlobalKeys**: Event becomes available if the world has any of these keys.
- **notRequiredGlobalKeys**: Event is not available if the world has any of these keys.
- **playerLimit**: Amount of required players in the area (`min-max`).
  - This can be used to trigger stronger events when more players are nearby.
  - The value is inclusive. For example `1-2` would trigger with 1 and 2 players.
- **playerDistance**: Distance from the triggering player.
  - Default value is `100` meters.
- **eventLimit**: Amount of required events in the area (`min-max`).
  - Requires **Multiple events** to be enabled. The distance is **Minimum distance between events**.
  - Recommended to enable **Check per player**. Otherwise multiple events can't trigger at the same time.
  - This can be used to trigger stronger events when multiple events trigger at the same time.
- **customInterval**: Event specific interval.
  - If set, a separate timer is used for this event.
  - However the event can also trigger from the global interval.
- **customChance**: Event specific chance.

### Player limit / event limit

Recommended to **not** use both because their effects are similar.

Event limit gives more gradual results, but requires multiple events to be enabled.

For example 2 players and 50% event chance causes:

- 0 events: 25% chance
- 1 event: 50% chance
- 2 events: 25% chance

With player limits **1** and **2-100**:

- If event triggers, the limit **2-100** always triggers.
- Third player has no effect (limit **2-100** already always triggers).

With player limits **1-2** and **2-100**:

- If event triggers, both limits have 50% chance to trigger.
- Third player would cause the limit **2-100** alway to trigger.

With event limits **0** and **1-100**:

- If event triggers, 66% chance for limit **0** and 33% chance for limit **1-100**.
- Third player would cause 43% chance for limit **0** and 57% chance for limit **1-100**.
- Fourth player would cause 27% chance for limit **0** and 73% chance for limit **1-100**.
- So even when both options are always possible, more players increase the chance for stronger events.

## Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_events)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
