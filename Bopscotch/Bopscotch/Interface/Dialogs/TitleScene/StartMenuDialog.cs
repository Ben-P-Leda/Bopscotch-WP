using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE
using Microsoft.Phone.Net.NetworkInformation;
#endif
#if __ANDROID__
using Android.Net;
#endif

using Leda.Core;

using Bopscotch.Data;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class StartMenuDialog : ButtonDialog
    {
        private bool _networkIsAvailable;
        private string _raceAvailabilityMessage;

        public StartMenuDialog()
            : base()
        {
            _defaultButtonCaption = "Adventure";
            _cancelButtonCaption = "Back";

            _boxCaption = "Select Game Mode";
            _networkIsAvailable = true;
        }

        public override void Activate()
        {
            _raceAvailabilityMessage = "";
#if WINDOWS_PHONE
            _networkIsAvailable = DeviceNetworkInformation.IsWiFiEnabled && DeviceNetworkInformation.IsNetworkAvailable;
#endif
#if __ANDROID__
			_networkIsAvailable = (MainActivity.NetManager.GetNetworkInfo(ConnectivityType.Wifi).GetState() == NetworkInfo.State.Connected);
#endif

            ClearButtons();
            if (Profile.IsTrialVersion) { SetForTrialVersion(); }
            else { SetForFullVersion(); }

            if ((!_networkIsAvailable) && ((!Profile.IsTrialVersion) || (Profile.TrialRacesRemaining > 0))) { DisableRaceOption(); }

            base.Activate();
        }

        private void SetForTrialVersion()
        {
            _raceAvailabilityMessage = string.Format("{0} trial races left", Math.Max(Profile.TrialRacesRemaining, 0));

            SetMetricsDependentOnMessageRequirement();

            if (Profile.TrialRacesRemaining > 0) 
            { 
                SetUpButtonsWithRaceModeAvailable(); 
            }
            else 
            { 
                SetUpButtonsWithRaceModeUnavailable();
                _raceAvailabilityMessage = string.Concat(_raceAvailabilityMessage, " - buy full game for more");
            }
        }

        private void SetMetricsDependentOnMessageRequirement()
        {
            if ((!_networkIsAvailable) || (Profile.IsTrialVersion))
            {
                Height = Dialog_Height_With_Message;
                TopYWhenActive = Top_Y_When_Active_With_Message;
            }
            else
            {
                Height = Dialog_Height_No_Message;
                TopYWhenActive = Top_Y_When_Active_No_Message;
            }
        }

        private void SetUpButtonsWithRaceModeAvailable()
        {
            SetUpDefaultButtons();
            AddButton("Race", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Race, Color.LawnGreen);
        }

        private void SetUpButtonsWithRaceModeUnavailable()
        {
            SetUpDefaultButtons();
            AddButton("Full Game", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Store, Color.Orange);
        }

        private void SetUpDefaultButtons()
        {
            AddButton("Adventure", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Adventure, Color.LawnGreen);
            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 320.0f), Button.ButtonIcon.Back, Color.Red, 0.7f);
        }

        private void SetForFullVersion()
        {
            SetMetricsDependentOnMessageRequirement();
            SetUpButtonsWithRaceModeAvailable();
        }

        private void DisableRaceOption()
        {
            DisableButton("Race");
            _buttons["Race"].IconBackgroundTint = Color.Gray;

            _raceAvailabilityMessage = "Wifi must be connected to race";
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_raceAvailabilityMessage))
            {
                TextWriter.Write(
                    Translator.Translation(_raceAvailabilityMessage), spriteBatch, new Vector2(Definitions.Back_Buffer_Center.X, 380.0f + WorldPosition.Y), 
                    Color.White, Color.Black, 3.0f, 0.7f, 0.1f, TextWriter.Alignment.Center);
            }

            base.Draw(spriteBatch);
        }

        private const int Dialog_Height_With_Message = 480;
        private const float Top_Y_When_Active_With_Message = 350.0f;
        private const int Dialog_Height_No_Message = 420;
        private const float Top_Y_When_Active_No_Message = 400.0f;
    }
}
