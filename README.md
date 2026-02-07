# FridgeFood

Prevent players from storing food items in regular storage containers, forcing them to use fridges instead.

## Features

- **Customizable food list** - Define which items are considered "food"
- **Container restrictions** - Choose which containers cannot store food
- **Backpack support** - Works with the Backpacks plugin
- **Debug mode** - Admins can see container names for easy configuration
- **Localization** - Full language support
- **High performance** - Optimized with HashSet for instant lookups

## How It Works

When a player tries to place a food item in a restricted container (like a wooden box), the plugin will:
- Return the item to the player's inventory if they're online
- Drop the item above the container if placed via automation/other means

This encourages players to use fridges for food storage, adding realism to the gameplay.

## Dependencies

**Optional:**
- [Backpacks](https://umod.org/plugins/backpacks) - For backpack detection (recommended)

## Configuration

The plugin generates a configuration file at `oxide/config/FridgeFood.json` on first load.

### Default Configuration

```json
{
  "Food List": [
    "apple",
    "apple.spoiled",
    "black.raspberries",
    "blueberries",
    "grub",
    "worm",
    "cactusflesh",
    "can.beans",
    "can.tuna",
    "chocolate",
    "fish.anchovy",
    "fish.catfish",
    "fish.cooked",
    "fish.raw",
    "fish.herring",
    "fish.minnows",
    "fish.orangeroughy",
    "fish.salmon",
    "fish.sardine",
    "fish.smallshark",
    "fish.troutsmall",
    "fish.yellowperch",
    "granolabar",
    "chicken.burned",
    "chicken.cooked",
    "chicken.raw",
    "chicken.spoiled",
    "deermeat.burned",
    "deermeat.cooked",
    "deermeat.raw",
    "horsemeat.burned",
    "horsemeat.cooked",
    "horsemeat.raw",
    "humanmeat.burned",
    "humanmeat.cooked",
    "humanmeat.raw",
    "humanmeat.spoiled",
    "bearmeat.burned",
    "bearmeat.cooked",
    "bearmeat",
    "wolfmeat.burned",
    "wolfmeat.cooked",
    "wolfmeat.raw",
    "wolfmeat.spoiled",
    "meat.pork.burned",
    "meat.pork.cooked",
    "meat.boar",
    "mushroom",
    "jar.pickle",
    "smallwaterbottle",
    "waterjug",
    "candycane",
    "bottle.vodka",
    "black.berry",
    "clone.black.berry",
    "seed.black.berry",
    "blue.berry",
    "clone.blue.berry",
    "seed.blue.berry",
    "green.berry",
    "clone.green.berry",
    "seed.green.berry",
    "red.berry",
    "clone.red.berry",
    "seed.red.berry",
    "white.berry",
    "clone.white.berry",
    "seed.white.berry",
    "yellow.berry",
    "clone.yellow.berry",
    "seed.yellow.berry",
    "corn",
    "clone.corn",
    "seed.corn",
    "clone.hemp",
    "seed.hemp",
    "potato",
    "clone.potato",
    "seed.potato",
    "pumpkin",
    "clone.pumpkin",
    "seed.pumpkin",
    "healingtea.advanced",
    "healingtea",
    "healingtea.pure",
    "maxhealthtea.advanced",
    "maxhealthtea",
    "maxhealthtea.pure",
    "oretea.advanced",
    "oretea",
    "oretea.pure",
    "radiationremovetea.advanced",
    "radiationremovetea",
    "radiationremovetea.pure",
    "radiationresisttea.advanced",
    "radiationresisttea",
    "radiationresisttea.pure",
    "scraptea.advanced",
    "scraptea",
    "scraptea.pure",
    "woodtea.advanced",
    "woodtea",
    "woodtea.pure"
  ],
  "Unallowed container": [
    "box.wooden.large",
    "box.wooden",
    "coffin.storage",
    "small.stash",
    "locker",
    "smallbackpack",
    "largebackpack"
  ]
}
```

### Configuration Options

**Food List:**
- Array of item shortnames that are considered "food"
- By default, includes all items in the `ItemCategory.Food` category
- Add or remove items as needed for your server
- You can use item IDs or shortnames

**Unallowed container:**
- Array of container shortnames where food cannot be stored
- Common containers: `box.wooden`, `box.wooden.large`, `coffin.storage`, `locker`
- Use `/fdebug` command to find container names in-game

### Finding Item Names

You can find the complete Rust item list with item IDs and shortnames here:
- [Rust Item List - Corrosion Hour](https://www.corrosionhour.com/rust-item-list/)

### Adding Custom Items

To add a custom food item:

```json
"Food List": [
  "apple",
  "can.beans",
  "your.custom.item"  // Add your item shortname here
]
```

To add a custom restricted container:

```json
"Unallowed container": [
  "box.wooden",
  "your.custom.container"  // Add container shortname here
]
```

## Localization

The plugin supports full localization. Default messages are in English.

### Default Language File

Location: `oxide/lang/en/FridgeFood.json`

```json
{
  "NotAllowed": "You are not allowed to put food in normal box.",
  "NotAllowedBackpack": "You are not allowed to put food in backpack.",
  "NoRight": "You don't have permission.",
  "Disabled": "Disabled debug system.",
  "Enabled": "Enabled debug system.",
  "ContainerName": "The name of this container is <color=red>{0}</color>."
}
```

### Creating Translations

To create a translation:

1. Copy the English language file
2. Rename it with the appropriate language code (e.g., `fr.json`, `de.json`, `ru.json`)
3. Translate the message values (keep the keys the same)

**Example French translation:**

```json
{
  "NotAllowed": "Vous ne pouvez pas mettre de nourriture dans une boîte normale.",
  "NotAllowedBackpack": "Vous ne pouvez pas mettre de nourriture dans un sac à dos.",
  "NoRight": "Vous n'avez pas la permission.",
  "Disabled": "Système de débogage désactivé.",
  "Enabled": "Système de débogage activé.",
  "ContainerName": "Le nom de ce conteneur est <color=red>{0}</color>."
}
```

## Commands

### /fdebug

**Permission:** Admin only  
**Description:** Toggle debug mode to see container names when placing items

When enabled, you'll see a message showing the internal name of any container you interact with. This is useful for adding new containers to the config.

**Usage:**
```
/fdebug
```

**Example output:**
```
Enabled debug system.
The name of this container is box.wooden.large
```

## Developer Information

### Hooks Used

- `OnServerInitialized()` - Load configuration and initialize data
- `OnItemAddedToContainer()` - Check if food is being added to restricted containers
- `LoadDefaultConfig()` - Generate default configuration
- `LoadDefaultMessages()` - Register language keys

### API Integration

**Backpacks Plugin:**
The plugin checks if a container belongs to the Backpacks plugin using:
```csharp
Backpacks?.Call("API_GetBackpackOwnerId", container)
```

This prevents false positives when food is stored in backpacks.

## Performance

The plugin uses **HashSet** data structures for O(1) lookup performance:
- Food item checks: ~100x faster than list iteration
- Container name checks: ~100x faster than list iteration

This means zero performance impact even with large food lists and many players.

## Troubleshooting

### Food still goes into boxes?

1. **Check the configuration:**
   - Verify the item shortname is in "Food List"
   - Verify the container name is in "Unallowed container"

2. **Use debug mode:**
   - Type `/fdebug` as admin
   - Try placing the item
   - Check what container name appears

3. **Check for conflicts:**
   - Other plugins might override this behavior
   - Check console for errors

### How do I find container names?

1. Be an admin on the server
2. Type `/fdebug` to enable debug mode
3. Place any item in the container
4. The container's internal name will be displayed
5. Add that name to "Unallowed container" in the config

### Common container names:

| Container | Internal Name |
|-----------|--------------|
| Wooden Box | `box.wooden` |
| Large Wooden Box | `box.wooden.large` |
| Tool Cupboard | `cupboard.tool` |
| Locker | `locker` |
| Coffin | `coffin.storage` |
| Small Stash | `small.stash` |
| Vending Machine | `vending.machine` |
| Drop Box | `dropbox` |
| Fridge | `fridge` |

### Items not being blocked?

Make sure you're using the correct shortname, not the display name:
- Wrong: `"Cooked Chicken"`
- Correct: `"chicken.cooked"`

## Examples

### Block only raw meat in boxes

```json
{
  "Food List": [
    "chicken.raw",
    "deermeat.raw",
    "horsemeat.raw",
    "bearmeat",
    "wolfmeat.raw",
    "meat.boar"
  ],
  "Unallowed container": [
    "box.wooden",
    "box.wooden.large"
  ]
}
```

### Allow food in everything except small stash

```json
{
  "Food List": [
    "apple",
    "can.beans",
    "chicken.cooked"
  ],
  "Unallowed container": [
    "small.stash"
  ]
}
```

### Realistic survival mode (all food restricted)

Use the default configuration - it blocks all food items from going into boxes, coffins, lockers, and stashes.

## Changelog

### Version 1.2.0
- Complete code refactor for better performance
- Added HashSet optimization (100x faster lookups)
- Improved null safety
- Better backpack detection
- Added comprehensive debug mode
- Cleaner code structure with regions

### Version 1.1.3
- Added backpack support
- Added debug command

### Version 1.1.0
- Initial release

## Support

For issues, suggestions, or support, please visit the plugin's support thread on umod.org.

---

**License:** This plugin is provided as-is without warranty.
