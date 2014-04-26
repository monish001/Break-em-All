using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
namespace Break_em_All
{
    class Paddle
    {
        Vector2 position;
        Vector2 motion;
        float paddleSpeed = 30f;
        //KeyboardState keyboardState;
        //GamePadState gamePadState;
        Accelerometer accelSensor;
        Boolean accelActive = false;
        Vector3 accelReading = new Vector3();
        Texture2D texture;
        Rectangle screenBounds;
        public Paddle(Texture2D texture, Rectangle screenBounds)
        {
            this.texture = texture;
            this.screenBounds = screenBounds;

            this.setupAccelerometer();

            SetInStartPosition();
        }

        private void setupAccelerometer()
        {
            accelSensor = new Accelerometer();

            // Add the accelerometer event handler to the accelerometer sensor.
            accelSensor.ReadingChanged +=
                new EventHandler<AccelerometerReadingEventArgs>(AccelerometerReadingChanged);

            // Start the accelerometer
            try
            {
                accelSensor.Start();
                accelActive = true;
            }
            catch (AccelerometerFailedException e)
            {
                // the accelerometer couldn't be started.  No fun!
                accelActive = false;
            }
            catch (UnauthorizedAccessException e)
            {
                // This exception is thrown in the emulator-which doesn't support an accelerometer.
                accelActive = false;
            }
        }

        public void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            accelReading.X = (float)e.X;
            accelReading.Y = (float)e.Y;
            accelReading.Z = (float)e.Z;
        }

        public void Update(Vector2 delta)
        {
            motion = Vector2.Zero;
            motion.X = delta.X;
            //keyboardState = Keyboard.GetState();
            //gamePadState = GamePad.GetState(PlayerIndex.One);
            //if (
            //    keyboardState.IsKeyDown(Keys.Left) ||
            //gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft) ||
            //gamePadState.IsButtonDown(Buttons.DPadLeft) ||
            //accelReading.Y > 0
            //    )
            //    motion.X = -1;

            //if (
            //    keyboardState.IsKeyDown(Keys.Right) ||
            //gamePadState.IsButtonDown(Buttons.LeftThumbstickRight) ||
            //gamePadState.IsButtonDown(Buttons.DPadRight) ||
            //accelReading.Y < 0
            //    )
            //    motion.X = 1;

            //if (accelReading.X * accelReading.Y < 0)
            //    motion.X = -1;
            //else
            //    motion.X = 1;

            //var x = accelReading.Y;
            //var factor = (float)(-5 / System.Math.Log(x < 0 ? -x : x));// |-k / logx|
            //var factor2 = x * x * x * (x < 0 ? -1 : 1);// |x^3|
            //var factor3 = paddleSpeed * x;

            //motion.X *= (float.IsNaN(factor) ? 0 : factor);//paddleSpeed;
            position += motion;
            LockPaddle();
        }

        private void LockPaddle()
        {
            if (position.X < screenBounds.X)
                position.X = screenBounds.X;
            if (position.X + texture.Width > screenBounds.X + screenBounds.Width)
                position.X = screenBounds.X + screenBounds.Width - texture.Width;
        }

        public void SetInStartPosition()
        {
            position.X = (screenBounds.Width - texture.Width) / 2;
            position.Y = screenBounds.Height - texture.Height - 5;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
            (int)position.X,
            (int)position.Y,
            texture.Width,
            texture.Height);
        }

        internal void Dispose()
        {
            // Stop the accelerometer if it's active.
            if (accelActive)
            {
                try
                {
                    accelSensor.Stop();
                }
                catch (AccelerometerFailedException e)
                {
                    // the accelerometer couldn't be stopped now.
                }
            }
        }
    }
}
