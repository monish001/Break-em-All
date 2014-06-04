// TODO:
// P0: Info - show text image over a popup
// P1: Pause the game if running on interruption like home key press
// P2: Back button on GameRunning screen should go to New game screen
// P2: Seperate icons for music and sounds

// DONE:
// P0: Store, read and show top five highscores
// P0: Update icons
// P1: Fix problem of text getting beneath the black overlay layer.

using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
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
        private float opacity = 0.7f;

        private bool isMoreIconExpanded = false;
        private bool isSound = true;
        private bool isGamePaused = false;

        // Title as shown in the start screen
        private Texture2D title;

        #region Icons and their bounding rectangles
        // Play icon shown in start screen and pause pane.
        private Texture2D playIcon;
        private Rectangle playIconStartScreenRect;

        // More icons shown in start and end screen.
        private Texture2D moreIcon;
        private Rectangle moreIconRect;
        private Texture2D soundOnIcon;
        private Texture2D soundOffIcon;
        private Rectangle soundIconRect;
        private Texture2D infoIcon;
        private Rectangle infoIconRect;
        private Texture2D rateIcon;
        private Rectangle rateIconRect;

        // Pause icon as shown during the game play.
        private Texture2D pauseIcon;
        private Rectangle pauseIconRect;

        // Icons shown in the pause pane. Play and sound are already mentioned above.
        private Texture2D resumeIcon;
        private Texture2D replayIcon;
        private Rectangle replayIconPausePaneRect;
        private Rectangle resumeIconRect;
        private Rectangle soundIconPausePaneRect;

        // Icon for end screen.
        private Rectangle replayIconEndScreenRect;
        #endregion

        private Rectangle pausePaneRect;
        private Texture2D blackTexture;
        private Texture2D whiteTexture;

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
        //private static readonly string applicationIdStr = "test_client";
        //private static readonly string adUnitIdStr = "Image320_50"; //other test values: Image480_80, Image300_50, TextAd
        private static readonly string applicationIdStr = "3179fba6-21f8-412c-a41a-081a160cc475";
        private static readonly string adUnitIdStr = "10736295"; //other test values: Image480_80, Image300_50, TextAd
        private Advertisement advertisement;

        private HighScores highScores;

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
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.HorizontalDrag;

            Advertisement.Initialize(this, applicationIdStr);

            this.highScores = new HighScores();

            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: Hide notification center if tried to open directly.

            base.Initialize();
        }

        //void GameActivated(object sender, ActivatedEventArgs e)
        //{
        //    bug.Position = (Vector2)(PhoneApplicationService.Current.State["BugPos"]);
        //    bug.Rotation = (float)(PhoneApplicationService.Current.State["BugRot"]);
        //    bug.Target = (Vector2)(PhoneApplicationService.Current.State["BugTarget"]);
        //    bug.Moving = (bool)(PhoneApplicationService.Current.State["BugMoving"]);
        //    foodLocations = (List<Vector2>)(PhoneApplicationService.Current.State["foodLocations"]);
        //    gameIsPaused = true;
        //    pausedImageOnScreen = false;
        //}

        //void GameDeactivated(object sender, DeactivatedEventArgs e)
        //{
        //    PhoneApplicationService.Current.State["BugPos"] = bug.Position;
        //    PhoneApplicationService.Current.State["BugRot"] = bug.Rotation;
        //    PhoneApplicationService.Current.State["BugTarget"] = bug.Target;
        //    PhoneApplicationService.Current.State["BugMoving"] = bug.Moving;
        //    PhoneApplicationService.Current.State["foodLocations"] = foodLocations;
        //}

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.title = Content.Load<Texture2D>("title single line");
            this.playIcon = Content.Load<Texture2D>("Icons/play256");

            this.moreIcon = Content.Load<Texture2D>("Icons/more64");
            this.infoIcon = Content.Load<Texture2D>("Icons/info64");
            this.rateIcon = Content.Load<Texture2D>("Icons/rate64");
            this.soundOnIcon = Content.Load<Texture2D>("Icons/soundOn64");
            this.soundOffIcon = Content.Load<Texture2D>("Icons/soundOff64");

            this.pauseIcon = Content.Load<Texture2D>("Icons/pause64");
            this.resumeIcon = Content.Load<Texture2D>("Icons/play256");
            this.replayIcon = Content.Load<Texture2D>("Icons/replay128");

            this.blackTexture = Content.Load<Texture2D>("Colors/black");
            this.whiteTexture = Content.Load<Texture2D>("Colors/white");

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

            Texture2D backgroundTexture = Content.Load<Texture2D>("stars256x256");
            Rectangle backgroundTextureBounds = new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height);
            backgroundObj = new Background(backgroundTexture, backgroundTextureBounds);

            Song song = Content.Load<Song>("Sound/Vegas Glitz");  // Put the name of your song in instead of "song_title"
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);

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
            // TODO: handle it based on page.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            switch (this.gameState)
            {
                case GameState.START:
                    backgroundObj.Update(gameTime);

                    // Wait till there is any touch gesture. Afterwards, change the gameState to RUNNING and continue further.
                    if (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gesture = TouchPanel.ReadGesture();
                        switch (gesture.GestureType)
                        {
                            case GestureType.Tap:
                                Rectangle gestureRect = new Rectangle((int)gesture.Position.X, (int)gesture.Position.Y, 0, 0);
                                if (this.playIconStartScreenRect.Intersects(gestureRect))
                                {
                                    this.gameState = GameState.RUNNING;
                                    this.isMoreIconExpanded = false;
                                }
                                else
                                {
                                    this.UpdateMoreIcons(gestureRect);
                                }
                                break;
                        }
                    }
                    break;

                case GameState.RUNNING:
                    if (isGamePaused)
                    {
                        this.UpdatePausePaneIcons();
                        break;
                    }

                    backgroundObj.Update(gameTime);

                    // Update paddle position as per user input
                    while (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gesture = TouchPanel.ReadGesture();
                        switch (gesture.GestureType)
                        {
                            case GestureType.HorizontalDrag:
                                paddleObj.Update(gesture.Delta);
                                break;
                            case GestureType.Tap:
                                Rectangle gestureRect = new Rectangle((int)gesture.Position.X, (int)gesture.Position.Y, 0, 0);
                                if (this.pauseIconRect.Intersects(gestureRect))
                                {
                                    this.isGamePaused = true;
                                }
                                break;
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
                                this.UpdateEndGame();
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
                        var isBrickCollided = brick.CheckCollision(ballObj, this.isSound ? brickBallCollisionSound : null);
                        if (isBrickCollided)
                            this.gameScore += 10;
                    }
                    ballObj.PaddleCollision(paddleObj.GetBounds(), this.isSound ? ballPaddleCollisionSound : null);

                    //If ball is fallen, end the game.
                    if (ballObj.OffBottom())
                    {
                        this.UpdateEndGame();
                    }
                    break;

                case GameState.END:
                    backgroundObj.Update(gameTime);

                    // Wait till there is any touch gesture. Afterwards, change the gameState to RUNNING and continue further.
                    if (TouchPanel.IsGestureAvailable)
                    {
                        GestureSample gesture = TouchPanel.ReadGesture();
                        switch (gesture.GestureType)
                        {
                            case GestureType.Tap:
                                Rectangle gestureRect = new Rectangle((int)gesture.Position.X, (int)gesture.Position.Y, 0, 0);
                                if (adUnitRect.Intersects(gestureRect))
                                {
                                    // Ad region is tapped
                                    // Do nothing for now.
                                }
                                else if (advertisement.isUserEngaged())
                                {
                                    // Ad is already tapped and now user is spending time on the ad landing page
                                    // Do nothing for now.
                                }
                                else if (this.replayIconEndScreenRect.Intersects(gestureRect))
                                {
                                    this.gameState = GameState.RUNNING;
                                    StartGame();
                                }
                                else
                                {
                                    this.UpdateMoreIcons(gestureRect);
                                }
                                break;
                        }
                    }
                    break;
                default:
                    //TODO: logging
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdateEndGame()
        {
            highScores.updateHighScore(this.gameScore);
            this.gameState = GameState.END;
        }

        private void UpdatePausePaneIcons()
        {
            // Update paddle position as per user input
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        Rectangle gestureRect = new Rectangle((int)gesture.Position.X, (int)gesture.Position.Y, 0, 0);
                        if (this.resumeIconRect.Intersects(gestureRect))
                        {
                            this.isGamePaused = false;
                        }
                        else if (this.replayIconPausePaneRect.Intersects(gestureRect))
                        {
                            this.isGamePaused = false;
                            this.gameState = GameState.RUNNING;
                            StartGame();
                        }
                        else if (this.soundIconPausePaneRect.Intersects(gestureRect))
                        {
                            this.isSound = !this.isSound;
                            MediaPlayer.IsMuted = !this.isSound;
                        }
                        else if (!this.pausePaneRect.Intersects(gestureRect))
                        {
                            this.isGamePaused = false;
                        }
                        break;
                }
            }
        }

        private void UpdateMoreIcons(Rectangle gestureRect)
        {
            if (this.moreIconRect.Intersects(gestureRect))
            {
                // toggle the state of expanded icons
                this.isMoreIconExpanded = !this.isMoreIconExpanded;
            }
            else if (this.isMoreIconExpanded && this.rateIconRect.Intersects(gestureRect))
            {
                // navigate to rate page
                MarketplaceReviewTask review = new MarketplaceReviewTask();
                review.Show();
            }
            else if (this.isMoreIconExpanded && this.infoIconRect.Intersects(gestureRect))
            {
                // TODO: Show info
            }
            else if (this.isMoreIconExpanded && this.soundIconRect.Intersects(gestureRect))
            {
                this.isSound = !this.isSound;
                MediaPlayer.IsMuted = !this.isSound;
            }
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
            this.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap,
                DepthStencilState.Default, RasterizerState.CullNone);

            float oneTenthHeight = Game1.gameContentRect.Height / 10.0f;
            switch (this.gameState)
            {
                case GameState.START:
                    // Set the gameContentRect height
                    Game1.gameContentRect.Height = this.graphics.PreferredBackBufferHeight;

                    //hide ads
                    advertisement.setVisible(false);

                    //Draw background image
                    backgroundObj.Draw(spriteBatch, 0.5f);
                    this.spriteBatch.End();
                    this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                    // Height is divided among the UI components as per following logic:
                    // > 10% margin-top
                    // > 20% game title
                    // > 10% spacing
                    // > 30% playIcon with 30% marginBottom

                    // DRAW GAME TITLE
                    Vector2 titleSize = new Vector2(this.title.Width, this.title.Height);
                    if (this.title.Width > 0.9 * Game1.gameContentRect.Width)
                    {
                        titleSize = new Vector2(0.9f * Game1.gameContentRect.Width, 0.9f * Game1.gameContentRect.Width * this.title.Height / this.title.Width);
                    }
                    if (titleSize.Y > 2 * oneTenthHeight)
                    {
                        titleSize = new Vector2(titleSize.X * 2 * oneTenthHeight / titleSize.Y, 2 * oneTenthHeight);
                    }

                    Rectangle titleRect = new Rectangle(
                        (int)(Game1.gameContentRect.X + Game1.gameContentRect.Width / 2.0 - titleSize.X / 2.0),
                        (int)(Game1.gameContentRect.Y + oneTenthHeight),
                        (int)titleSize.X, (int)titleSize.Y);
                    this.spriteBatch.Draw(title, titleRect, Color.White);

                    // DRAW PLAY ICON
                    Vector2 playIconSize = new Vector2(this.playIcon.Width, this.playIcon.Height);
                    if (playIconSize.Y > /*max height for play icon*/ 3 * oneTenthHeight)
                    { // If icon height > 0.9 avail height
                        playIconSize = new Vector2(playIconSize.X * 3 * oneTenthHeight / playIconSize.Y, 3 * oneTenthHeight);
                    }
                    this.playIconStartScreenRect = new Rectangle(
                        (int)(Game1.gameContentRect.X + Game1.gameContentRect.Width / 2.0 - playIconSize.X / 2.0),
                        (int)(Game1.gameContentRect.Y + 4 * oneTenthHeight // leave marginTop
                        + (3 * oneTenthHeight - playIconSize.Y) / 2),
                        (int)playIconSize.X, (int)playIconSize.Y);
                    this.spriteBatch.Draw(this.playIcon, this.playIconStartScreenRect, Color.White);

                    this.DrawMoreIcons();
                    break;

                case GameState.RUNNING:
                    // Set the gameContentRect height
                    Game1.gameContentRect.Height = this.graphics.PreferredBackBufferHeight;

                    //hide ads
                    advertisement.setVisible(false);

                    //Draw background image
                    backgroundObj.Draw(spriteBatch, 1f);
                    this.spriteBatch.End();
                    this.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    paddleObj.Draw(spriteBatch);

                    ballObj.Draw(spriteBatch);
                    foreach (Brick brick in bricksArray)
                        brick.Draw(spriteBatch);

                    // Draw the score
                    var scoreSize = font24.MeasureString("Score: 1000");
                    this.spriteBatch.DrawString(font24, "Score: " + this.gameScore, new Vector2(
                        Game1.gameContentRect.X + Game1.gameContentRect.Width - scoreSize.X - oneTenthHeight / 10,
                        Game1.gameContentRect.Y), Color.White);

                    // Draw pause button
                    float iconSize = Game1.gameContentRect.Height / 8.0f;
                    Vector2 pauseIconSize = new Vector2(this.pauseIcon.Width, this.pauseIcon.Height);
                    if (pauseIcon.Height > iconSize)
                    {
                        pauseIconSize = new Vector2(iconSize, iconSize);
                    }
                    spriteBatch.Draw(this.pauseIcon, this.pauseIconRect = new Rectangle(Game1.gameContentRect.X, Game1.gameContentRect.Y, (int)pauseIconSize.X, (int)pauseIconSize.Y), Color.White * this.opacity);

                    if (this.isGamePaused)
                    {
                        this.DrawPausePane();
                    }

                    break;
                case GameState.END:
                    // Set the gameContentRect height as per ad unit height
                    Game1.gameContentRect.Height = this.graphics.PreferredBackBufferHeight - adUnitRect.Height;

                    //Show ads
                    advertisement.setVisible(true);

                    // Draw background image
                    backgroundObj.Draw(spriteBatch, 0.5f);
                    this.spriteBatch.End();
                    this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                    // Draw the two sections. One for score and replay icon and other for highscores.
                    Rectangle sectionOneEndScreenRect, sectionTwoEndScreenRect;
                    this.spriteBatch.Draw(this.blackTexture, sectionOneEndScreenRect = new Rectangle(
                        Game1.gameContentRect.X + (int)oneTenthHeight,
                        Game1.gameContentRect.Y + (int)oneTenthHeight,
                        (int)((Game1.gameContentRect.Width - 3 * oneTenthHeight) / 2),
                        (int)(Game1.gameContentRect.Height - 2 * oneTenthHeight)
                        ), Color.White * this.opacity);
                    this.spriteBatch.Draw(this.whiteTexture, new Rectangle(sectionOneEndScreenRect.X + sectionOneEndScreenRect.Width, sectionOneEndScreenRect.Y + (int)oneTenthHeight, 1, sectionOneEndScreenRect.Height - (int)(2 * oneTenthHeight)), Color.White * this.opacity);
                    this.spriteBatch.Draw(this.blackTexture, sectionTwoEndScreenRect = new Rectangle(
                        Game1.gameContentRect.X + (int)oneTenthHeight + sectionOneEndScreenRect.Width,
                        sectionOneEndScreenRect.Y + 1,
                        sectionOneEndScreenRect.Width,
                        sectionOneEndScreenRect.Height
                        ), Color.White * this.opacity);

                    this.spriteBatch.End();
                    this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                    this.DrawSectionOneEndScreen(sectionOneEndScreenRect);
                    this.DrawSectionTwoEndScreen(sectionTwoEndScreenRect);

                    this.DrawMoreIcons();
                    break;
                default:
                    // TODO: logging
                    break;
            }

            this.spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawSectionTwoEndScreen(Rectangle container)
        {
            // Utility variables
            int padding = container.Height / 20;

            // Draw highscore title
            string HIGHSCORE_TITLE = "Highscores";
            Vector2 titleSize = this.font36.MeasureString(HIGHSCORE_TITLE);
            Vector2 titlePosition;
            this.spriteBatch.DrawString(this.font36, HIGHSCORE_TITLE, titlePosition = new Vector2(container.X + container.Width / 2 - titleSize.X / 2, container.Y + padding), Color.White);

            // Draw highscores
            Vector2 highScorePosition = new Vector2(titlePosition.X + padding, titlePosition.Y + this.font36.LineSpacing + padding);
            List<int> highScores = this.highScores.GetHighScores();
            for (int index = 0; index < highScores.Count; index++)
            {
                int score = highScores[index];
                this.spriteBatch.DrawString(this.font24, score.ToString(), new Vector2(highScorePosition.X, highScorePosition.Y + this.font24.LineSpacing * index), score == this.gameScore ? Color.Yellow : Color.White);
            }
        }

        private void DrawSectionOneEndScreen(Rectangle container)
        {
            // Utility variables
            float oneTenthHeight = Game1.gameContentRect.Height / 10.0f;
            int padding = container.Height / 10;

            // Draw 'Score'
            Vector2 scoreSize = this.font24.MeasureString("Score: " + this.gameScore);
            this.spriteBatch.DrawString(this.font24, "Score: " + this.gameScore,
                new Vector2(container.X + container.Width / 2 - scoreSize.X / 2, container.Y + padding), Color.White);

            // Draw 'Play'
            Vector2 replayIconSize = new Vector2(this.replayIcon.Width, this.replayIcon.Height);
            if (replayIconSize.X > 3 * oneTenthHeight)// maximum size of replay icon
            {
                replayIconSize = new Vector2(3 * oneTenthHeight, 3 * oneTenthHeight);
            }
            this.replayIconEndScreenRect = new Rectangle(
                (int)(container.X + container.Width / 2.0 - replayIconSize.X / 2.0),
                (int)(container.Y + container.Height / 2.0 - (replayIconSize.Y) / 2.0),
                (int)replayIconSize.X, (int)replayIconSize.Y);
            this.spriteBatch.Draw(this.replayIcon, this.replayIconEndScreenRect, Color.White);
        }

        private void DrawPausePane()
        {
            // utility variables
            float oneTenthHeight = Game1.gameContentRect.Height / 5.0f; // this is the height/width of icons also.
            int paddingBetweenIcons = (int)oneTenthHeight / 10;

            // Draw background for icons pane
            this.spriteBatch.Draw(this.blackTexture, this.pausePaneRect = new Rectangle(Game1.gameContentRect.X, Game1.gameContentRect.Y, (int)oneTenthHeight + 2 * paddingBetweenIcons, Game1.gameContentRect.Height), new Rectangle(0, 0, 1, 1), Color.White * this.opacity);

            // Draw resume icon
            Vector2 resumeIconSize = new Vector2(this.resumeIcon.Width, this.resumeIcon.Height);
            if (this.resumeIcon.Height > oneTenthHeight)
            {
                resumeIconSize = new Vector2(oneTenthHeight, oneTenthHeight);
            }
            this.resumeIconRect = new Rectangle(
                Game1.gameContentRect.X + paddingBetweenIcons // Starting position
                + (int)(oneTenthHeight - resumeIconSize.X) / 2,
                Game1.gameContentRect.Y + paddingBetweenIcons // Starting position
                + (int)(oneTenthHeight - resumeIconSize.Y) / 2,
                (int)resumeIconSize.X, (int)resumeIconSize.Y);
            this.spriteBatch.Draw(this.resumeIcon, this.resumeIconRect, Color.White);

            // Draw replay icon
            Vector2 replayIconPausePaneSize = new Vector2(this.resumeIcon.Width, this.resumeIcon.Height);
            if (this.resumeIcon.Height > oneTenthHeight)
            {
                resumeIconSize = new Vector2(oneTenthHeight, oneTenthHeight);
            }
            this.replayIconPausePaneRect = new Rectangle(
                Game1.gameContentRect.X + paddingBetweenIcons // Starting position
                + (int)(oneTenthHeight - resumeIconSize.X) / 2,
                Game1.gameContentRect.Y + paddingBetweenIcons * 2 + (int)oneTenthHeight // Starting position
                + (int)(oneTenthHeight - resumeIconSize.Y) / 2,
                (int)resumeIconSize.X, (int)resumeIconSize.Y);
            this.spriteBatch.Draw(this.replayIcon, this.replayIconPausePaneRect, Color.White);

            // Draw sound icon
            Vector2 soundIconPausePaneSize = new Vector2(this.soundOffIcon.Width, this.soundOffIcon.Height);
            if (this.resumeIcon.Height > oneTenthHeight)
            {
                soundIconPausePaneSize = new Vector2(oneTenthHeight, oneTenthHeight);
            }
            this.spriteBatch.Draw(this.isSound ? this.soundOnIcon : this.soundOffIcon,
                this.soundIconPausePaneRect = new Rectangle(
                Game1.gameContentRect.X + paddingBetweenIcons // Starting position
                + (int)(oneTenthHeight - soundIconPausePaneSize.X) / 2,
                Game1.gameContentRect.Y + paddingBetweenIcons * 3 + (int)oneTenthHeight * 2 // Starting position
                + (int)(oneTenthHeight - soundIconPausePaneSize.Y) / 2,
                (int)soundIconPausePaneSize.X, (int)soundIconPausePaneSize.Y),
                Color.White);
        }

        private void DrawMoreIcons()
        {
            // Utility variables
            float iconSize = Game1.gameContentRect.Height / 8.0f;
            
            // DRAW MORE ICON
            // > 10% height of gameContent = height of moreIcon + 5% marginBottom
            Vector2 moreIconSize = new Vector2(this.moreIcon.Width, this.moreIcon.Height);
            if (moreIconSize.Y > iconSize)
            {
                moreIconSize = new Vector2(iconSize, iconSize);
            }
            this.moreIconRect = new Rectangle(
                (int)(Game1.gameContentRect.X + Game1.gameContentRect.Width - iconSize - iconSize / 2.0 // leaving margin top
                + (iconSize - moreIconSize.X) / 2), // takes care of padding if icon is smaller than required size but dont stretch.
                (int)(Game1.gameContentRect.Y + Game1.gameContentRect.Height - iconSize - iconSize / 2.0 // leaving margin top
                + (iconSize - moreIconSize.X) / 2),
                (int)(moreIconSize.X),
                (int)(moreIconSize.Y));
            this.spriteBatch.Draw(this.moreIcon, this.moreIconRect,
                Color.White);

            // if more icon is expanded, draw these icons from top to bottom above the more icon - rate, sound, info and more.
            if (this.isMoreIconExpanded)
            {
                int verticalPaddingBetweenIcons = (int)iconSize / 10;
                int iconHeight = this.moreIconRect.Height;

                this.spriteBatch.Draw(this.infoIcon,
                    this.infoIconRect = new Rectangle(this.moreIconRect.X, this.moreIconRect.Y - iconHeight - verticalPaddingBetweenIcons, this.moreIconRect.Width, this.moreIconRect.Height),
                    Color.White);
                this.spriteBatch.Draw(this.isSound ? this.soundOnIcon : this.soundOffIcon,
                    this.soundIconRect = new Rectangle(this.moreIconRect.X, this.moreIconRect.Y - 2 * iconHeight - 2 * verticalPaddingBetweenIcons, this.moreIconRect.Width, this.moreIconRect.Height),
                    Color.White);
                this.spriteBatch.Draw(this.rateIcon,
                    this.rateIconRect = new Rectangle(this.moreIconRect.X, this.moreIconRect.Y - 3 * iconHeight - 3 * verticalPaddingBetweenIcons, this.moreIconRect.Width, this.moreIconRect.Height),
                    Color.White);
            }
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
