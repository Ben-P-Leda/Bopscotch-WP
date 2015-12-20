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
        private Vector2 _physicalPosition;
        private Vector2 _difference;

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
            _physicalPosition = Vector2.Zero;
            _difference = Vector2.Zero;

            WorldPosition = Vector2.Zero;
        }

        public void Update(int millisecondsSinceLastUpdate)
        {
            Vector2 currentPosition = ((OpponentRaceProgressCoordinator)Communicator.OtherPlayerData).PlayerWorldPosition + (_velocity * millisecondsSinceLastUpdate);

            if (_physicalPosition == Vector2.Zero)
            {
                _physicalPosition = currentPosition;
                WorldPosition = currentPosition;
            }

            if (_millisecondsSinceLastComms > Communicator.MillisecondsSinceLastReceive)
            {
                _difference = currentPosition - WorldPosition;

                Vector2 step = (_difference / _millisecondsSinceLastComms) * 0.95f;
                if (_velocity.X == 0)
                {
                    _velocity = step / 0.95f;
                }
                else
                {
                    _velocity = (_velocity * 0.5f) + (step * 0.5f);
                }
            }

            _physicalPosition += _velocity * millisecondsSinceLastUpdate;
            _millisecondsSinceLastComms = Communicator.MillisecondsSinceLastReceive;

            WorldPosition = _physicalPosition + _difference;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures["opponent-marker"], GameBase.ScreenPosition((WorldPosition - CameraPosition) - new Vector2(0.0f, 40.0f)), null, Color.White, 0.0f, new Vector2(40, 40), 0.5f, SpriteEffects.None, 0.4999f);
        }
    }
}
