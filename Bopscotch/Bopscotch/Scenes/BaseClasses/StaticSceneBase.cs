﻿using System;
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

        protected AnimatedBackground _animBackground;
        private Vector2 _cameraPosition;
        private Vector2 _cameraStep;

        protected string BackgroundTextureName { set { _backgroundTextureName = value; SetBackgroundTexture(); } }

        public StaticSceneBase()
            : base()
        {
            _inputProcessors = Data.Profile.Settings.AllControllers;
            _backgroundTextureName = "";

            _cameraPosition = Vector2.Zero;
            _cameraStep = new Vector2(Background_Camera_Step, 0.0f);
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

        public void CreateBackgroundForScene(string reference, int[] componentSequence)
        {
            _animBackground = AnimatedBackground.Create(reference, componentSequence);
            RegisterGameObject(_animBackground);
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _inputProcessors.Count; i++) { _inputProcessors[i].Update(MillisecondsSinceLastUpdate); }

            if (_animBackground != null)
            {
                if (_animBackground.Wrap)
                {
                    UpdateWrappingBackground();
                }
                else
                {
                    UpdatePanningBackground();
                }
            }

            base.Update(gameTime);
        }

        private void UpdateWrappingBackground()
        {
            _animBackground.UpdateComponentPositions(_cameraStep * MillisecondsSinceLastUpdate);
        }

        private void UpdatePanningBackground()
        {
            float updatedX = _cameraPosition.X + (_cameraStep.X * MillisecondsSinceLastUpdate);
            if ((updatedX > AnimatedBackground.Fixed_Width) || (updatedX < 0.0f))
            {
                _cameraStep.X = -_cameraStep.X;
            }

            _cameraPosition += _cameraStep * MillisecondsSinceLastUpdate;
            _animBackground.CameraPosition = _cameraPosition;
        }

        private float Background_Camera_Step = 0.125f;
    }
}
