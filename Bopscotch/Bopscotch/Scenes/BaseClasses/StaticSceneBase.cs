using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Scenes.NonGame;
using Bopscotch.Scenes.Objects;
using Bopscotch.Gameplay.Objects.Environment;

namespace Bopscotch.Scenes.BaseClasses
{
    public abstract class StaticSceneBase : Scene
    {
        private string _backgroundTextureName;

        protected List<Input.InputProcessorBase> _inputProcessors;
        protected Background _background;

        private AnimatedBackground _animBackground;
        private Vector2 _cameraPosition;
        private int _cameraStep;

        protected string BackgroundTextureName { set { _backgroundTextureName = value; SetBackgroundTexture(); } }

        public StaticSceneBase()
            : base()
        {
            _inputProcessors = Data.Profile.Settings.AllControllers;
            _backgroundTextureName = "";

            _cameraPosition = Vector2.Zero;
            _cameraStep = 2;
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            if (loaderSceneType == typeof(StartupLoadingScene)) { CompletePostStartupLoadInitialization(); }
        }

        protected virtual void CompletePostStartupLoadInitialization()
        {
            _background = new Bopscotch.Gameplay.Objects.Environment.Background();
            SetBackgroundTexture();
            RegisterGameObject(_background);
        }

        private void SetBackgroundTexture()
        {
            if ((_background != null) && (!string.IsNullOrEmpty(_backgroundTextureName)))
            {
                _background.TextureReference = _backgroundTextureName;
            }
        }

        public void CreateAnimatedBackground(string reference)
        {
            _animBackground = new AnimatedBackground(reference, new Point(Animated_Background_Width, Definitions.Back_Buffer_Height), 0);
            _animBackground.CreateComponents();
            _animBackground.RegisterBackgroundObjects(RegisterGameObject);
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _inputProcessors.Count; i++) { _inputProcessors[i].Update(MillisecondsSinceLastUpdate); }

            if (_animBackground != null)
            {
                if ((_cameraPosition.X + _cameraStep > Animated_Background_Width) || (_cameraPosition.X + _cameraStep < 0.0f))
                {
                    _cameraStep = -_cameraStep;
                }
                _cameraPosition += new Vector2(_cameraStep, 0.0f);
                _animBackground.CameraPosition = _cameraPosition;
            }

            base.Update(gameTime);
        }

        private const int Animated_Background_Width = 4800;
    }
}
