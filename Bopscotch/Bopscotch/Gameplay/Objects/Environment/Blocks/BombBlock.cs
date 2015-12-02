using System;

using Microsoft.Xna.Framework;

using Leda.Core.Timing;

using Bopscotch.Effects.Particles;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class BombBlock : Block
    {
        private Timer _actionTimer;
        
        public BlockMap Map { private get; set; }
        public Point MapLocation { private get; set; }
        public bool ShouldRegenerate { private get; set; }
        public BombBlockBlastCollider BlastCollider { get; private set; }

        public AdditiveLayerParticleEffectManager.CloudBurstEffectInitiator RegenerationParticleEffect { private get; set; }
        public AdditiveLayerParticleEffectManager.FireballEffectInitiator DetonationParticleEffect { private get; set; }

        public TimerController.TickCallbackRegistrationHandler TickCallback
        {
            set
            {
                if (_actionTimer == null) { _actionTimer = new Timer("action-timer", UpdateState); }
                value(_actionTimer.Tick);
            }
        }

        public BombBlock()
            : base()
        {
            _actionTimer = null;

            BlastCollider = new BombBlockBlastCollider();
        }

        public void TriggerByImpact()
        {
            _actionTimer.NextActionDuration = Impact_Detonation_Delay;
        }

        public void TriggerByChain()
        {
            _actionTimer.NextActionDuration = Chain_Detonation_Delay;
        }

        private void UpdateState()
        {
            if (Visible)
            {
                Detonate();
            }
            else if (ShouldRegenerate)
            {
                Regenerate();
            }
        }

        private void Detonate()
        {
            DetonationParticleEffect(this);

            Visible = false;
            Collidable = false;

            BlastCollider.WorldPosition = WorldPosition;
            BlastCollider.Collidable = true;

            TriggerNext(-1);
            TriggerNext(1);
        }

        private void TriggerNext(int direction)
        {
            int position = MapLocation.X;
            bool finished = false;

            while (!finished)
            {
                position += direction;
                if ((position < 0) || (position >= Map.MapDimensions.X))
                {
                    finished = true;
                }
                else
                {
                    Type blockType = Map.GetTileType(position, MapLocation.Y);
                    if (blockType == null)
                    {
                        finished = true;
                    }
                    else if (blockType == typeof(BombBlock))
                    {
                        ((BombBlock)Map.GetTile(position, MapLocation.Y)).TriggerByChain();
                        finished = true;
                    }
                }
            }
        }

        private void Regenerate()
        {

        }

        public new const string Data_Node_Name = "bomb-block";

        private const float Impact_Detonation_Delay = 150.0f;
        private const float Chain_Detonation_Delay = 30.0f;
    }
}
