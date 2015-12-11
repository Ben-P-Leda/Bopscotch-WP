using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;
using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Scenes.NonGame
{
    public abstract class ContentSceneWithControlDialog : FlatContentScene
    {
        private MotionController _motionController;

        protected bool _maintainsTitleSceneMusic;
        protected string _contentFileName;
        protected ButtonDialog Dialog { private get; set; }

        public ContentSceneWithControlDialog()
            : base()
        {
            _motionController = new MotionController();

            RegisterGameObject(new Effects.FullScreenColourOverlay() { Tint = Color.Black, TintFraction = 0.5f });

            _maintainsTitleSceneMusic = true;            
        }

        public override void Initialize()
        {
            base.Initialize();

            if (Dialog != null)
            {
                Dialog.InputSources = _inputProcessors;
                _motionController.AddMobileObject(Dialog);
                RegisterGameObject(Dialog);
            }
        }

        protected override void Reset()
        {
            if (Dialog != null)
            {
                Dialog.Reset();
            }

            base.Reset();
        }

        public override void Activate()
        {
            Dialog.Activate();

            base.Activate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _motionController.Update(MillisecondsSinceLastUpdate);
        }

        protected override void HandleBackButtonPress()
        {
            base.HandleBackButtonPress();

            if (CurrentState == Status.Active) { Dialog.Cancel(); }
        }

        protected const float Element_Render_Depth = 0.5f;
    }
}
