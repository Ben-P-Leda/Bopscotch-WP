using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Motion;

using Bopscotch.Communication;

namespace Bopscotch.Gameplay.Objects.Characters
{
    public class RaceOpponent : ICameraRelative, ISimpleRenderable, IMobile
    {
        private long _millisecondsSinceLastComms;
        private Vector2 _velocity;
        private Vector2 _expectedPosition;
        private Vector2 _lastCommsPosition;
        private int _packetsAtCurrentPosition;
        private bool _isAlive;

        public Vector2 WorldPosition { get; set; }
        public bool WorldPositionIsFixed { get { return false; } }
        public bool Visible { get { return true; } set { } }
        public int RenderLayer { get { return 2; } set { } }
        public Vector2 CameraPosition { get; set; }
        public InterDeviceCommunicator Communicator { private get; set; }
        public IMotionEngine MotionEngine { get { return null; } } 

        public void Initialize()
        { 
        }

        public void Reset()
        {
            _millisecondsSinceLastComms = 0;
            _velocity = Vector2.Zero;

            WorldPosition = Vector2.Zero;
        }

        public void SetForRaceStart(Vector2 startPosition)
        {
            _expectedPosition = startPosition;
            WorldPosition = startPosition;

            _lastCommsPosition = startPosition;
            _packetsAtCurrentPosition = 0;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            if (_millisecondsSinceLastComms > Communicator.MillisecondsSinceLastReceive)
            {
                LogPositionUpdates();
                UpdateLifeState();

                if (_isAlive)
                {
                    Vector2 step = ((Communicator.OtherPlayerData.PlayerWorldPosition - _expectedPosition) / _millisecondsSinceLastComms) * 0.95f;
                    _velocity = (_velocity * 0.5f) + (step * 0.5f);
                }
            }

            _expectedPosition += _velocity * millisecondsSinceLastUpdate;
            _millisecondsSinceLastComms = Communicator.MillisecondsSinceLastReceive;

            WorldPosition = _expectedPosition;
        }

        private void LogPositionUpdates()
        {
            if (Communicator.OtherPlayerData.PlayerWorldPosition != _lastCommsPosition)
            {
                _lastCommsPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                _packetsAtCurrentPosition = 0;
            }
            else
            {
                _packetsAtCurrentPosition++;
            }
        }

        private void UpdateLifeState()
        {
            if ((_isAlive) && (_packetsAtCurrentPosition > 1))
            {
                _isAlive = false;
                _velocity = Vector2.Zero;
            }
            else if ((!_isAlive) && (_packetsAtCurrentPosition < 1))
            {
                _isAlive = true;
                _expectedPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
                WorldPosition = Communicator.OtherPlayerData.PlayerWorldPosition;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures["opponent-marker"], GameBase.ScreenPosition((WorldPosition - CameraPosition) - new Vector2(0.0f, 40.0f)), null, Color.White, 0.0f, new Vector2(40, 40), 0.5f, SpriteEffects.None, 0.4999f);
        }
    }
}
