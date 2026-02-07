using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    /// <summary>
    /// Prevents food from being placed in boxes instead of fridges
    /// </summary>
    [Info("FridgeFood", "sami37", "1.2.0")]
    [Description("Prevent food from being placed in box instead of fridge.")]
    public class FridgeFood : RustPlugin
    {
        #region Fields

        [PluginReference]
        private Plugin Backpacks;

        // Cached sets for O(1) lookups
        private HashSet<object> _foodSet;
        private HashSet<string> _containerSet;

        // Configuration lists
        private List<object> _listFood;
        private List<object> _listContainer;

        private readonly List<object> _unallowedContainer = new List<object>
        {
            "box.wooden.large",
            "box.wooden",
            "coffin.storage",
            "small.stash",
            "locker",
            "smallbackpack",
            "largebackpack"
        };

        private readonly HashSet<ulong> _adminDebug = new HashSet<ulong>();

        #endregion

        #region Constants

        private const string LangNotAllowed = "NotAllowed";
        private const string LangNotAllowedBackpack = "NotAllowedBackpack";
        private const string LangNoRight = "NoRight";
        private const string LangDisabled = "Disabled";
        private const string LangEnabled = "Enabled";
        private const string LangContainerName = "ContainerName";

        private const BaseEntity.GiveItemReason GiveReason = BaseEntity.GiveItemReason.PickedUp;
        private const int MaxContainerDepth = 10;

        #endregion

        #region Config Functions

        private string ListToString<T>(List<T> list, int first = 0, string separator = ", ") =>
            string.Join(separator, list.Skip(first).Select(val => val.ToString()));

        private void SetConfig(params object[] args)
        {
            List<string> stringArgs = args.Take(args.Length - 1).Select(arg => arg.ToString()).ToList();
            if (Config.Get(stringArgs.ToArray()) == null)
                Config.Set(args);
        }

        private T GetConfig<T>(T defaultVal, params object[] args)
        {
            List<string> stringArgs = args.Select(arg => arg.ToString()).ToList();
            if (Config.Get(stringArgs.ToArray()) == null)
            {
                PrintError($"The plugin failed to read something from the config: {ListToString(stringArgs, 0, "/")}{Environment.NewLine}" +
                          "Please reload the plugin and see if this message is still showing. If so, please post this into the support thread of this plugin.");
                return defaultVal;
            }

            return (T)Convert.ChangeType(Config.Get(stringArgs.ToArray()), typeof(T));
        }

        #endregion

        #region Oxide Hooks

        protected override void LoadDefaultConfig()
        {
            Config.Clear();

            var defaultFoodList = ItemManager.GetItemDefinitions()
                .Where(itemDef => itemDef.category == ItemCategory.Food)
                .Select(itemDef => (object)itemDef.shortname)
                .ToList();

            SetConfig("Food List", defaultFoodList);
            SetConfig("Unallowed container", _unallowedContainer);
            SaveConfig();
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangNotAllowed] = "You are not allowed to put food in normal box.",
                [LangNotAllowedBackpack] = "You are not allowed to put food in backpack.",
                [LangNoRight] = "You don't have permission.",
                [LangDisabled] = "Disabled debug system.",
                [LangEnabled] = "Enabled debug system.",
                [LangContainerName] = "The name of this container is <color=red>{0}</color>."
            }, this);
        }

        private void OnServerInitialized()
        {
            // Load configuration
            _listFood = GetConfig(new List<object>(), "Food List");
            _listContainer = GetConfig(_unallowedContainer, "Unallowed container");

            // Initialize cached sets for fast lookups
            _foodSet = new HashSet<object>(_listFood);
            _containerSet = new HashSet<string>(_listContainer.Select(c => c.ToString()));

            SaveConfig();

            // Check dependencies
            if (Backpacks == null)
            {
                PrintWarning("Backpacks plugin not found - backpack detection will not work.");
            }
        }

        void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            // Check if item is food
            if (!IsForbiddenFood(item))
                return;

            // Check for backpack containers first
            var rootContainer = GetRootContainer(item) ?? container;
            var backpacksOwnerResult = Backpacks?.Call("API_GetBackpackOwnerId", rootContainer);
            if (backpacksOwnerResult is ulong backpackOwner && backpackOwner > 0)
                return;

            // Handle nested containers (backpacks, etc.)
            if (container.parent?.parent != null && container.parent.parent.playerOwner != null)
            {
                HandleNestedContainer(container, item);
                return;
            }

            // Handle regular containers
            if (container.entityOwner != null && container.playerOwner == null)
            {
                HandleRegularContainer(container, item);
            }
        }

        #endregion

        #region Chat Commands

        [ChatCommand("fdebug")]
        private void ChatCommandDebug(BasePlayer player, string cmd, string[] args)
        {
            if (!player.IsAdmin)
            {
                SendReply(player, lang.GetMessage(LangNoRight, this, player.UserIDString));
                return;
            }

            if (_adminDebug.Contains(player.userID))
            {
                _adminDebug.Remove(player.userID);
                SendReply(player, lang.GetMessage(LangDisabled, this, player.UserIDString));
            }
            else
            {
                _adminDebug.Add(player.userID);
                SendReply(player, lang.GetMessage(LangEnabled, this, player.UserIDString));
            }
        }

        #endregion

        #region Helper Methods

        private static BasePlayer GetPlayerFromContainer(ItemContainer container, Item item)
        {
            var owner = item.GetOwnerPlayer();
            if (owner != null)
                return owner;

            return BasePlayer.activePlayerList.FirstOrDefault(p =>
                p.inventory.loot.IsLooting() &&
                p.inventory.loot.entitySource == container.entityOwner);
        }

        private static ItemContainer GetRootContainer(Item item)
        {
            var container = item.parent;
            if (container == null)
                return null;

            int depth = 0;
            while (container.parent?.parent != null &&
                   container.parent != item &&
                   depth < MaxContainerDepth)
            {
                container = container.parent.parent;
                depth++;
            }

            return container;
        }

        private static string NormalizeContainerName(string name)
        {
            return name.Replace("_deployed", "")
                      .Replace(".deployed", "")
                      .Replace("_", ".");
        }

        private bool IsForbiddenFood(Item item)
        {
            return _foodSet != null &&
                   (_foodSet.Contains(item.info.itemid) || _foodSet.Contains(item.info.shortname));
        }

        private bool IsUnallowedContainer(string containerName)
        {
            return _containerSet != null && _containerSet.Contains(containerName);
        }

        private void HandleForbiddenFood(BasePlayer player, Item item, ItemContainer container, string langKey)
        {
            if (player != null)
            {
                SendReply(player, lang.GetMessage(langKey, this, player.UserIDString));
                player.GiveItem(item, GiveReason);
            }
            else
            {
                DropItem(item, container);
            }
        }

        private void DropItem(Item item, ItemContainer container)
        {
            var entityOwner = container.GetEntityOwner();
            if (entityOwner == null)
                return;

            var dropPos = entityOwner.ServerPosition;
            var newDrop = new Vector3(dropPos.x, dropPos.y + 1f, dropPos.z);
            item.Drop(newDrop, Vector3.up);
        }

        private void HandleNestedContainer(ItemContainer container, Item item)
        {
            var player = container.parent?.parent?.playerOwner;
            if (player == null)
                return;

            var containerInfo = container.parent?.info;
            if (containerInfo == null)
                return;

            var containerName = NormalizeContainerName(containerInfo.shortname);

            // Debug output
            if (_adminDebug.Contains(player.userID))
            {
                SendReply(player, string.Format(
                    lang.GetMessage(LangContainerName, this, player.UserIDString),
                    containerName));
            }

            // Check if container is not allowed
            if (IsUnallowedContainer(containerName))
            {
                HandleForbiddenFood(player, item, container, LangNotAllowedBackpack);
            }
        }

        private void HandleRegularContainer(ItemContainer container, Item item)
        {
            if (container.entityOwner == null)
                return;

            var player = GetPlayerFromContainer(container, item);
            var containerName = NormalizeContainerName(container.entityOwner.ShortPrefabName);

            // Debug output
            if (player != null && _adminDebug.Contains(player.userID))
            {
                SendReply(player, string.Format(
                    lang.GetMessage(LangContainerName, this, player.UserIDString),
                    containerName));
            }

            // Check if container is not allowed
            if (IsUnallowedContainer(containerName))
            {
                HandleForbiddenFood(player, item, container, LangNotAllowed);
            }
        }
        #endregion
    }
}
