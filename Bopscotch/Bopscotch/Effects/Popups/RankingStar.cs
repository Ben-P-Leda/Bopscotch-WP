using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core.Asset_Management;

using Bopscotch.Gameplay.Coordination;

namespace Bopscotch.Effects.Popups
{
    public class RankingStar : ISimpleRenderable, IAnimated, ITransformationAnimatable, IColourAnimatable
    {
        
        public SurvivalRankingCoordinator.RankSequenceCallback NextAction { private get; set; }

        private Texture2D _texture;
        private Vector2 _origin;
        private Rectangle _frame;
        private SequenceStep _step;

        public IAnimationEngine AnimationEngine { get; private set; }

        public Vector2 DisplayPosition { private get; set; }
        public bool Visible { get; set; }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public Color Tint { get; set; }
        public int FrameOffset { set { _frame.X = value * _frame.Width; } }

        public void Initialize()
        {
            _texture = TextureManager.Textures[Texture_Name];
            _origin = new Vector2(_texture.Bounds.Height) / 2.0f;
            _frame = new Rectangle(0, 0, _texture.Bounds.Height,_texture.Bounds.Height);

            AnimationEngine = new TransformationAnimationEngine(this);
            AnimationEngine.SequenceCompletionHandler = HandleAnimationSequenceComplete;
        }

        public void Activate()
        {
            Scale = 0.0f;
            Tint = Color.White;
            Rotation = 0.0f;
            Visible = true;

            StartStep(SequenceStep.Entering, Entry_Animation_Sequence);
        }

        public void Reset()
        {
            Scale = 0.0f;
            Visible = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, GameBase.ScreenPosition(DisplayPosition), _frame, Tint, Rotation, _origin, GameBase.ScreenScale(Scale),
                SpriteEffects.None, Render_Depth);
        }

        private void HandleAnimationSequenceComplete()
        {
            switch (_step)
            {
                case SequenceStep.Entering: StartStep(SequenceStep.Bounce, Bounce_Animation_Sequence); NextAction(); break;
                case SequenceStep.Bounce: StartStep(SequenceStep.Rock, Rock_Animation_Sequence); break;
            }
        }

        private void StartStep(SequenceStep step, string animationSequence)
        {
            _step = step;
            AnimationEngine.Sequence = AnimationDataManager.Sequences[animationSequence];
        }

        private enum SequenceStep
        {
            Entering,
            Bounce,
            Rock
        }

        private const int Render_Layer = 4;
        private const float Render_Depth = 0.7f;
        private const string Texture_Name = "ranking-stars";
        private const string Entry_Animation_Sequence = "image-popup-entry-fast";
        private const string Bounce_Animation_Sequence = "quadruple-bounce";
        private const string Rock_Animation_Sequence = "rock";
    }
}
