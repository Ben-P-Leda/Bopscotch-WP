using System;
using System.Collections.Generic;

using Windows.ApplicationModel.Store;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface.Dialogs;
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

            LoadProducts();

            _dialogs.Add("store-status", new StoreStatusDialog());
            _dialogs.Add("store-items", new StorePurchaseDialog(RegisterGameObject, UnregisterGameObject));

            BackgroundTextureName = Background_Texture_Name;
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            foreach (KeyValuePair<string, ButtonDialog> kvp in _dialogs) { kvp.Value.ExitCallback = HandleActiveDialogExit; }
        }

        public override void Activate()
        {
            base.Activate();

            if ((_products == null) || (_products.ProductListings.Count < 1)) { _dialogs["store-status"].Activate(); }
            else { _dialogs["store-items"].Activate(); }
        }

        private async void LoadProducts()
        {
            _products = await CurrentApp.LoadListingInformationAsync();
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            // TODO: Get this working
            NextSceneType = typeof(TitleScene);
            Deactivate();

            //switch (selectedOption)
            //{
            //    case "Back":
            //        if (_lastActiveDialogName == "options") { NextSceneType = typeof(TitleScene); Deactivate(); }
            //        else { ActivateDialog("options"); }
            //        break;
            //    case "Change":
            //        ActivateComponentSelector();
            //        break;
            //    case "Select":
            //        _avatar.ClearSkin();
            //        _avatar.SkinBones(AvatarComponentManager.SideFacingAvatarSkin(Profile.Settings.SelectedAvatarSlot));
            //        ActivateDialog("options");
            //        break;
            //}
        }

        private const string Background_Texture_Name = "background-1";
    }
}
