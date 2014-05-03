using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Break_em_All
{
    /// <summary>
    /// 
    /// </summary>
    public class Background
    {
        Texture2D texture;
        Rectangle screenBounds, textureBounds;

        /// <summary>
        /// this represents the starting point from the texture that is rendered as background.
        /// </summary>
        Vector2 currentSourcePosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture">represents background image which will be used as background in full of part.</param>
        /// <param name="textureBounds">represents part of texture that should be used as background.</param>
        /// <param name="screenBounds">represents part of screen where background should be rendered.</param>
        public Background(Texture2D texture, Rectangle textureBounds, Rectangle screenBounds)
        {
            this.texture = texture;
            this.screenBounds = screenBounds;
            this.textureBounds = textureBounds;
            this.currentSourcePosition = new Vector2(textureBounds.X, textureBounds.Y);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            var sign = (new Random()).Next(0, 2) == 0 ? -1 : 1;
            var speedVec = new Vector2(sign * 0.03F, 0.05F); // pixels per millisecond
            this.currentSourcePosition += (gameTime.ElapsedGameTime.Milliseconds * speedVec);
            //this.currentSourcePosition.X += 0.2F;
            //this.currentSourcePosition.Y += 0.5F;
            if (this.currentSourcePosition.X >= texture.Width)
                this.currentSourcePosition.X = 0;
            if (this.currentSourcePosition.Y >= texture.Height)
                this.currentSourcePosition.Y = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Moving the starting point for source texture.
            Rectangle sourceTexture = new Rectangle((int)this.currentSourcePosition.X, (int)this.currentSourcePosition.Y, this.textureBounds.Width, this.textureBounds.Height);
            spriteBatch.Draw(this.texture, screenBounds, sourceTexture, Color.White);

            //Color color = Color.White;
            //Rectangle destRect = new Rectangle(0, 0, 1 * this.texture.Width, 1 * this.texture.Height);
            //spriteBatch.Draw(this.texture, Vector2.Zero, destRect, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            // Moving the destination position
            //Rectangle destRect = new Rectangle((int)this.currentSourcePosition.X, (int)this.currentSourcePosition.Y, this.screenBounds.Width, this.screenBounds.Height);
        }
    }
}
