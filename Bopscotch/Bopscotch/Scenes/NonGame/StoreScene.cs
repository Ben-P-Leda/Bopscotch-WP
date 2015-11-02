using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

using Windows.ApplicationModel.Store;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.StoreScene;

namespace Bopscotch.Scenes.NonGame
{
    public class StoreScene : MenuDialogScene
    {
        private ListingInformation _products;
        private string _lastAttemptedPurchase;

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

            _lastAttemptedPurchase = "";

            if ((_products == null) || (_products.ProductListings.Count < 1)) { ActivateDialog("store-status"); }
            else { ActivateDialog("store-items"); }
        }

        private async void LoadProducts()
        {
            _products = await CurrentApp.LoadListingInformationAsync();
        }

        private void HandleActiveDialogExit(string selectedOption)
        {
            if (selectedOption == "Buy")
            {
                _lastAttemptedPurchase = "";
                InitiatePurchase(((StorePurchaseDialog)_dialogs["store-items"]).Selection);
            }
            else
            {
                NextSceneType = typeof(TitleScene);
                NextSceneParameters.Set("music-already-running", true);
                Deactivate();
            }
        }

        private void InitiatePurchase(string selection)
        {
            selection = "Bopscotch_Test_Product";

            Deployment.Current.Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    string receipt = await CurrentApp.RequestProductPurchaseAsync(selection, true);

                    if (CurrentApp.LicenseInformation.ProductLicenses[selection].IsActive)
                    {
                        _lastAttemptedPurchase = selection;

                        CurrentApp.ReportProductFulfillment(selection);

                        FulfillPurchase(selection);

                    }
                }
                catch (Exception ex)
                {
                    string err = ex.ToString();
                }
            });
        }

        private void FulfillPurchase(string productCode)
        {
            // TODO: make this work
            Console.WriteLine("Fulfill: " + productCode);
        }

        public override void HandleFastResume()
        {
            Console.WriteLine("Last purchase: " + _lastAttemptedPurchase);
            base.HandleFastResume();



            if (!string.IsNullOrEmpty(_lastAttemptedPurchase))
            {
                // TODO: Open success dialog with appropriate captioning
            }
            else
            {
                ActivateDialog("store-items");
            }
        }

        private const string Background_Texture_Name = "background-1";
    }
}
