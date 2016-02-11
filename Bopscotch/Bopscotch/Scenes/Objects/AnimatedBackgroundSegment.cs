using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

namespace Bopscotch.Scenes.Objects
{
    public class AnimatedBackgroundSegment : ISimpleRenderable
    {
        private Texture2D _texture;
        private Rectangle _sourceArea;
        private Vector2 _initialPosition;
        private Vector2 _position;

        private float _heightRatio;
        public bool Visible { get { return true; } set { } }
        public int RenderLayer { get { return Render_Layer; } set { } }

        public AnimatedBackgroundSegment(string texture, int segmentIndex, int frameSeed, int heightSeed)
        {
            _texture = TextureManager.Textures[texture];

            int frameCount = _texture.Height / Unscaled_Height;
            _sourceArea = new Rectangle(0, (((segmentIndex + frameSeed) % frameCount) * Unscaled_Height) + 1, _texture.Width, Unscaled_Height - 2);

            _heightRatio = (float)GameBase.Instance.GraphicsDevice.Viewport.Height / (float)Definitions.Back_Buffer_Height;
            _initialPosition = new Vector2(
                segmentIndex * TextureManager.Textures[texture].Width * Scale, 
                ((Definitions.Back_Buffer_Height - (Unscaled_Height * Scale)) - 2) + (Height_Step * heightSeed));
        }

        public void Initialize()
        { 

        }

        public void Reset()
        {

        }

        public void SetOffset(Vector2 offset)
        {
            _position = _initialPosition + offset;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position * _heightRatio, _sourceArea, Color.White, 0.0f, Vector2.Zero, _heightRatio * Scale, 
                SpriteEffects.None, Render_Depth);
        }

        private const int Render_Layer = 0;
        private const float Render_Depth = 0.9f;
        private const int Unscaled_Height = 225;
        private const float Height_Step = 15.0f;

        public const float Scale = 2.0f;
    }
}
