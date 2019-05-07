using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using MultiMod.Interface;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ItemExchange {

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.mango.ItemExchange", "ItemExchange", "1.0.1")]
    
    public class ItemData {
        public int count;
        public ItemIndex item_index;

        public ItemData(int count, ItemIndex itemindex) {
            this.count = count;
            this.item_index = itemindex;
        }
    }

    public class ItemExchange : ModBehaviour {

        [SerializeField] public GameObject ExchangeWindow;
        [SerializeField] private GameObject PersonalTile;

        private List<ItemData> InvData;
        private List<GameObject> TileList;
        private ItemIcon latestItem;
        private float tile_size;
        private float team_offset_size;
        private float vertical_tile_offset;
        private float horizontal_tile_offset;
        // public List<GameObject> adjust_button;

        public override void OnLoaded(ContentHandler contentHandler) {
            Debug.Log("------ Item Exchange Initiated ------");

            InvData = new List<ItemData>();
            TileList = new List<GameObject>();
            InitUI();            

        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                try {
                    if (Camera.main.aspect >= 2.0) {
                        tile_size = 35f;
                        team_offset_size = 56.2f;
                        vertical_tile_offset = 140.5f;
                        horizontal_tile_offset = -276.3f;
                    } else if (Camera.main.aspect >= 1.7) {
                        tile_size = 49.75f;
                        team_offset_size = 74.75f;
                        vertical_tile_offset = 187;
                        horizontal_tile_offset = -110f;
                    } else {
                        tile_size = 20;
                        vertical_tile_offset = 329;
                    }
                    ResetInv();
                } catch {
                    Debug.Log("no inv");
                }
                ExchangeWindow.SetActive(true);
            } else if (Input.GetKeyUp(KeyCode.Tab)) {
                ExchangeWindow.SetActive(false);
            }

            /*if (Input.GetKeyDown(KeyCode.F11)) {
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.AlienHead);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.AttackSpeedOnCrit);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.AutoCastEquipment);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Bandolier);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Bear);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.BeetleGland);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Behemoth);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.BleedOnHit);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.HealOnCrit);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.HealWhileSafe);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Hoof);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.IceRing);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Icicle);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.IgniteOnKill);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Infusion);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.KillEliteFrenzy);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.LunarDagger);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Medkit);
                LocalUserManager.GetFirstLocalUser().cachedBody.inventory.GiveItem(ItemIndex.Missile);
            }*/
        }

        private void ResetInv(RoR2.Inventory inv = null) {

            foreach(GameObject go in TileList) {
                Destroy(go);
            }
            InvData.Clear();
            TileList.Clear();
            int team_player = 0;
            int current_player_items = 0;
            foreach (var player in PlayerCharacterMasterController.instances) {
                inv = player.master.GetBody().inventory;
                var inv_order = (List<ItemIndex>)inv.itemAcquisitionOrder;
                using (List<ItemIndex>.Enumerator enumerator = (inv_order).GetEnumerator()) {
                    while (enumerator.MoveNext()) {
                        ItemIndex current = enumerator.Current;
                        if(current == ItemIndex.DrizzlePlayerHelper) { continue;  }
                        ItemData itemdata = new ItemData(inv.GetItemCount(current), current);

                        float overflowcounter = (Camera.main.aspect >= 2.0) ? 23 : 11;
                        float tile_size_offset = (inv_order.Count < overflowcounter) ? tile_size : tile_size * ((overflowcounter) / inv_order.Count);
                        float modified_horizontal_tile_offset = horizontal_tile_offset - (tile_size - tile_size_offset)/2;
                        GameObject TileSetup = Instantiate(PersonalTile, ExchangeWindow.transform);
                        TileSetup.GetComponent<RectTransform>().anchoredPosition = new Vector2(tile_size_offset * (current_player_items) + modified_horizontal_tile_offset, vertical_tile_offset - team_player * team_offset_size);
                        int TilePos = TileList.Count;
                        TileSetup.GetComponent<Button>().onClick.AddListener(() => RegisterOffer(TilePos, player.master.GetBody()));

                        InvData.Add(itemdata);
                        TileList.Add(TileSetup);
                        current_player_items++;
                    }
                }
                team_player++;
                current_player_items = 0;
            }
        }

        public void InitUI() {
            // get reference to game's main canvas
            var canvas = RoR2.RoR2Application.instance.mainCanvas;

            /*adjust_button[0].GetComponent<Button>().onClick.AddListener(() => shift_up());
            adjust_button[1].GetComponent<Button>().onClick.AddListener(() => shift_down());
            adjust_button[2].GetComponent<Button>().onClick.AddListener(() => shift_left());
            adjust_button[3].GetComponent<Button>().onClick.AddListener(() => shift_right());*/

            // instantiate UI prefab
            ExchangeWindow.transform.SetParent(canvas.transform, false);

            // set the parent to game's canvas and fix the sizings
            var rect = ExchangeWindow.GetComponent<RectTransform>();
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.anchorMin = new Vector2(0.00f, 0.00f);
            rect.anchorMax = new Vector2(1.00f, 1.00f);

            ExchangeWindow.SetActive(false);
        }

        /*public void shift_up() {
            vertical_tile_offset++;
            Debug.Log(vertical_tile_offset);
        }
        public void shift_down() {
            vertical_tile_offset--;
            Debug.Log(vertical_tile_offset);
        }
        public void shift_left() {
            tile_size -= 0.5f;
            Debug.Log(tile_size);
        }
        public void shift_right() {
            tile_size += 0.5f;
            Debug.Log(tile_size);
        }*/

        public void RegisterOffer(int ItemListLocation, CharacterBody CB) {
            // LocalUser User = LocalUserManager.GetFirstLocalUser();

            // Chat.AddMessage($"Item Loc: {ItemListLocation}, Item List: {InvData.Count}, {InvData[ItemListLocation].item_index}");

            ItemIndex item = InvData[ItemListLocation].item_index;

            Transform UserTransform = CB.transform;
            // Debug.Log(firstLocalUser.userProfile.name);
            CB.inventory.RemoveItem(item, 1);
            float player_rot = 0;
            
            Vector3 mod_rot = new Vector3((float)(Math.Cos(player_rot)) * 10, 20, (float)(Math.Sin(player_rot)) * 10);

            PickupDropletController.CreatePickupDroplet(
                new PickupIndex(item), UserTransform.position, mod_rot);

            string color_tag = "#" + ColorCatalog.GetColorHexString(ItemCatalog.GetItemDef(item).colorIndex);
            // Chat.AddMessage($"{CB.name} has dropped <color={color_tag}> {Language.GetString(ItemCatalog.GetItemDef(item).nameToken)} </color>");
            sendChatMessage($"{CB.GetUserName()} has dropped <color={color_tag}> {Language.GetString(ItemCatalog.GetItemDef(item).nameToken)} </color>");


            InvData[ItemListLocation].count -= 1;
            if(InvData[ItemListLocation].count <= 0) {
                InvData.RemoveAt(ItemListLocation);
                Destroy(TileList[TileList.Count - 1]);
                TileList.RemoveAt(TileList.Count - 1);
            }
        }

        public void sendChatMessage(string message) {
            Chat.SendBroadcastChat((Chat.ChatMessageBase)new Chat.SimpleChatMessage() {
                baseToken = message
            });
        }
    }
}
