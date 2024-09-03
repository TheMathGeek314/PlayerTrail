using Modding;
using Hkmp.Api.Client;
using System.Threading.Tasks;

namespace PlayerTrail {
    public class ClientTrailManager {

        public ClientTrailManager(TrailClientAddon addon, IClientApi clientApi) {
            PlayerTrail._netManager = new ClientNetManager(addon, clientApi.NetClient);
            PlayerTrail._clientApi = clientApi;
        }

        public void Initialize() {
            Enable();
            PlayerTrail._netManager.TrailEvent += OnTrail;
            PlayerTrail._netManager.CleanEvent += OnClean;
        }

        public void Enable() {
            ModHooks.HeroUpdateHook += PlayerTrail.instance.HeroUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += PlayerTrail.instance.earlySceneLoad;
            On.GameManager.OnNextLevelReady += PlayerTrail.instance.lateSceneLoad;
        }

        public void Disable() {
            ModHooks.HeroUpdateHook -= PlayerTrail.instance.HeroUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= PlayerTrail.instance.earlySceneLoad;
            On.GameManager.OnNextLevelReady -= PlayerTrail.instance.lateSceneLoad;
        }

        private void OnTrail(TrailPacket packet) {
            PlayerTrail.instance.createNewTrailSeed(packet.scene, packet.position);
        }

        private void OnClean(CleanPacket packet) {
            Task.Run(() => PlayerTrail.instance.cleanupTrailSeeds(packet.scene));
        }
    }
}
