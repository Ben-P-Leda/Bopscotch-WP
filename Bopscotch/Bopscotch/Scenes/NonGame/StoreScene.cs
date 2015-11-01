using Windows.ApplicationModel.Store;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface.Dialogs.StoreScene;

namespace Bopscotch.Scenes.NonGame
{
    public class StoreScene : MenuDialogScene
    {
        private ListingInformation _products;

        public StoreScene()
            : base()
        {
            _products = null;

            _dialogs.Add("store-status", new StoreStatusDialog());

            BackgroundTextureName = Background_Texture_Name;
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            _dialogs["store-status"].ExitCallback = StoreStatusDialogClose;
        }

        public override void Activate()
        {
            base.Activate();

            if (_products == null) { _dialogs["store-status"].Activate(); }
        }

        private void StoreStatusDialogClose(string selection)
        {
            NextSceneType = typeof(TitleScene); 
            Deactivate();
        }

        private const string Background_Texture_Name = "background-1";
    }
}
