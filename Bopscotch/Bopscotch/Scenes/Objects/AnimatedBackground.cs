using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;
using Leda.Core.Gamestate_Management;

namespace Bopscotch.Scenes.Objects
{
    public class AnimatedBackground : ISimpleRenderable, ICameraLinked
    {
        private string _texture;
        private Vector2 _worldDimensions;
        private Rectangle _targetArea;
        private AnimatedBackgroundSegment[] _segments;
        private float _maximumY;
        private float _verticalModifier;

        public bool Visible { get; set; }
        public int RenderLayer { get; set; }
        public Vector2 CameraPosition { set { UpdateSegmentPositions(value); } }

        public AnimatedBackground(string texture, Point worldDimensions)
        {
            _texture = "anim-" + texture;
            _worldDimensions = worldDimensions.ToVector2();

            CalculateBackgroundTargetArea();
            RenderLayer = Render_Layer;
            Visible = true;
        }

        private void CalculateBackgroundTargetArea()
        {
            float heightRatio = (float)Definitions.Back_Buffer_Height / (float)GameBase.Instance.GraphicsDevice.Viewport.Height;
            float targetWidth = (float)Definitions.Back_Buffer_Width / heightRatio;
            float xOffset = ((float)GameBase.Instance.GraphicsDevice.Viewport.Width - targetWidth) / 2.0f;

            _targetArea = new Rectangle((int)xOffset, 0, (int)targetWidth, GameBase.Instance.GraphicsDevice.Viewport.Height);
        }

        public void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public void CreateSegments()
        {
            float maximumCameraY = _worldDimensions.Y - Definitions.Back_Buffer_Height;
            float allowedVerticalRange = MathHelper.Min(maximumCameraY * Vertical_Range_Modifier, Maximum_Vertical_Range);

            _maximumY = allowedVerticalRange;
            _verticalModifier = allowedVerticalRange / maximumCameraY;

            string segmentTexture = _texture + "-segments";
            int segmentCount = (int)(_worldDimensions.X / TextureManager.Textures[segmentTexture].Width) + 1;

            _segments = new AnimatedBackgroundSegment[segmentCount];
            for (int i=0; i<segmentCount; i++)
            {
                _segments[i] = new AnimatedBackgroundSegment(segmentTexture, i);
            }
        }

        public void RegisterBackgroundObjects(Scene.ObjectRegistrationHandler register)
        {
            register(this);
            for (int i=0; i<_segments.Length; i++)
            {
                register(_segments[i]);
            }
        }

        public void UpdateSegmentPositions(Vector2 cameraPosition)
        {
            Vector2 offset = new Vector2(-(cameraPosition.X / 2.0f), MathHelper.Max(_maximumY - (cameraPosition.Y * _verticalModifier), 0.0f));

            for (int i=0; i<_segments.Length; i++)
            {
                _segments[i].SetOffset(offset);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[_texture], _targetArea, null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, Render_Depth);
        }

        private const int Render_Layer = 0;
        private const float Render_Depth = 0.9f;
        private const float Maximum_Vertical_Range = 225.0f;
        private const float Vertical_Range_Modifier = 0.25f;
    }
}
