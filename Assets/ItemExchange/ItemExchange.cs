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
    [BepInPlugin("com.mango.ItemExchange", "ItemExchange", "0.0.1")]
    
    public class ItemData {
        public int count;
        public ItemIndex item_index;

        public ItemData(int count, ItemIndex itemindex) {
            this.count = count;
            this.item_index = itemindex;
        }
    }

    public class ItemExchange : ModBehaviour {

        [SerializeField] private GameObject ExchangeWindow;
        [SerializeField] private GameObject PersonalTile;
        //private Dictionary<ItemIndex, int> CurrentInv;
        //private List<GameObject> ItemTileList;
        //private List<ItemIndex> TileItemIndex;
        private List<ItemData> InvData;
        private List<GameObject> TileList;
        private GameObject gobj;
        private ItemIcon latestItem; 

        public override void OnLoaded(ContentHandler contentHandler) {
            Debug.Log("------ Item Exchange Initiated ------");

            InvData = new List<ItemData>();
            TileList = new List<GameObject>();
            InitUI();

            On.RoR2.GenericPickupController.GrantItem += (orig, self, body, inventory) => {
                orig(self, body, inventory);

                var item = self.pickupIndex.itemIndex;

                int ItemDataIndex = InvData.FindIndex(x => x.item_index == item);
                Debug.Log($"index: {ItemDataIndex}");
                if (ItemDataIndex != -1) {
                    InvData[ItemDataIndex].count += 1;
                } else {
                    GameObject TileSetup = Instantiate(PersonalTile, gobj.transform);
                    TileSetup.GetComponent<RectTransform>().anchoredPosition = new Vector2(-608f + 47.25f * (TileList.Count), 329);
                    int TilePos = TileList.Count;
                    Debug.Log($"position: {TilePos}");
                    TileSetup.GetComponent<Button>().onClick.AddListener(() => RegisterOffer(TilePos));

                    // Chat.AddMessage($"current list size: {TileList.Count}");

                    InvData.Add(new ItemData(1, item));
                    TileList.Add(TileSetup);
                }
            };


        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                gobj.SetActive(true);
            } else if (Input.GetKeyUp(KeyCode.Tab)) {
                gobj.SetActive(false);
            }
        }

        public void InitUI() {
            // get reference to game's main canvas
            var canvas = RoR2.RoR2Application.instance.mainCanvas;

            // instantiate UI prefab
            gobj = Instantiate(ExchangeWindow);
            gobj.transform.SetParent(canvas.transform, false);

            // set the parent to game's canvas and fix the sizings
            var rect = gobj.GetComponent<RectTransform>();
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.anchorMin = new Vector2(0.00f, 0.00f);
            rect.anchorMax = new Vector2(1.00f, 1.00f);

            gobj.SetActive(false);
        }

        public void RegisterOffer(int ItemListLocation) {
            LocalUser User = LocalUserManager.GetFirstLocalUser();

            //Chat.AddMessage($"Item Loc: {ItemListLocation}, Item List: {InvData.Count}, {InvData[ItemListLocation].item_index}");

            ItemIndex item = InvData[ItemListLocation].item_index;

            Transform UserTransform = User.cachedBody.transform;
            // Debug.Log(firstLocalUser.userProfile.name);
            User.cachedBody.inventory.RemoveItem(item, 1);
            float player_rot = UserTransform.rotation.eulerAngles.y;
            
            Vector3 mod_rot = new Vector3((float)(Math.Cos(player_rot)) * 10, 20, (float)(Math.Sin(player_rot)) * 10);

            PickupDropletController.CreatePickupDroplet(
                new PickupIndex(item), UserTransform.position, mod_rot);
            

            string color_tag = "#" + ColorCatalog.GetColorHexString(ItemCatalog.GetItemDef(item).colorIndex);
            Debug.Log(color_tag);
            Chat.AddMessage($"{User.userProfile.name} has dropped <color={color_tag}> {Language.GetString(ItemCatalog.GetItemDef(item).nameToken)} </color>");

            InvData[ItemListLocation].count -= 1;
            if(InvData[ItemListLocation].count <= 0) {
                InvData.RemoveAt(ItemListLocation);
                Destroy(TileList[TileList.Count - 1]);
                TileList.RemoveAt(TileList.Count - 1);
            }
        }
    }
}
