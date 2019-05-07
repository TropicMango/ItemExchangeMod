using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using RoR2;

namespace ItemExchange {

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.mango.TakeMyChance", "TakeMyChance", "1.0.0")]
    public class TakeMyChace : BaseUnityPlugin {
        public void Awake() {
            On.RoR2.Stage.Start += (orig, self) => {
                
            };
            EntityStateManager.CreateInstance("barrel");
        }
    }
}
