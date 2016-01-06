using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

using Bopscotch.Communication;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class CommunicationStatusDisplay : ISimpleRenderable
    {
        private Vector2 _position;
        private Rectangle _frame;
        private long _millisecondsSinceLastComms;
        private long _previousUpdateLag;
        private bool _communicationsLost;

        public Vector2 Position { set { _position = new Vector2(value.X, value.Y - Frame_Side_Length); } }
        public int RenderLayer { get { return Render_Layer; } set { } }
        public bool Visible { get; set; }

        public InterDeviceCommunicator Communicator { private get; set; }

        public CommunicationStatusDisplay()
        {
            _frame = new Rectangle(0, 0, Frame_Side_Length, Frame_Side_Length);
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
            Visible = false;

            _frame.X = 0;
            _communicationsLost = false;
            _millisecondsSinceLastComms = 0;
            _previousUpdateLag = 0;

            UpdateCommunicationStatus(CommsStatus.Good);
        }

        public void Update()
        {
            if (!_communicationsLost)
            {
                if (Communicator.MillisecondsSinceLastReceive < _millisecondsSinceLastComms)
                {
                    if (_previousUpdateLag < Signal_Deterioration_Threshold)
                    {
                        UpdateCommunicationStatus(CommsStatus.Good);
                    }
                    _previousUpdateLag = _millisecondsSinceLastComms;

                    System.Diagnostics.Debug.WriteLine(_millisecondsSinceLastComms);
                }
                else if (Communicator.MillisecondsSinceLastReceive > Signal_Deterioration_Threshold)
                {
                    UpdateCommunicationStatus(CommsStatus.Poor);
                }

                _millisecondsSinceLastComms = Communicator.MillisecondsSinceLastReceive;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[Texture_Name], GameBase.ScreenPosition(_position), _frame, Color.White, 0.0f, Vector2.Zero,
                GameBase.ScreenScale(0.8f), SpriteEffects.None, Render_Depth);
        }

        public void DisplayCommunicationLost()
        {
            _communicationsLost = true;
            UpdateCommunicationStatus(CommsStatus.Lost);
        }

        private void UpdateCommunicationStatus(CommsStatus status)
        {
            _frame.X = (int)status;
        }

        private enum CommsStatus
        {
            Good = 0,
            Poor = 200,
            Lost = 400
        }

        private const int Frame_Side_Length = 200;
        private const string Texture_Name = "icon-comms-status";
        private const float Render_Depth = 0.1f;
        private const int Render_Layer = 4;
        private const int Signal_Deterioration_Threshold = 112;
    }
}
