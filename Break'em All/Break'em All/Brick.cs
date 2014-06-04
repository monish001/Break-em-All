using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
namespace Break_em_All
{
    class Brick
    {
        Texture2D texture;
        Rectangle location;
        Color tint;
        bool isAlive;

        public bool getIsAlive()
        {
            return isAlive;
        }
        public void toggleIsAlive()
        {
            this.isAlive = !(this.isAlive);
        }

        public Rectangle Location
        {
            get { return location; }
        }
        public Brick(Texture2D texture, Rectangle location, Color tint, bool isAlive)
        {
            this.texture = texture;
            this.location = location;
            this.tint = tint;
            this.isAlive = isAlive;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ball"></param>
        /// <returns>true if the brick collided with the ball.</returns>
        public bool CheckCollision(Ball ball, SoundEffect brickBreakSound)
        {
            if (isAlive && ball.Bounds.Intersects(location))
            {
                isAlive = false;
                ball.Deflection(this);
                if (brickBreakSound != null)
                {
                    brickBreakSound.Play();
                }
                return true;
            }
            return false;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (isAlive)
                spriteBatch.Draw(texture, location, tint * 0.9f);
        }
    }
}
