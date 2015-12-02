using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Shapes;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class BombBlockBlastCollider : ICircularCollidable
    {
        public Circle CollisionBoundingCircle { get; private set; }
        public Circle PositionedCollisionBoundingCircle { get; set; }
        public bool Collidable { get; set; }
        public Vector2 WorldPosition { get; set; }
        public bool WorldPositionIsFixed { get { return true; } }

        public BombBlockBlastCollider()
        {
            CollisionBoundingCircle = new Circle(Vector2.Zero, Collision_Radius);
            Collidable = false;
        }

        public void HandleCollision(ICollidable collider)
        {
        }

        private const float Collision_Radius = 50.0f;
    }
}
