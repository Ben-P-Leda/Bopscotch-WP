﻿using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Data;
using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.NonGame;
using Bopscotch.Interface.Dialogs.RaceFinishScene;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceFinishScene : MenuDialogScene
    {
        private AnimationController _animationController;

        public RaceFinishScene()
            : base()
        {
            _animationController = new AnimationController();

            _dialogs.Add("results", new ResultsDialog(RegisterGameObject) { ExitCallback = ReturnToTitleScene } );

            BackgroundTextureName = Background_Texture_Name;
        }

        private void ReturnToTitleScene(string buttonCaption)
        {
            NextSceneParameters.Clear();
            NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, "start");
            NextSceneType = typeof(TitleScene);
            Deactivate();
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            ((ResultsDialog)_dialogs["results"]).InitializeComponents();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        public override void Activate()
        {
            base.Activate();

            Definitions.RaceOutcome outcome = NextSceneParameters.Get<Definitions.RaceOutcome>(Outcome_Parameter_Name);

            ((ResultsDialog)_dialogs["results"]).Outcome = outcome;
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();

            ActivateDialog("results");
        }

        public override void Update(GameTime gameTime)
        {
            _animationController.Update(MillisecondsSinceLastUpdate);

            base.Update(gameTime);
        }

        private const string Background_Texture_Name = "background-2";

        public const string Outcome_Parameter_Name = "race-outcome";
    }
}