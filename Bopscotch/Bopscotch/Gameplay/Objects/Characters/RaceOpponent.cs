using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;

using Bopscotch.Communication;
using Bopscotch.Gameplay.Coordination;

namespace Bopscotch.Gameplay.Objects.Characters
{
    public class RaceOpponent : ICameraRelative, ISimpleRenderable, IMobile
    {
        private long _millisecondsSinceLastComms;
        private Vector2 _peerVelocity;
        private Vector2 _expectedPosition;
        private Vector2 _lastCommsPosition;
        private int _lastCommsUpdateTime;
        private int _packetsAtCurrentPosition;
        private int _restartDirection;
        private Vector2 _clientVelocity;
        private int _millisecondsToRestart;
        private int _currentLap;
        private int _currentCheckpoint;

        public Vector2 WorldPosition { get; set; }
        public bool WorldPositionIsFixed { get { return false; } }
        public bool Visible { get; set; }
        public int RenderLayer { get { return 2; } set { } }
        public Vector2 CameraPosition { get; set; }
        public InterDeviceCommunicator Communicator { private get; set; }
        public IMotionEngine MotionEngine { get { return null; } } 

        public void Initialize()
        { 
        }

        public void Reset()
        {
            _currentLap = 0;
            _currentCheckpoint = 0;
            _millisecondsSinceLastComms = 0;
            _peerVelocity = Vector2.Zero;

            WorldPosition = Vector2.Zero;
        }

        public void SetForRaceStart(Vector2 startPosition, bool facingLeft)
        {
            _restartDirection = (facingLeft ? -1 : 1);
            _expectedPosition = startPosition;
            _clientVelocity = Vector2.Zero;

            _lastCommsPosition = startPosition;
            _packetsAtCurrentPosition = 0;

            WorldPosition = startPosition;
            Visible = true;
        }

        public void StartMovement()
        {
            _clientVelocity = new Vector2(Player.PlayerMotionEngine.Minimum_Movement_Speed * _restartDirection, 0.0f) * 0.95f;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            CheckForRestart(millisecondsSinceLastUpdate);

            if (_millisecondsSinceLastComms > Communicator.MillisecondsSinceLastReceive)
            {
                LogPositionUpdates();
                UpdateProgress();
                UpdateLifeState();

                if (Visible)
                {
                    Vector2 step = ((Communicator.OtherPlayerData.PlayerWorldPosition - _expectedPosition) / _millisecondsSinceLastComms) * 0.95f;
                    _peerVelocity = (_peerVelocity * 0.5f) + (step * 0.5f);
                }
            }

            _expectedPosition += _peerVelocity * millisecondsSinceLastUpdate;
            _millisecondsSinceLastComms = Communicator.MillisecondsSinceLastReceive;

            UpdateClientVelocity();

            WorldPosition += _clientVelocity * millisecondsSinceLastUpdate;
        }

        private void LogPositionUpdates()
        {
            if (Communicator.OtherPlayerData.PlayerWorldPosition != _lastCommsPosition)
            {
                _lastCommsPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _lastCommsUpdateTime = Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds;
                _packetsAtCurrentPosition = 0;
            }
            else
            {
                _packetsAtCurrentPosition++;
            }
        }

        private void UpdateProgress()
        {
            if ((Communicator.OtherPlayerData.LapsCompleted > _currentLap) || (Communicator.OtherPlayerData.LastCheckpointIndex > _currentCheckpoint))
            {
                _currentCheckpoint = Communicator.OtherPlayerData.LastCheckpointIndex;
                _currentLap = Communicator.OtherPlayerData.LapsCompleted;

                _restartDirection = Math.Sign(_peerVelocity.X);
            }
        }

        private void UpdateLifeState()
        {
            if ((Visible) && (_packetsAtCurrentPosition > 1))
            {
                _peerVelocity = Vector2.Zero;
                _millisecondsToRestart = RaceProgressCoordinator.Race_Resurrect_Sequence_Duration_In_Milliseconds;

                Visible = false;
            }
            else if ((!Visible) && (_packetsAtCurrentPosition < 1))
            {
                _expectedPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _clientVelocity = Vector2.Zero;
                _peerVelocity = Vector2.Zero;

                WorldPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                Visible = true;
            }
        }

        private void CheckForRestart(int millisecondsSinceLastUpdate)
        {
            if (_millisecondsToRestart > 0)
            {
                _millisecondsToRestart -= millisecondsSinceLastUpdate;

                if (_millisecondsToRestart < 1)
                {
                    Visible = true;
                    StartMovement();
                }
            }
        }

        private void UpdateClientVelocity()
        {
            if (_peerVelocity.X != 0.0f)
            {
                if (_clientVelocity.X == 0)
                {
                    StartMovement();
                }

                _clientVelocity = (_clientVelocity * 0.5f) + (_peerVelocity * 0.5f);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures["opponent-marker"], GameBase.ScreenPosition((WorldPosition - CameraPosition) - new Vector2(0.0f, 40.0f)), null, Color.White, 0.0f, new Vector2(40, 40), 0.5f, SpriteEffects.None, 0.4999f);
        }
    }
}
