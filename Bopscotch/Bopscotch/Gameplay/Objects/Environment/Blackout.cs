using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Gameplay.Objects.Environment
{
    public class Blackout : ISimpleRenderable
    {
        private Timer _timer;
        private State _state;
        private Color _tint;
        private float _renderDepth;
        private Rectangle _targetArea;

        public bool Visible { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }

        public TimerController.TickCallbackRegistrationHandler TickCallback { set { value(_timer.Tick); } }

        public Blackout()
            : base()
        {
            _renderDepth = Render_Depth;
            _timer = new Timer("blackout-timer", HandleTimerActionComplete);

            CalculateBackgroundTargetArea();
            Reset();
        }

        private void HandleTimerActionComplete()
        {
            switch (_state)
            {
                case State.FadingIn: _state = State.Sustaining; _timer.NextActionDuration = Sustain_Duration_In_Milliseconds; break;
                case State.Sustaining: _state = State.FadingOut; _timer.NextActionDuration = Fade_Duration_In_Milliseconds; break;
                case State.FadingOut: Reset(); break;
            }
        }

        private void CalculateBackgroundTargetArea()
        {
            float heightRatio = (float)Definitions.Back_Buffer_Height / (float)GameBase.Instance.GraphicsDevice.Viewport.Height;
            float targetWidth = (float)Definitions.Back_Buffer_Width / heightRatio;
            float xOffset = ((float)GameBase.Instance.GraphicsDevice.Viewport.Width - targetWidth) / 2.0f;

            _targetArea = new Rectangle((int)xOffset, 0, (int)targetWidth, GameBase.Instance.GraphicsDevice.Viewport.Height);
        }

        public void Initialize() { }

        public void Reset()
        {
            _tint = Color.Transparent;
            _state = State.FadingIn;
            Visible = false;
        }

        public void Activate()
        {
            Reset();
            _timer.NextActionDuration = Fade_Duration_In_Milliseconds;
            Visible = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (_state)
            {
                case State.FadingIn: _tint = Color.Lerp(Color.Transparent, Color.Black, _timer.CurrentActionProgress); break;
                case State.Sustaining: _tint = Color.Black; break;
                case State.FadingOut: _tint = Color.Lerp(Color.Black, Color.Transparent, _timer.CurrentActionProgress); break;
            }

            spriteBatch.Draw(TextureManager.Textures[Texture_Reference], _targetArea, null, _tint, 0.0f, Vector2.Zero, SpriteEffects.None, _renderDepth);
        }

        private enum State
        {
            FadingIn,
            Sustaining,
            FadingOut
        }

        private const string Texture_Reference = "pixel";
        private const int Render_Layer = 2;
        private const float Render_Depth = 0.425f;
        private const int Fade_Duration_In_Milliseconds = 250;
        private const int Sustain_Duration_In_Milliseconds = 2000;
    }
}