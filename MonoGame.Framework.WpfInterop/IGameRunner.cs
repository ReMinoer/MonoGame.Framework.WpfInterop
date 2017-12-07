using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.WpfInterop
{
    public interface IGameRunner : IDisposable
    {
        void Draw(D3D11Client client, GameTime gameTime);
        event EventHandler<DrawingEventArgs> Drawing;
        event EventHandler Disposed;
    }
}