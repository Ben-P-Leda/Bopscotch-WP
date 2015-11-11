using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.Dialogs.StoreScene
{
    public class LoadingDialog : ButtonDialog
    {
        public delegate void EntryCompleteHandler();
        private EntryCompleteHandler _entryCompleteHandler;

        public LoadingDialog(EntryCompleteHandler entryCompleteHandler)
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            _boxCaption = Translator.Translation(Dialog_Caption);
            _entryCompleteHandler = entryCompleteHandler;
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        protected override void HandleDialogEntryCompletion()
        {
            base.HandleDialogEntryCompletion();

            if (_entryCompleteHandler != null)
            {
                _entryCompleteHandler();
            }
        }

        private const int Dialog_Height = 300;
        private const string Dialog_Caption = "loading";
        private const float Top_Line_Y = 80.0f;
        private const float Line_Height = 50.0f;
    }
}
