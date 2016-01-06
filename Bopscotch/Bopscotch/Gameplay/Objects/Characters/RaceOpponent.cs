using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Animation.Skeletons;

using Bopscotch.Communication;
using Bopscotch.Data.Avatar;
using Bopscotch.Effects.Particles;

namespace Bopscotch.Gameplay.Objects.Characters
{
    public class RaceOpponent : DisposableSkeleton, ICameraRelative, IMobile
    {
        private long _millisecondsSinceLastComms;
        private Vector2 _velocity;
        private Vector2 _physicalPosition;
        private Vector2 _difference;

        public InterDeviceCommunicator Communicator { private get; set; }
        public IMotionEngine MotionEngine { get { return null; } }
        public AdditiveLayerParticleEffectManager ParticleManager { private get; set; }


        public RaceOpponent()
            : base()
        {
            RenderLayer = 2;
            WorldPositionIsFixed = false;
            Scale = 0.9f;
            RenderDepth = 0.495f;
        }

        public override void Reset()
        {
            _millisecondsSinceLastComms = 0;
            _velocity = Vector2.Zero;
            _physicalPosition = Vector2.Zero;
            _difference = Vector2.Zero;

            WorldPosition = Vector2.Zero;
            Visible = true;

            if (Bones.Count < 1)
            {
                CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Side);
                AddBone(CreateCloudBone(), "body");
            }

            SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Communicator.OtherPlayerAvatarSlot));
            Rectangle bounds = TextureManager.Textures["race-p2-cloud"].Bounds;
            Bones["cloud"].ApplySkin("race-p2-cloud", new Vector2(bounds.Width, bounds.Height) * 0.5f, bounds);
        }

        private StorableBone CreateCloudBone()
        {
            StorableBone cloud = new StorableBone();
            cloud.ID = "cloud";
            cloud.RelativePosition = new Vector2(0.0f, 30.0f);
            cloud.RelativeDepth = -0.1f;

            return cloud;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            WorldPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
        }
    }
}
