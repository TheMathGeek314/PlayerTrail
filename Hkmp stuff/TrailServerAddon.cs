using Hkmp.Api.Server;

namespace PlayerTrail {
    public class TrailServerAddon: ServerAddon {
        protected override string Name => "PlayerTrailAddon";
        protected override string Version => "1.0.0.1";
        public override bool NeedsNetwork => true;

        public override void Initialize(IServerApi serverApi) {
            new ServerTrailManager(this, serverApi).Initialize();
        }
    }
}
