using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;

namespace Bopscotch.Gameplay.Objects.Environment.Flags
{
    public class ApproachZone : IBoxCollidable, IGameObject
    {
        private Rectangle _collisionBoundingBox;

        public Vector2 WorldPosition { get; set; }
        public bool WorldPositionIsFixed { get { return true; } }
        public Rectangle CollisionBoundingBox { get { return _collisionBoundingBox; } }
        public Rectangle PositionedCollisionBoundingBox { get; set; }
        public bool Collidable { get { return true; } }

        public ApproachZone()
        {
            _collisionBoundingBox = new Rectangle(0, 0, Definitions.Grid_Cell_Pixel_Size * 7, Definitions.Grid_Cell_Pixel_Size * 7);
        }

        public void HandleCollision(ICollidable collider)
        {
        }

        public void Initialize()
        {
        }

        public void Reset()
        {
        }
    }
}
