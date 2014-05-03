using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Diagnostics;

namespace Break_em_All
{

    enum GameState { START, RUNNING, END }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Paddle paddleObj;
        private Ball ballObj;

        int numBricksInX;
        int numBricksInY;
        private Texture2D brickTexture;
        private Brick[,] bricksArray;

        private Texture2D playIcon;

        public static Rectangle gameContentRect;
        Rectangle adUnitRect;
        private int gameScore;

        // The font used to display UI elements
        private SpriteFont font24, font36;

        private GameState gameState;

        private Background backgroundObj;

        // Used for adding new bricks every x seconds.
        private TimeSpan oldElapsedGameTime;

        private SoundEffect ballPaddleCollisionSound;
        private SoundEffect brickBallCollisionSound;

        //// Advertisement
        private static readonly string applicationIdStr = "test_client";
        private static readonly string adUnitIdStr = "Image320_50"; //other test values: Image480_80, Image300_50, TextAd
        private Advertisement advertisement;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            //TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            //InactiveSleepTime = TimeSpan.FromSeconds(1);

            //graphics.PreferredBackBufferWidth = 750;
            //graphics.PreferredBackBufferHeight = 600;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            TouchPanel.EnabledGestures = GestureType.Tap;

            Advertisement.Initialize(this, applicationIdStr);

            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.playIcon = Content.Load<Texture2D>("playIcon");
            this.ballPaddleCollisionSound = Content.Load<SoundEffect>("sound/beep1");
            this.brickBallCollisionSound = Content.Load<SoundEffect>("sound/beep6");
            this.font24 = Content.Load<SpriteFont>("font/Kootenay24");
            this.font36 = Content.Load<SpriteFont>("font/Kootenay36");
            this.brickTexture = Content.Load<Texture2D>("brickTranslucent");

            // Initialize dimensions fo game content. brickTexture.Width is used here as gameContent.Width should be N * brickTexture.Width.
            var availScreenWidthForGame = this.graphics.PreferredBackBufferWidth;
            var paddingInDirectionX = availScreenWidthForGame % this.brickTexture.Width; // This value of padding will be divided on either side.
            var actualScreenWidthUsedForGame = availScreenWidthForGame - paddingInDirectionX;
            var availScreenHeightForGame = this.graphics.PreferredBackBufferHeight;
            Game1.gameContentRect = new Rectangle( // TODO: center the rectangle.
             (paddingInDirectionX / 2) + (paddingInDirectionX % 2), 0, // position on screen from where onwards (inclusive), game components could be drawn.
             actualScreenWidthUsedForGame,
             availScreenHeightForGame);
            // Now create an actual ad for display.
            // Create a banner ad for the game. Advertisement is shown below the game content.
            int adUnitWidth = 320, adUnitHeight = 50;
            adUnitRect = new Rectangle(
                (GraphicsDevice.Viewport.Bounds.Width - adUnitWidth) / 2, // centered on the display
                Game1.gameContentRect.Y + Game1.gameContentRect.Height - adUnitHeight,
                adUnitWidth, adUnitHeight);
            this.advertisement = new Advertisement(adUnitRect, applicationIdStr, adUnitIdStr);

            Texture2D paddleTexture = Content.Load<Texture2D>("paddleTranslucent");
            this.paddleObj = new Paddle(paddleTexture, Game1.gameContentRect);

            Texture2D ballTexture = Content.Load<Texture2D>("ball");
            this.ballObj = new Ball(ballTexture, Game1.gameContentRect);

            //initialize number of brick slots on the screen.
            this.numBricksInY = (Game1.gameContentRect.Height - paddleTexture.Height - ballTexture.Height) / this.brickTexture.Height;
            this.numBricksInX = Game1.gameContentRect.Width / this.brickTexture.Width;

            //Texture2D backgroundTexture = Content.Load<Texture2D>("XNA_pow2");
            Texture2D backgroundTexture = Content.Load<Texture2D>("stars256x256");
            Rectangle backgroundTextureBounds = new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height);
            backgroundObj = new Background(backgroundTexture, backgroundTextureBounds);

            this.gameState = GameState.START;

            StartGame();
        }

        /// <summary>
        /// updated these with start values: paddle, ball and bricks. 
        /// </summary>
        private void StartGame()
        {
            paddleObj.SetInStartPosition();
            ballObj.SetInStartPosition(paddleObj.GetBounds());

            bricksArray = new Brick[numBricksInX, numBricksInY];
            for (int y = 0; y < numBricksInY; y++)
            {
                Color tint = Color.White;
                switch (y % 5)
                {
                    case 0:
                        tint = Color.Blue;
                        break;
                    case 1:
                        tint = Color.Red;
                        break;
                    case 2:
                        tint = Color.Green;
                        break;
                    case 3:
                        tint = Color.Yellow;
                        break;
                    case 4:
                        tint = Color.Purple;
                        break;
                }
                for (int x = 0; x < numBricksInX; x++)
                {
                    var isBrickAlive = (y < 5);

                    bricksArray[x, y] = new Brick(
                    brickTexture,
                    new Rectangle(
                    x * brickTexture.Width + gameContentRect.X,
                    y * brickTexture.Height + gameContentRect.Y,
                    brickTexture.Width,
                    brickTexture.Height),
                    tint,
                    isBrickAlive);
                }
            }

            //Initialize score with 0
            this.gameScore = 0;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            paddleObj.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            backgroundObj.Update(gameTime);

            switch (this.gameState)
            {
                case GameState.START:
                    // Wait till there is any touch gesture. Afterwards, change the gameState to RUNNING and continue further.
                    if (TouchPanel.IsGestureAvailable)
                    {
                        this.gameState = GameState.RUNNING;
                        TouchPanel.EnabledGestures = GestureType.HorizontalDrag;
                    }
                    break;
                case GameState.RUNNING:
                    // Update paddle position as per user horizontal drag input
                    while (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gesture = TouchPanel.ReadGesture();
                        if (gesture.GestureType == GestureType.HorizontalDrag)
                        {
                            paddleObj.Update(gesture.Delta);
                        }
                    }

                    // Update the ball position
                    ballObj.Update(gameTime);

                    // Add the new brick before checking for collision with the ball.
                    if (oldElapsedGameTime.TotalMilliseconds > 1000)
                    {
                        oldElapsedGameTime = new TimeSpan(0);
                        Random rand = new Random();
                        var brickNumberX = rand.Next(0, this.numBricksInX);
                        for (int brickNumberY = 0; ; brickNumberY++)
                        {
                            if (brickNumberY == numBricksInY)
                            {
                                this.gameState = GameState.END;
                                TouchPanel.EnabledGestures = GestureType.Tap;
                                break;
                            }

                            var currentBrick = bricksArray[brickNumberX, brickNumberY];
                            if (currentBrick.getIsAlive() == false)
                            {
                                currentBrick.toggleIsAlive();
                                break;
                            }
                        }
                    }
                    else
                        oldElapsedGameTime += gameTime.ElapsedGameTime;

                    // Check brick and ball collisions. Update scores
                    foreach (Brick brick in bricksArray)
                    {
                        var isBrickCollided = brick.CheckCollision(ballObj, brickBallCollisionSound);
                        if (isBrickCollided)
                            this.gameScore += 10;
                    }
                    ballObj.PaddleCollision(paddleObj.GetBounds(), ballPaddleCollisionSound);

                    //If ball is fallen, end the game.
                    if (ballObj.OffBottom())
                    {
                        this.gameState = GameState.END;
                        TouchPanel.EnabledGestures = GestureType.Tap;
                    }
                    break;

                case GameState.END:
                    // Wait till there is any touch gesture. Afterwards, change the gameState to RUNNING and continue further.
                    if (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gesture = TouchPanel.ReadGesture();
                        if (gesture.GestureType == GestureType.Tap)
                        {
                            if (gesture.Position.Y >= adUnitRect.Top && gesture.Position.Y <= adUnitRect.Bottom)
                            {
                                // Ad region is tapped
                                // Do nothing
                            }
                            else if (advertisement.isUserEngaged())
                            {
                                //User spending time on the ad landing page
                                // Do nothing
                            }
                            else
                            {
                                this.gameState = GameState.RUNNING;
                                TouchPanel.EnabledGestures = GestureType.HorizontalDrag;
                                StartGame();
                            }
                        }

                    }
                    break;
                default:
                    //TODO: logging
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //spriteBatch.Begin();
            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Opaque, SamplerState.LinearWrap, null, null);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap,
                DepthStencilState.Default, RasterizerState.CullNone);

            switch (this.gameState)
            {
                case GameState.START:
                    // Set the gameContentRect height
                    Game1.gameContentRect.Height = this.graphics.PreferredBackBufferHeight;

                    //hide ads
                    advertisement.setVisible(false);

                    //Draw background image
                    backgroundObj.Draw(spriteBatch, 0.5f);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                    // Draw 'play'
                    Vector2 playTextureDimensions = font36.MeasureString("Play  ");
                    Vector2 playIconDimensions = new Vector2(this.playIcon.Width, this.playIcon.Height);
                    Vector2 playTextPosition = new Vector2(Game1.gameContentRect.X + Game1.gameContentRect.Width / 2 - (playTextureDimensions.X + playIconDimensions.X) / 2, Game1.gameContentRect.Y + Game1.gameContentRect.Height / 2 - (playTextureDimensions.Y) / 2);
                    spriteBatch.DrawString(font36, "Play  ", playTextPosition, Color.White);
                    Rectangle playIconRect = new Rectangle(
                        (int)(Game1.gameContentRect.X + Game1.gameContentRect.Width / 2.0 + playTextureDimensions.X / 2.0 - playIconDimensions.X / 2.0),
                        (int)(Game1.gameContentRect.Y + Game1.gameContentRect.Height / 2.0 - (playIconDimensions.Y) / 2.0),
                        this.playIcon.Width, this.playIcon.Height);
                    spriteBatch.Draw(playIcon, playIconRect, Color.White);
                    break;
                case GameState.RUNNING:
                    // Set the gameContentRect height
                    Game1.gameContentRect.Height = this.graphics.PreferredBackBufferHeight;

                    //hide ads
                    advertisement.setVisible(false);

                    //Draw background image
                    backgroundObj.Draw(spriteBatch, 1f);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    paddleObj.Draw(spriteBatch);

                    ballObj.Draw(spriteBatch);
                    foreach (Brick brick in bricksArray)
                        brick.Draw(spriteBatch);

                    // Draw the score
                    spriteBatch.DrawString(font24, "Score: " + gameScore, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + gameContentRect.X, GraphicsDevice.Viewport.TitleSafeArea.Y + gameContentRect.Y), Color.White);

                    break;
                case GameState.END:
                    // Set the gameContentRect height as per ad unit height
                    Game1.gameContentRect.Height = this.graphics.PreferredBackBufferHeight - 50;

                    //Show ads
                    advertisement.setVisible(true);

                    //Draw background image
                    backgroundObj.Draw(spriteBatch, 0.5f);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                    // Draw 'Score'
                    var scoreTexture = font24.MeasureString("Score: " + this.gameScore);
                    spriteBatch.DrawString(font24, "Score: " + this.gameScore, new Vector2(gameContentRect.X + gameContentRect.Width / 2 - scoreTexture.X / 2, gameContentRect.Y + gameContentRect.Height / 2 - 2 * scoreTexture.Y), Color.White);

                    // Draw 'Play'
                    Vector2 playTextureDimensions2 = font36.MeasureString("Play  ");
                    Vector2 playIconDimensions2 = new Vector2(this.playIcon.Width, this.playIcon.Height);
                    Vector2 playTextPosition2 = new Vector2(Game1.gameContentRect.X + Game1.gameContentRect.Width / 2 - (playTextureDimensions2.X + playIconDimensions2.X) / 2, Game1.gameContentRect.Y + Game1.gameContentRect.Height / 2 - (playTextureDimensions2.Y) / 2);
                    spriteBatch.DrawString(font36, "Play  ", playTextPosition2, Color.White);
                    Rectangle playIconRect2 = new Rectangle(
                        (int)(Game1.gameContentRect.X + Game1.gameContentRect.Width / 2.0 + playTextureDimensions2.X / 2.0 - playIconDimensions2.X / 2.0),
                        (int)(Game1.gameContentRect.Y + Game1.gameContentRect.Height / 2.0 - (playIconDimensions2.Y) / 2.0),
                        this.playIcon.Width, this.playIcon.Height);
                    spriteBatch.Draw(playIcon, playIconRect2, Color.White);
                    break;
                default:
                    //TODO: logging
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Clean up the Advertisement
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            advertisement.Dispose(disposing);
        }
    }
}
