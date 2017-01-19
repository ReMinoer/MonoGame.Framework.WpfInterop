using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.WpfInterop
{
    public class DrawingEventArgs : EventArgs
    {
        public GameTime GameTime { get; }

        public DrawingEventArgs(GameTime gameTime)
        {
            GameTime = gameTime;
        }
    }
}