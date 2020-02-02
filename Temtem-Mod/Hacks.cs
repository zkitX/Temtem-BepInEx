using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Temtem.Core;
using Temtem.World;
using Temtem.Players;
using Temtem.Network;
using Temtem.UI;
using Sfs2X;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using BepInEx.Logging;

namespace TemTemMod
{
    public class Hacks
    {
        static public void HealTemtem()
        {
            NetworkLogic.nkqrjhelndm.grcrfnfjqmq();
            TemTemMod.Log("zkitX Temtem Mods", LogLevel.Info, "Temtem healed!");
        }
        static public void OpenBank()
        {
            typeof(UIManager).GetField<UIManager>().dhcnooefhhj();
        }
        static public void OpenShop()
        {
            var allItems = typeof(Temtem.Core.ConfigReader).GetField<Temtem.Core.ConfigReader>().GetField<Temtem.Inventory.AllItemDefinitions>();
            var items = new List<Temtem.Inventory.ItemDefinition>();
            items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("generalItems"));
            items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("medicineItems"));
            items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("captureItems"));
            /* items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("medicineItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("heldItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("courseItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("keyItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("cosmeticItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("emoteItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("tintItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("tintBundleItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("stickerItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("bundleItems"));
             items.AddRange(allItems.GetField<List<Temtem.Inventory.ItemDefinition>>("hardcodedPromoCodeItems"));*/
            var itemList = new Temtem.Inventory.BuyableItemList();
            itemList.ShopItemIds = items.FindAll(i => i != null && i.Price > 0).Select(i => i.Id).ToList();
            Temtem.UI.InGameShopBuyUI.nkqrjhelndm.nnkdhejnfdp(itemList, false);
            typeof(Temtem.UI.UIManager).GetField<Temtem.UI.UIManager>().hrfjkefrolc();
        }
    }
}
