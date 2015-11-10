﻿using Microsoft.Xna.Framework;

using Bopscotch.Interface;

using Windows.ApplicationModel.Store;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class PurchaseCompleteDialog : ButtonDialog
    {
        public ListingInformation Products { private get; set; }
        public string ItemCode { private get; set; }

        public PurchaseCompleteDialog(string caption)
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            AddButton("OK", new Vector2(Definitions.Back_Buffer_Center.X, 200), Button.ButtonIcon.Tick, Color.LawnGreen);

            _cancelButtonCaption = "OK";

            _boxCaption = Translator.Translation(caption);
        }

        public override void Activate()
        {
            _boxCaption = Translator.Translation("purchase-complete").Replace("[ITEM]", Products.ProductListings[ItemCode].Name);
            base.Activate();
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        private const int Dialog_Height = 300;
        private const float Top_Line_Y = 80.0f;
        private const float Line_Height = 50.0f;
    }
}
