using System;

namespace MonoGame.Framework.WpfInterop
{
    public interface IGameRunner
    {
        event EventHandler<DrawingEventArgs> Drawing;
        event EventHandler Disposed;
    }
}