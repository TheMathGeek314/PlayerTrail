using Hkmp.Api.Client;

namespace PlayerTrail {
    public class TrailClientAddon: TogglableClientAddon {
        public override bool NeedsNetwork => true;
        protected override string Name => "PlayerTrailAddon";
        protected override string Version => "1.0.0.3";

        private ClientTrailManager _trailManager;

        public override void Initialize(IClientApi clientApi) {
            _trailManager = new ClientTrailManager(this, clientApi);
            _trailManager.Initialize();
        }

        protected override void OnEnable() {
            _trailManager.Enable();
        }

        protected override void OnDisable() {
            _trailManager.Disable();
        }
    }
}
