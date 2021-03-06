using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace Break_em_All
{
    class Ball
    {
        Vector2 direction; // Unit vector to represent the direction of ball's movement.
        Vector2 position;
        Rectangle bounds;
        float currentBallSpeedPerMilliSec;
        Texture2D texture;
        Rectangle screenBounds;
        bool collided;
        const float INIT_BALL_SPEED = 0.5f;
        public Rectangle Bounds
        {
            get
            {
                bounds.X = (int)position.X;
                bounds.Y = (int)position.Y;
                return bounds;
            }
        }
        public Ball(Texture2D texture, Rectangle screenBounds)
        {
            bounds = new Rectangle(0, 0, texture.Width, texture.Height);
            this.texture = texture;
            this.screenBounds = screenBounds;
        }
        public void Update(GameTime gameTime)
        {
            //TODO: MAKE IT FRAME-RATE INDEPENDENT
            collided = false;
            position += direction * currentBallSpeedPerMilliSec * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            currentBallSpeedPerMilliSec += 0.0001f;

            CheckWallCollision();
        }
        private void CheckWallCollision()
        {
            // If ball goes out of screen on left side, bring it back and turn the horizontal direction.
            if (position.X < screenBounds.X)
            {
                position.X = screenBounds.X;
                direction.X *= -1;
            }

            // If ball goes out of screen on right side, bring it back and turn the horizontal direction.
            if (position.X + texture.Width > screenBounds.X + screenBounds.Width)
            {
                position.X = screenBounds.X + screenBounds.Width - texture.Width;
                direction.X *= -1;
            }

            // If ball goes beyond the top, bring it bit back and turn the vertical direction.
            if (position.Y < 0)
            {
                position.Y = 0;
                direction.Y *= -1;
            }
        }
        public void SetInStartPosition(Rectangle paddleLocation)
        {
            Random rand = new Random();
            direction = new Vector2(rand.Next(2, 6), -rand.Next(2, 6));
            direction.Normalize();
            currentBallSpeedPerMilliSec = INIT_BALL_SPEED;
            position.Y = paddleLocation.Y - texture.Height;
            position.X = paddleLocation.X + (paddleLocation.Width - texture.Width) / 2;
        }
        public bool OffBottom()
        {
            if (position.Y > screenBounds.Y + screenBounds.Height)
                return true;
            return false;
        }
        public void PaddleCollision(Rectangle paddleLocation, SoundEffect paddleCollisionSound)
        {
            Rectangle ballLocation = new Rectangle(
            (int)position.X,
            (int)position.Y,
            texture.Width,
            texture.Height);
            if (paddleLocation.Intersects(ballLocation))
            {
                position.Y = paddleLocation.Y - texture.Height;
                direction.Y *= -1;

                if (paddleCollisionSound != null)
                {
                    paddleCollisionSound.Play();
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public void Deflection(Brick brick)
        {
            if (!collided)
            {
                direction.Y *= -1;
                collided = true;
            }
        }

    }
}
