using System;

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
        private long _millisecondsSinceLastPacket;
        private Vector2 _difference;
        private int _fadeDirection;
        private float _fadeFraction;

        private float[] _previousCommsLagTimes;
        private int _earliestCommsLagSlot;
        private float _averageCommsLagTime;

        private Vector2 _targetPosition;
        private bool _shouldBeAhead;
        private Vector2 _playerPositionLastUpdate;
        private int _playerMovementDirection;
        private float _boundaryLine;
        private int _playerTimeAtBoundary;
        private int _peerTimeAtBoundary;

        public InterDeviceCommunicator Communicator { private get; set; }
        public IMotionEngine MotionEngine { get { return null; } }
        public AdditiveLayerParticleEffectManager ParticleManager { private get; set; }

        public RaceOpponent()
            : base()
        {
            _previousCommsLagTimes = new float[Comms_Sample_Count];

            RenderLayer = 2;
            WorldPositionIsFixed = false;
            Scale = 0.9f;
            RenderDepth = 0.495f;
        }

        public override void Reset()
        {
            _difference = Vector2.Zero;
            _fadeDirection = 1;
            _fadeFraction = 0.0f;

            _shouldBeAhead = false;
            _millisecondsSinceLastPacket = 0;
            _playerPositionLastUpdate = Vector2.Zero;
            _boundaryLine = -1.0f;
            _playerTimeAtBoundary = 0;
            _peerTimeAtBoundary = 0;

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

            ResetAverageLagTime();
        }

        private StorableBone CreateCloudBone()
        {
            StorableBone cloud = new StorableBone();
            cloud.ID = "cloud";
            cloud.RelativePosition = new Vector2(0.0f, 30.0f);
            cloud.RelativeDepth = -0.1f;

            return cloud;
        }

        private void ResetAverageLagTime()
        {
            for (int i=0; i<Comms_Sample_Count; i++)
            {
                _previousCommsLagTimes[i] = InterDeviceCommunicator.Default_Send_Interval;
            }

            _earliestCommsLagSlot = 0;
            _averageCommsLagTime = InterDeviceCommunicator.Default_Send_Interval;
        }

        public void SetForRaceStart(Vector2 startPosition, bool startFacingLeft)
        {
            WorldPosition = startPosition;
            _playerMovementDirection = startFacingLeft ? 1 : -1;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (Communicator.MillisecondsSinceLastReceive < _millisecondsSinceLastPacket)
            {
                HandlePacketReceived();
            }

            _millisecondsSinceLastPacket = Communicator.MillisecondsSinceLastReceive;
            _playerMovementDirection = Math.Sign(Communicator.OwnPlayerData.PlayerWorldPosition.X - _playerPositionLastUpdate.X);

            UpdateDisplaySettings();

            _playerPositionLastUpdate = Communicator.OwnPlayerData.PlayerWorldPosition;
        }

        private void HandlePacketReceived()
        {
            _previousCommsLagTimes[_earliestCommsLagSlot] = _millisecondsSinceLastPacket;
            _earliestCommsLagSlot = (_earliestCommsLagSlot + 1) % Comms_Sample_Count;

            _averageCommsLagTime = 0;
            for (int i=0; i<Comms_Sample_Count; i++)
            {
                _averageCommsLagTime += _previousCommsLagTimes[i];
            }
            _averageCommsLagTime /= Comms_Sample_Count;
        }

        private void UpdateDisplaySettings()
        {
            if (Communicator.MillisecondsSinceLastReceive > InterDeviceCommunicator.Signal_Deterioration_Threshold)
            {
                SetForDegradedSignal();
            }
            else
            {
                SetForGoodSignal();
            }

            UpdateTint();
        }

        private void SetForDegradedSignal()
        {
            _fadeDirection = -1;
            WorldPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
        }

        private void SetForGoodSignal()
        {
            _fadeDirection = 1;

            if (Vector2.DistanceSquared(Communicator.OwnPlayerData.PlayerWorldPosition, Communicator.OtherPlayerData.PlayerWorldPosition) < 
                Position_Lock_Distance * Position_Lock_Distance)
            {
                SetPositionRelativeToClient();
            }
            else
            {
                SetPositionFromCommsData();
            }

            WorldPosition = _targetPosition;
        }

        private void SetPositionRelativeToClient()
        {
            _targetPosition = new Vector2(Communicator.OwnPlayerData.PlayerWorldPosition.X, _playerPositionLastUpdate.Y);

            CheckAndHandleBoundaryPass();
            if (Math.Abs(Communicator.OwnPlayerData.PlayerWorldPosition.X - _boundaryLine) > Position_Lock_Distance)
            {
                SetNewBoundary();
            }

            if (_shouldBeAhead)
            {
                _targetPosition.X += Position_Offset_Distance * _playerMovementDirection;
            }
            else
            {
                _targetPosition.X -= Position_Offset_Distance * _playerMovementDirection;
            }
        }

        private void SetNewBoundary()
        {
            _boundaryLine = Communicator.OwnPlayerData.PlayerWorldPosition.X + (Definitions.Grid_Cell_Pixel_Size * _playerMovementDirection);
            _playerTimeAtBoundary = 0;
            _peerTimeAtBoundary = 0;
        }

        private void CheckAndHandleBoundaryPass()
        {
            if (_boundaryLine > -1.0f)
            {
                if (_playerTimeAtBoundary == 0)
                {
                    if ((_playerMovementDirection > 0) && (Communicator.OwnPlayerData.PlayerWorldPosition.X > _boundaryLine))
                    {
                        _playerTimeAtBoundary = Communicator.OwnPlayerData.TotalRaceTimeElapsedInMilliseconds;
                    }
                    else if ((_playerMovementDirection < 0) && (Communicator.OwnPlayerData.PlayerWorldPosition.X < _boundaryLine))
                    {
                        _playerTimeAtBoundary = Communicator.OwnPlayerData.TotalRaceTimeElapsedInMilliseconds;
                    }
                }

                if (_peerTimeAtBoundary == 0)
                {
                    if ((_playerMovementDirection > 0) && (Communicator.OtherPlayerData.PlayerWorldPosition.X > _boundaryLine))
                    {
                        _peerTimeAtBoundary = Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds;
                    }
                    else if ((_playerMovementDirection < 0) && (Communicator.OtherPlayerData.PlayerWorldPosition.X < _boundaryLine))
                    {
                        _peerTimeAtBoundary = Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds;
                    }
                }

                if ((_playerTimeAtBoundary > 0) && (_peerTimeAtBoundary > 0))
                {
                    _shouldBeAhead = (_peerTimeAtBoundary < _playerTimeAtBoundary);
                    _boundaryLine = -1.0f;
                }
            }
        }

        private void SetPositionFromCommsData()
        {
            _targetPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
        }

        private void UpdateTint()
        {
            _fadeFraction = MathHelper.Clamp(_fadeFraction + (Fade_Step * _fadeDirection), 0.2f, 0.8f);
            Tint = Color.Lerp(Color.Transparent, Color.White, _fadeFraction); 
        }

        private const float Fade_Step = 0.01f;
        private const float Position_Lock_Distance = 250.0f;
        private const float Position_Offset_Distance = 60.0f;
        private const int Comms_Sample_Count = 20;
    }
}
