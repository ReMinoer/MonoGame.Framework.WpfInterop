# MonoGame WPF Interop

This adds WPF support to MonoGame (tested with version 3.5.1.1679).

Note that you **need** MonoGame.Framework.WindowsDX (as this interop uses SharpDX). WPF only supports DirectX.

You can host as many MonoGame controls in Wpf as you want by using the WpfGame control. Note that they are however limited to 60 FPS (this is a WPF limit).

___
## Available on NuGet  [![NuGet Status](http://img.shields.io/nuget/v/MonoGame.Framework.WpfInterop.svg?style=flat)](https://www.nuget.org/packages/MonoGame.Framework.WpfInterop/)

https://nuget.org/packages/MonoGame.Framework.WpfInterop/

    PM> Install-Package MonoGame.Framework.WpfInterop
   
By adding the NuGet package to a project it is possible to host MonoGame inside WPF windows.

## Example

```csharp

public class MyGame : WpfGame
{
	private IGraphicsDeviceService _graphicsDeviceManager;
	private WpfKeyboard _keyboard;
	private WpfMouse _mouse;

	protected override void Initialize()
	{
		// must be initialized. required by Content loading and rendering (will add itself to the Services)
		_graphicsDeviceManager = new WpfGraphicsDeviceService(this);

		// wpf and keyboard need reference to the host control in order to receive input
		// this means every WpfGame control will have it's own keyboard & mouse manager which will only react if the mouse is in the control
		_keyboard = new WpfKeyboard(this);
		_mouse = new WpfMouse(this);
		
		// must be called after the WpfGraphicsDeviceService instance was created
		base.Initialize();
	}

	protected override void Update(GameTime time)
	{
		// every update we can now query the keyboard & mouse for our WpfGame
		var mouseState = _mouse.GetState();
		var keyboardState = _keyboard.GetState();
	}

	protected override void Draw(GameTime time)
	{
	}
}

```

Now you can use it in any of your WPF forms:

&lt;MyGame Width="800" Height="480" />

# Features

Some of the Monogame classes where incompatible with WPF (Game always spawns it's own window, Mouse didn't care which control had focus, ..) so they had to be reimplemented.

As a convention, all reimplemented classes will have the prefix Wpf:

* WpfGame as a replacement for Game class. Note that due to WPF limitations the WpfGame will always run at a maximum 60 FPS in fixed step (Update and Draw are always called, no Updates are skipped)
* WpfMouse and WpfKeyboard provide input per host instance. When multiple WpfGame instances are spawned, only one will receive input at any time
* WpfGraphicsDeviceService as a dummy implementation of IGraphicsDeviceService to prevent Content manager from crashing (it looks for IGraphicsDeviceService inside Services)
* WpfGameComponent and WpfDrawableGameComponent as a replacement for the original ones which required a reference to a Game instance

# Roadmap

* Properly implement GraphicsDeviceService (call all events when appropriate)

# Changelog

**v1.3.1**

* Moved to Gitlab and updated all links to Gitlab

**v1.3.0**

* Added support for GameComponents (WpfGameComponent and WpfDrawableGameComponent) that mirror the behaviour of the original ones (which cannot be used due to requiring a reference to Game)

**v1.2.2**

* fixed mistake in readme

**v1.2.1**

* Bugfix in demoscene

**v1.2.0**

* Added dependency on MonoGame.Framework.WindowsDX nuget package to prevent accidental use of different platform versions

**v1.1.1**

* Added SetCursor function to WpfMouse which now allows resetting the cursor (the monogame Mouse.SetPosition function would throw due to not finding a Winforms window)

**v1.1.0**

* New class "WpfGame" that derives from D3D11Host. It provides a cleaner interface and is more similar to the original Game class
* Input is now available via WpfMouse and WpfKeyboard

**v1.0.0**

* Initial release, can render and load content
* Input is not working yet