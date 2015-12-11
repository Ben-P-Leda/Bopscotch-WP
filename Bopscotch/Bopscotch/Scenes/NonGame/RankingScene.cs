using System.Xml;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Controllers;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface.Dialogs.RankingScene;

namespace Bopscotch.Scenes.NonGame
{
    public class RankingScene : ContentSceneWithControlDialog
    {
        public RankingScene()
            : base()
        {
            BackgroundTextureName = Background_Texture_Name;
        }

        public override void Initialize()
        {
            NavigationDialog navigationDialog = new NavigationDialog();
            navigationDialog.SelectionCallback = HandleNavigationDialogDismiss;

            Dialog = navigationDialog;

            base.Initialize();
        }

        private void HandleNavigationDialogDismiss(string buttonCaption)
        {
            NextSceneType = typeof(TitleScene);
            NextSceneParameters.Set("music-already-running", true);

            Deactivate();
        }

        private const string Background_Texture_Name = "background-1";
    }
}
