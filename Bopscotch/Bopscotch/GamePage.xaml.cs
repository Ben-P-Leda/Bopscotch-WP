using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;

using MonoGame.Framework.WindowsPhone;

using Bopscotch.Resources;

namespace Bopscotch
{
    public partial class GamePage : PhoneApplicationPage
    {
        private Game1 _game;

        public GamePage()
        {
            InitializeComponent();

            _game = XamlGame<Game1>.Create("", this);
        }
    }
}