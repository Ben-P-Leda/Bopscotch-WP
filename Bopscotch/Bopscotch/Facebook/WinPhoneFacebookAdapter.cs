using System;
using System.Collections.Generic;

using Facebook.Client;

using Microsoft.Xna.Framework;

namespace Leda.FacebookAdapter
{
    public class WinPhoneFacebookAdapter : FacebookAdapterBase, IFacebookAdapter
    {
        public static WinPhoneFacebookAdapter Instance { get; private set; }

        public static void CreateInstance()
        {
            Instance = new WinPhoneFacebookAdapter();
        }

        public WinPhoneFacebookAdapter()
            : base()
        {
        }

        public void AttemptLogin()
        {
            if (!_loggedIn)
            {
                if (string.IsNullOrEmpty(Facebook.Client.Session.ActiveSession.CurrentAccessTokenData.AccessToken))
                {
                    Facebook.Client.Session.ActiveSession.LoginWithBehavior(
                        "email,public_profile,user_friends",
                        Facebook.Client.FacebookLoginBehavior.LoginBehaviorMobileInternetExplorerOnly);
                }
                else
                {
                    _loggedIn = true;
                    CompleteAction(ActionResult.LoginAlreadyLoggedIn);
                }
            }
        }

        public void HandleAuthorizationComplete(AccessTokenData tokenData)
        {
            if (string.IsNullOrEmpty(tokenData.AccessToken))
            {
                CompleteAction(ActionResult.LoginCancelled);
            }
            else
            {
                FinishLogin(tokenData.AccessToken, true);
            }
        }

        public override void AttemptLogout()
        {
            Facebook.Client.Session.ActiveSession.Logout();

            base.AttemptLogout();
        }
    }
}

