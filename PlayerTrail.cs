using Modding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Satchel.BetterMenus;
using Hkmp.Api.Client;
using Hkmp.Api.Server;

namespace PlayerTrail {
    public class PlayerTrail: Mod, ICustomMenuMod, IGlobalSettings<GlobalSettings> {
        new public string GetName() => "PlayerTrail";
        public override string GetVersion() => "1.0.0.3";
        public static PlayerTrail instance;

        public static GameObject lightseedPrefab;
        public static bool hasPlacedHere = false;
        public static Vector3 lastLocation;
        public static Dictionary<string, Dictionary<float, Vector3>> trail;
        public static Queue<(float, string, Vector3)> delayedTrail;
        public static bool isDelayThreadRunning = false;

        private Menu MenuRef;
        public static GlobalSettings gs = new();

        public static ClientNetManager _netManager;
        public static IClientApi _clientApi;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            instance = this;
            
            ClientAddon.RegisterAddon(new TrailClientAddon());
            ServerAddon.RegisterAddon(new TrailServerAddon());

            lightseedPrefab = preloadedObjects["Crossroads_14"]["Scuttler Spawn 1 (2)/Orange Scuttler (1)"];
            foreach(BoxCollider2D bc2d in lightseedPrefab.GetComponents<BoxCollider2D>()) {
                Component.Destroy(bc2d);
            }
            Component.Destroy(lightseedPrefab.GetComponent<BoxCollider2D>());
            Component.Destroy(lightseedPrefab.GetComponent<AudioSource>());
            Component.Destroy(lightseedPrefab.GetComponent<EnemyDreamnailReaction>());
            Component.Destroy(lightseedPrefab.GetComponent<CollisionEnterEvent>());
            Component.Destroy(lightseedPrefab.GetComponent<ScuttlerControl>());
            Component.Destroy(lightseedPrefab.GetComponent<Rigidbody2D>());
            Component.Destroy(lightseedPrefab.GetComponent<tk2dSpriteAnimator>());

            trail = new();
            delayedTrail = new();
        }

        public override List<(string, string)> GetPreloadNames() {
            return new List<(string, string)> {
                ("Crossroads_14", "Scuttler Spawn 1 (2)/Orange Scuttler (1)")
            };
        }

        public void earlySceneLoad(Scene arg0, Scene arg1) {
            if(gs.canLayTrail) {
                hasPlacedHere = false;
            }
        }

        public void lateSceneLoad(On.GameManager.orig_OnNextLevelReady orig, GameManager self) {
            orig(self);
            string sceneName = self.sceneName;
            if(gs.canSeeTrail) {
                if(trail.ContainsKey(sceneName)) {
                    foreach(float key in trail[sceneName].Keys) {
                        if(Time.time - key < gs.trailLinger) {
                            GameObject.Instantiate(lightseedPrefab, trail[sceneName][key], Quaternion.identity).SetActive(true);
                        }
                    }
                }
            }
            if(_clientApi.NetClient.IsConnected) {
                _netManager.SendCleanup(sceneName);
            }
            else {
                Task.Run(() => cleanupTrailSeeds(sceneName));
            }
        }

        public void HeroUpdate() {
            if(gs.canLayTrail) {
                if(!hasPlacedHere || getDistance() > gs.trailDistance) {
                    Vector3 position = HeroController.instance.transform.position;
                    if(_clientApi.NetClient.IsConnected) {
                        _netManager.SendTrail(GameManager.instance.sceneName, position.x, position.y);
                    }
                    else {
                        createNewTrailSeed(GameManager.instance.sceneName, position.x, position.y);
                    }
                    lastLocation = position;
                    hasPlacedHere = true;
                }
            }
        }

        public void createNewTrailSeed(string scene, float positionX, float positionY) {
            if(gs.canSeeTrail) {
                delayedTrail.Enqueue((Time.time, scene, new Vector3(positionX, positionY, 0.0067f)));
                if(!isDelayThreadRunning) {
                    delayTrailSeeds();
                }
            }
        }

        public void cleanupTrailSeeds(string scene) {
            if(trail.ContainsKey(scene)) {
                List<float> keysToRemove = new();
                foreach(float key in trail[scene].Keys) {
                    if(Time.time - key >= gs.trailLinger) {
                        keysToRemove.Add(key);
                    }
                }
                foreach(float key in keysToRemove) {
                    trail[scene].Remove(key);
                }
            }
        }

        private async void delayTrailSeeds() {
            isDelayThreadRunning = true;
            while(true) {
                if(GameManager.instance.IsNonGameplayScene() || delayedTrail.Count < 1) {
                    isDelayThreadRunning = false;
                    break;
                }
                while(Time.time - (delayedTrail.Peek().Item1 + gs.trailDelay) < 0) {
                    await Task.Yield();
                }
                (float, string, Vector3) newSeed = delayedTrail.Dequeue();
                float time = newSeed.Item1;
                string scene = newSeed.Item2;
                Vector3 position = newSeed.Item3;
                if(!trail.ContainsKey(scene)) {
                    trail.Add(scene, new());
                }
                while(trail[scene].ContainsKey(time)) {
                    time += 0.00001f;
                }
                trail[scene].Add(time, position);
                if(GameManager.instance.sceneName == scene) {
                    GameObject.Instantiate(lightseedPrefab, position, Quaternion.identity).SetActive(true);
                }
            }
        }

        private double getDistance() {
            Vector3 knight = HeroController.instance.transform.position;
            return Math.Pow(lastLocation.x - knight.x, 2) + Math.Pow(lastLocation.y - knight.y, 2);
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates) {
            MenuRef ??= new Menu(
                name: "PlayerTrail",
                elements: new Element[]
                {
                    new HorizontalOption(
                        name: "Lay Trail",
                        description: "",
                        values: new string[] {"On", "Off"},
                        applySetting: index =>
                        {
                            gs.canLayTrail = index == 0;
                        },
                        loadSetting: () => gs.canLayTrail ? 0 : 1),
                    new CustomSlider(
                        name: "Trail Interval",
                        storeValue: val => {
                            gs.trailDistance = (int)(val*val);
                        },
                        loadValue : () => (int)Mathf.Sqrt(gs.trailDistance),
                        minValue: 1,
                        maxValue: 10,
                        wholeNumbers: true
                        ),
                    new TextPanel(""),
                    new TextPanel(""),
                    new HorizontalOption(
                        name: "See Trail",
                        description: "",
                        values: new string[] {"On", "Off"},
                        applySetting: index =>
                        {
                            gs.canSeeTrail = index == 0;
                        },
                        loadSetting: () => gs.canSeeTrail ? 0 : 1),
                    new CustomSlider(
                        name: "Delay Before Appearing",
                        storeValue: val => {
                            gs.trailDelay = (int)val;
                        },
                        loadValue: () => gs.trailDelay,
                        minValue: 0,
                        maxValue: 120,
                        wholeNumbers: true
                        ),
                    new CustomSlider(
                        name: "Linger Duration",
                        storeValue: val => {
                            gs.trailLinger = (int)val;
                        },
                        loadValue: () => gs.trailLinger,
                        minValue: 10,
                        maxValue: 600,
                        wholeNumbers: true
                        ),
                    new TextPanel(
                        name: "Trail appears in realtime but disappears on room load",
                        fontSize: 28
                        )
                }
            );

            return MenuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu {
            get;
        }

        public void OnLoadGlobal(GlobalSettings s) {
            gs = s;
        }

        public GlobalSettings OnSaveGlobal() {
            return gs;
        }
    }

    public class GlobalSettings {
        public bool canLayTrail;
        public bool canSeeTrail;
        public int trailDistance = 9;
        public int trailDelay = 30;
        public int trailLinger = 300;
    }
}