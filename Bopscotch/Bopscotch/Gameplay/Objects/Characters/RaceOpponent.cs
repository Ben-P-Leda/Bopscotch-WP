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

        private Vector2 _velocity;

        private float[] _previousCommsLagTimes;
        private int _earliestCommsLagSlot;
        private float _averageCommsLagTime;
        private float _relativePositionOffset;

        private bool _shouldBeAhead;
        private Vector2 _playerPositionLastUpdate;
        private int _playerMovementDirection;
        private float _boundaryLine;
        private int _playerTimeAtBoundary;
        private int _peerTimeAtBoundary;

        private Vector2 _lastPeerPosition;
        private int _packetsAtLastPosition;
        private bool _awaitingMovementStart;
        private bool _raceInProgress;

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
            _relativePositionOffset = 0;

            _lastPeerPosition = Vector2.Zero;
            _packetsAtLastPosition = 0;
            _raceInProgress = false;
            _awaitingMovementStart = true;

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
            Mirror = startFacingLeft;
            WorldPosition = startPosition;

            _playerMovementDirection = startFacingLeft ? 1 : -1;
            _awaitingMovementStart = true;
            _velocity = Vector2.Zero;
            _lastPeerPosition = WorldPosition;
        }

        public void StartRace()
        {
            _raceInProgress = true;
            _awaitingMovementStart = false;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (Communicator.MillisecondsSinceLastReceive < _millisecondsSinceLastPacket)
            {
                HandlePacketReceived();
            }

            _millisecondsSinceLastPacket = Communicator.MillisecondsSinceLastReceive;
            _playerMovementDirection = Math.Sign(Communicator.OwnPlayerData.PlayerWorldPosition.X - _playerPositionLastUpdate.X);

            UpdateDisplaySettings(millisecondsSinceLastUpdate);

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

            if (Communicator.OtherPlayerData.PlayerWorldPosition == _lastPeerPosition)
            {
                _packetsAtLastPosition++;
                if ((_raceInProgress) && (_packetsAtLastPosition > 0) && (Visible))
                {
                    ParticleManager.LaunchCloudBurst(this);
                    Visible = false;
                    _velocity = Vector2.Zero;
                    _awaitingMovementStart = true;
                }
            }
            else
            {
                if (!Visible)
                {
                    WorldPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                    Visible = true;
                    _fadeFraction = 0.0f;
                }
                else if (_lastPeerPosition.X != Communicator.OtherPlayerData.PlayerWorldPosition.X)
                {
                    _awaitingMovementStart = false;
                }

                _lastPeerPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _packetsAtLastPosition = 0;
            }
        }

        private void UpdateDisplaySettings(int millisecondsSinceLastUpdate)
        {
            if (Communicator.MillisecondsSinceLastReceive > InterDeviceCommunicator.Signal_Deterioration_Threshold)
            {
                SetForDegradedSignal();
            }
            else
            {
                SetForGoodSignal(millisecondsSinceLastUpdate);
            }

            if (_velocity.X != 0.0f)
            {
                Mirror = _velocity.X < 0.0f;
            }

            _fadeFraction = MathHelper.Clamp(_fadeFraction + (Fade_Step * _fadeDirection), 0.2f, 0.8f);
            Tint = Color.Lerp(Color.Transparent, Color.White, _fadeFraction); 
        }

        private void SetForDegradedSignal()
        {
            _fadeDirection = -1;
            _velocity = Vector2.Zero;
            WorldPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
        }

        private void SetForGoodSignal(int millisecondsSinceLastUpdate)
        {
            _fadeDirection = 1;

            if (PeerShouldSyncToPlayer())
            {
                SetVelocityFromPlayer();
            }
            else
            {
                SetVelocityFromCommsData();
            }

            WorldPosition += _velocity * millisecondsSinceLastUpdate;
        }

        private string _desyncMessage = "";

        private bool PeerShouldSyncToPlayer()
        {
            bool shouldSync = true;
            _desyncMessage = "";

            double plRow = Math.Floor(Communicator.OwnPlayerData.PlayerWorldPosition.Y / Definitions.Grid_Cell_Pixel_Size);
            double peRow = Math.Floor(Communicator.OtherPlayerData.PlayerWorldPosition.Y / Definitions.Grid_Cell_Pixel_Size);

            _desyncMessage = string.Format("{0}/{1}", plRow, peRow);

            if ((_awaitingMovementStart) || (_playerMovementDirection == 0))
            {
                shouldSync = false;
            }
            else
            {
                float distanceToCurrent = Vector2.DistanceSquared(Communicator.OwnPlayerData.PlayerWorldPosition, 
                    Communicator.OtherPlayerData.PlayerWorldPosition);
                float distanceToUpdated = Vector2.DistanceSquared(Communicator.OwnPlayerData.PlayerWorldPosition,
                    Communicator.OtherPlayerData.PlayerWorldPosition + _velocity);

                if ((distanceToCurrent >= Position_Lock_Distance * Position_Lock_Distance) && 
                    (distanceToUpdated >= Position_Lock_Distance * Position_Lock_Distance))
                {
                    shouldSync = false;
                    //_desyncMessage = "Out of zone";
                }
                else if (_playerMovementDirection != Math.Sign(_velocity.X))
                {
                    double playerRow = Math.Floor(Communicator.OwnPlayerData.PlayerWorldPosition.Y / Definitions.Grid_Cell_Pixel_Size);
                    double peerRow = Math.Floor(Communicator.OtherPlayerData.PlayerWorldPosition.Y / Definitions.Grid_Cell_Pixel_Size);

                    if (playerRow != peerRow)
                    {
                        shouldSync = false;
                        //_desyncMessage = string.Format("row: {0}/{1}", playerRow, peerRow);
                    }
                }
            }

            return shouldSync;
        }

        private void SetVelocityFromPlayer()
        {
            Vector2 targetPosition = new Vector2(Communicator.OwnPlayerData.PlayerWorldPosition.X, _playerPositionLastUpdate.Y);

            CheckAndHandleBoundaryPass();
            if (Math.Abs(Communicator.OwnPlayerData.PlayerWorldPosition.X - _boundaryLine) > Position_Lock_Distance)
            {
                SetNewBoundary();
            }

            if (_shouldBeAhead)
            {
                _relativePositionOffset = Math.Min(Maximum_Position_Offset_Distance, _relativePositionOffset + Position_Offset_Step);
            }
            else
            {
                _relativePositionOffset = Math.Max(-Maximum_Position_Offset_Distance, _relativePositionOffset - Position_Offset_Step);
            }

            targetPosition.X += _relativePositionOffset * _playerMovementDirection;

            UpdateVelocity((targetPosition - WorldPosition) / Milliseconds_Per_Frame);
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

        private void SetVelocityFromCommsData()
        {
            Vector2 targetPosition = Communicator.OtherPlayerData.PlayerWorldPosition;

            UpdateVelocity((targetPosition - WorldPosition) / _averageCommsLagTime);
        }

        private void UpdateVelocity(Vector2 newVelocity)
        {
            if (_velocity == Vector2.Zero)
            {
                _velocity = newVelocity;
            }
            else
            {
                _velocity = (_velocity * 0.5f) + (newVelocity * 0.5f);
            }
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Leda.Core.TextWriter.Write(_desyncMessage, spriteBatch, new Vector2(50, 300), Color.White, 0.99f, Leda.Core.TextWriter.Alignment.Left);
        }

        private const float Fade_Step = 0.01f;
        private const float Position_Lock_Distance = 330.0f;
        private const float Position_Offset_Step = 1.0f;
        private const float Maximum_Position_Offset_Distance = 60.0f;
        private const int Comms_Sample_Count = 20;
        private const int Milliseconds_Per_Frame = 16;
    }
}
