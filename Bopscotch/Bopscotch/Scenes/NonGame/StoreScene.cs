using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

using Windows.ApplicationModel.Store;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Scenes.Gameplay.Survival;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.StoreScene;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class StoreScene : MenuDialogScene
    {
        private bool _returnToGame;

        private PurchaseCompleteDialog _purchaseCompleteDialog;
        private ConsumablesDialog _consumablesDialog;

        public StoreScene()
            : base()
        {
            _purchaseCompleteDialog = new PurchaseCompleteDialog("");
            _purchaseCompleteDialog.SelectionCallback = PurchaseDialogButtonCallback;
            _consumablesDialog = new ConsumablesDialog();

            _dialogs.Add("loading-store", new LoadingDialog(LoadProducts));
            _dialogs.Add("store-closed", new StoreClosedDialog());
            _dialogs.Add("store-items", new StorePurchaseDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("purchase-complete", _purchaseCompleteDialog);
            _dialogs.Add("consumables", _consumablesDialog);

            BackgroundTextureName = Background_Texture_Name;
        }

        private void PurchaseDialogButtonCallback(string buttonCaption)
        {
            if ((buttonCaption == "Back") && (_consumablesDialog.Active)) { _consumablesDialog.DismissWithReturnValue(""); }
        }

        protected override void CompletePostStartupLoadInitialization()
        {
            base.CompletePostStartupLoadInitialization();

            foreach (KeyValuePair<string, ButtonDialog> kvp in _dialogs) { kvp.Value.ExitCallback = HandleActiveDialogExit; }
        }

        public override void Activate()
        {
            _returnToGame = NextSceneParameters.Get<bool>("return-to-game");

            base.Activate();

            MusicManager.StopMusic();

            ActivateDialog("loading-store");
        }

        private async void LoadProducts()
        {
            ListingInformation products = null;

            try
            {
                products = await CurrentApp.LoadListingInformationAsync();
            }
            catch (Exception)
            {
                products = null;
            }

            _dialogs["loading-store"].DismissWithReturnValue("");

            if ((products != null) && (products.ProductListings.Count > 0))
            {
                ((StorePurchaseDialog)_dialogs["store-items"]).InitializeProducts(products);
                _purchaseCompleteDialog.Products = products;
                ActivateDialog("store-items");
                _consumablesDialog.Activate();
            }
            else
            {
                ActivateDialog("store-closed");
            }
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            if (selectedOption == "Buy")
            {
                InitiatePurchase(((StorePurchaseDialog)_dialogs["store-items"]).Selection);
            }
            else if (_lastActiveDialogName == "purchase-complete")
            {
                ActivateDialog("store-items");
            }
            else if (!string.IsNullOrWhiteSpace(selectedOption))
            {
                if ((_returnToGame) && (Data.Profile.Lives > 0))
                {
                    NextSceneType = typeof(SurvivalGameplayScene);
                    MusicManager.PlayLoopedMusic("survival-gameplay");
                }
                else
                {
                    NextSceneType = typeof(TitleScene);
                }
                Deactivate();
            }
        }

        private void InitiatePurchase(string selection)
        {
            Deployment.Current.Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    string receipt = await CurrentApp.RequestProductPurchaseAsync(selection, true);

                    if (CurrentApp.LicenseInformation.ProductLicenses[selection].IsActive)
                    {
                        CurrentApp.ReportProductFulfillment(selection);

                        FulfillPurchase(selection);

                        _purchaseCompleteDialog.ItemCode = selection;
                        ActivateDialog("purchase-complete");
                    }
                    else
                    {
                        ActivateDialog("store-items");
                    }
                }
                catch (Exception)
                {
                    ActivateDialog("store-items");
                }
            });
        }

        private void FulfillPurchase(string productCode)
        {
            switch (productCode)
            {
                // OLD SET
                //case "Bopscotch_Test_Product": Data.Profile.Lives += 1; Data.Profile.GoldenTickets += 1; break;
                //case "Bopscotch_10_Lives": Data.Profile.Lives += 10; break;
                //case "Bopscotch_20_Lives": Data.Profile.Lives += 20; break;
                //case "Bopscotch_50_Lives": Data.Profile.Lives += 50; break;
                //case "Bopscotch_2_Tickets": Data.Profile.GoldenTickets += 2; break;
                //case "Bopscotch_5_tickets": Data.Profile.GoldenTickets += 5; break;
                //case "Bopscotch_10_Tickets": Data.Profile.GoldenTickets += 10; break;

                case "Bopscotch_15_Lives": Data.Profile.Lives += 15; break;                 // £0.79 - $0.99
                case "Bopscotch_30_Lives": Data.Profile.Lives += 30; break;                 // £1.49 - $1.99
                case "Bopscotch_50_Lives": Data.Profile.Lives += 50; break;                 // £2.29 - $2.99
                case "Bopscotch_2_Tickets": Data.Profile.GoldenTickets += 2; break;         // £0.79
                case "Bopscotch_5_Tickets": Data.Profile.GoldenTickets += 5; break;         // £1.49
            }

            Data.Profile.Save();
        }

        private const string Background_Texture_Name = "background-1";
        
        public const float Dialog_Margin = 40.0f;
    }
}
