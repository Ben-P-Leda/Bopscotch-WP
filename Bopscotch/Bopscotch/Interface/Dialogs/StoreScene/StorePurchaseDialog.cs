using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;
using Leda.Core.Gamestate_Management;

using Bopscotch.Data;
using Bopscotch.Data.Avatar;
using Bopscotch.Input;
using Bopscotch.Interface.Objects;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class StorePurchaseDialog : CarouselDialog
    {
        private List<AvatarComponentSet> _selectableComponentSets;

        public AvatarComponentSet SelectedComponentSet { get { return _selectableComponentSets[SelectedItem]; } }

        public StorePurchaseDialog(Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _selectableComponentSets = new List<AvatarComponentSet>();

            Height = Dialog_Height;
            TopYWhenActive = (Definitions.Back_Buffer_Height - Dialog_Height) * 0.5f;
            CarouselCenter = new Vector2(Definitions.Back_Buffer_Center.X, Carousel_Center_Y);
            CarouselRadii = new Vector2(Carousel_Horizontal_Radius, Carousel_Vertical_Radius);
            _itemRenderDepths = new Range(Minimum_Item_Render_Depth, Maximum_Item_Render_Depth);
            _itemScales = new Range(Minimum_Item_Scale, Maximum_Item_Scale);

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - 450, 175), Button.ButtonIcon.Previous, Color.DodgerBlue);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + 450, 175), Button.ButtonIcon.Next, Color.DodgerBlue);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 500), Button.ButtonIcon.Back, Color.DodgerBlue, 0.7f);
            AddButton("Buy", new Vector2(Definitions.Right_Button_Column_X, 500), Button.ButtonIcon.Tick, Color.Orange, 0.7f);

            ActionButtonPressHandler = HandleActionButtonPress;
            TopYWhenInactive = Definitions.Back_Buffer_Height;

            SetupButtonLinkagesAndDefaultValues();

            registrationHandler(this);
        }

        private void HandleActionButtonPress(string action)
        {
            DismissWithReturnValue(action);
        }

        protected override void SetupButtonLinkagesAndDefaultValues()
        {
            base.SetupButtonLinkagesAndDefaultValues();

            SetMovementLinksForButton("Back", "previous", "", "", "Change");
            SetMovementLinksForButton("Change", "next", "", "Back", "");
            SetMovementLinksForButton("next", "", "Change", "previous", "");

            _defaultButtonCaption = "Change";
            _activeButtonCaption = "Change";
            _nonSpinButtonCaptions.Add("Change");
        }

        public override void Activate()
        {
            FlushItems();
            _selectableComponentSets.Clear();

            base.Activate();

            CreateCarouselContent();

            ActivateButton("Change");

            InitializeForSpin();
        }

        private void CreateCarouselContent()
        {
            var texts = new[] { "One", "Two", "Three", "Four", "Five", "Six" };
            var bg = new [] {"background-1", "background-2", "background-3", "background-4", "background-5", "background-6"};

            for (int i = 0; i < 6; i++) { AddItem(texts[i], bg[i]); }
        }

        protected void AddItem(string areaName, string textureName)
        {
            StoreCarouselItem area = new StoreCarouselItem(areaName, textureName);
            area.RenderLayer = RenderLayer;

            AddItem(area);
        }

        private const float Maximum_Item_Render_Depth = 0.1f;
        private const float Minimum_Item_Render_Depth = 0.05f;
        private const float Maximum_Item_Scale = 1.0f;
        private const float Minimum_Item_Scale = 0.75f;

        private const float Carousel_Center_Y = 190.0f;
        private const float Carousel_Horizontal_Radius = 270.0f;
        private const float Carousel_Vertical_Radius = 35.0f;
        private const int Dialog_Height = 580;
    }
}
