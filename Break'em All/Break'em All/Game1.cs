using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Break_em_All
{

    enum GameState { START, RUNNING, END }
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Paddle paddle;
        Ball ball;

        int bricksWide;
        int bricksHigh;
        Texture2D brickImage;
        Brick[,] bricks;

        Rectangle screenRectangle;
        private int score;

        // The font used to display UI elements
        SpriteFont font;

        GameState gameState;

        // Used for adding new bricks every x seconds.
        TimeSpan oldElapsedGameTime;

        SoundEffect ballPaddleCollisionSound;
        SoundEffect brickBallCollisionSound;

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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ballPaddleCollisionSound = Content.Load<SoundEffect>("sound/smallBeep2");
            brickBallCollisionSound = Content.Load<SoundEffect>("sound/explosion");

            brickImage = Content.Load<Texture2D>("brick");

            screenRectangle = new Rectangle(
             0,
             0,
             graphics.PreferredBackBufferWidth - graphics.PreferredBackBufferWidth % brickImage.Width,
             graphics.PreferredBackBufferHeight);

            Texture2D paddleTexture = Content.Load<Texture2D>("paddle");
            paddle = new Paddle(paddleTexture, screenRectangle);

            Texture2D ballTexture = Content.Load<Texture2D>("ball");
            ball = new Ball(ballTexture, screenRectangle);

            //initialize number of brick slots on the screen.
            bricksHigh = (screenRectangle.Height - paddleTexture.Height - ballTexture.Height) / brickImage.Height;
            bricksWide = screenRectangle.Width / brickImage.Width;

            font = Content.Load<SpriteFont>("Kootenay");

            this.gameState = GameState.START;

            StartGame();
        }

        /// <summary>
        /// updated these with start values: paddle, ball and bricks. 
        /// </summary>
        private void StartGame()
        {
            paddle.SetInStartPosition();
            ball.SetInStartPosition(paddle.GetBounds());

            bricks = new Brick[bricksWide, bricksHigh];
            for (int y = 0; y < bricksHigh; y++)
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
                for (int x = 0; x < bricksWide; x++)
                {
                    var isBrickAlive = (y < 5);

                    bricks[x, y] = new Brick(
                    brickImage,
                    new Rectangle(
                    x * brickImage.Width,
                    y * brickImage.Height,
                    brickImage.Width,
                    brickImage.Height),
                    tint,
                    isBrickAlive);
                }
            }

            //Initialize score with 0
            this.score = 0;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            paddle.Dispose();
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
                            paddle.Update(gesture.Delta);
                        }
                    }

                    // Update the ball position
                    ball.Update();

                    // Add the new brick before checking for collision with the ball.
                    if (oldElapsedGameTime.TotalMilliseconds > 1000)
                    {
                        oldElapsedGameTime = new TimeSpan(0);
                        Random rand = new Random();
                        var brickNumberX = rand.Next(0, this.bricksWide);
                        for (int brickNumberY = 0; ; brickNumberY++)
                        {
                            if (brickNumberY == bricksHigh)
                            {
                                this.gameState = GameState.END;
                                TouchPanel.EnabledGestures = GestureType.Tap;
                                break;
                            }

                            var currentBrick = bricks[brickNumberX, brickNumberY];
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
                    foreach (Brick brick in bricks)
                    {
                        var isBrickCollided = brick.CheckCollision(ball, brickBallCollisionSound);
                        if (isBrickCollided)
                            this.score += 10;
                    }
                    ball.PaddleCollision(paddle.GetBounds(), ballPaddleCollisionSound);

                    //If ball is fallen, end the game.
                    if (ball.OffBottom())
                    {
                        this.gameState = GameState.END;
                        TouchPanel.EnabledGestures = GestureType.Tap;
                    }

                    base.Update(gameTime);

                    break;

                case GameState.END:
                    // Wait till there is any touch gesture. Afterwards, change the gameState to RUNNING and continue further.
                    if (TouchPanel.IsGestureAvailable)
                    {
                        this.gameState = GameState.RUNNING;
                        TouchPanel.EnabledGestures = GestureType.HorizontalDrag;
                        StartGame();
                    }
                    break;
                default:
                    //TODO: logging
                    break;
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            Texture2D rect = new Texture2D(GraphicsDevice, screenRectangle.Width, screenRectangle.Height);

            Color[] data = new Color[screenRectangle.Width * screenRectangle.Height];
            for (int i = 0; i < data.Length; ++i) 
                data[i] = Color.CornflowerBlue;
            rect.SetData(data);

            spriteBatch.Draw(rect, new Vector2(0, 0), Color.CornflowerBlue);

            switch (this.gameState)
            {
                case GameState.START:
                    // Draw 'play'
                    var playTexture = font.MeasureString("Play");
                    spriteBatch.DrawString(font, "Play", new Vector2(screenRectangle.Width / 2 - playTexture.X / 2, screenRectangle.Height / 2 - playTexture.Y / 2), Color.White);
                    break;
                case GameState.RUNNING:
                    paddle.Draw(spriteBatch);

                    ball.Draw(spriteBatch);
                    foreach (Brick brick in bricks)
                        brick.Draw(spriteBatch);

                    // Draw the score
                    spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);

                    break;
                case GameState.END:
                    //paddle.Draw(spriteBatch);
                    //ball.Draw(spriteBatch);
                    //foreach (Brick brick in bricks)
                    //    brick.Draw(spriteBatch);

                    // Draw the score
                    //spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);

                    //Show animation for game end. Afterwards, give option to play again.

                    // Draw 'play again'
                    var scoreTexture = font.MeasureString("Score: " + this.score);
                    spriteBatch.DrawString(font, "Score: " + this.score, new Vector2(screenRectangle.Width / 2 - scoreTexture.X / 2, screenRectangle.Height / 2 - 2 * scoreTexture.Y), Color.White);
                    var playAgainTexture = font.MeasureString("Play Again");
                    spriteBatch.DrawString(font, "Play Again", new Vector2(screenRectangle.Width / 2 - playAgainTexture.X / 2, screenRectangle.Height / 2 - playAgainTexture.Y / 2), Color.White);
                    break;
                default:
                    //TODO: logging
                    break;
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
