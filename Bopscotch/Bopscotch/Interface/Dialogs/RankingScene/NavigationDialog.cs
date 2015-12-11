using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Tasks;

namespace Bopscotch.Interface.Dialogs.RankingScene
{
    public class NavigationDialog : ButtonDialog
    {
        public NavigationDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Dialog_Height + Bottom_Margin);
                
            _cancelButtonCaption = "Back";
        }

        public override void Activate()
        {
            ClearButtons();

            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, Button_Y), Button.ButtonIcon.Back, Color.Red, 0.6f);

            float offset = Definitions.Back_Buffer_Center.X * 0.85f;

            AddIconButton("previous", new Vector2(Definitions.Back_Buffer_Center.X - offset, Button_Y), Button.ButtonIcon.Previous, Color.DodgerBlue, 0.6f);
            AddIconButton("next", new Vector2(Definitions.Back_Buffer_Center.X + offset, Button_Y), Button.ButtonIcon.Next, Color.DodgerBlue, 0.6f);

            base.Activate();
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, Definitions.Back_Buffer_Height);
        }

        protected override bool HandleButtonTouch(string buttonCaption)
        {
            return buttonCaption == "Back";
        }

        private const int Dialog_Height = 150;
        private const float Bottom_Margin = 20.0f;
        private const float Button_Y = 75.0f;
    }
}
