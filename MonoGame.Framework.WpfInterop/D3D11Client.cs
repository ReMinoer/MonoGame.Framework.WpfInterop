using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MonoGame.Framework.WpfInterop
{
    public abstract class D3D11Client : Image, IDisposable
	{
        #region Fields

        private static readonly object _graphicsDeviceLock = new object();

        private static bool? _isInDesignMode;

		private D3D11Image _d3D11Image;
		private bool _disposed;
        private static int _referenceCount;

        public RenderTarget2D RenderTarget { get; private set; }
		private bool _resetBackBuffer;
	    private bool _isRendering;

        private IGameRunner _runner;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="D3D11Host"/> class.
        /// </summary>
        protected D3D11Client()
        {
            // defaulting to fill as that's what's needed in most cases
            Stretch = Stretch.Fill;

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
            Initialized += OnInitialized;
		}

	    #endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether the controls runs in the context of a designer (e.g.
		/// Visual Studio Designer or Expression Blend).
		/// </summary>
		/// <value>
		/// <see langword="true" /> if controls run in design mode; otherwise, 
		/// <see langword="false" />.
		/// </value>
		public static bool IsInDesignMode
		{
			get
			{
				if (!_isInDesignMode.HasValue)
					_isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;

				return _isInDesignMode.Value;
			}
		}

        static private GraphicsDevice _graphicsDevice;

        static public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (_graphicsDevice == null)
                {
                    InitializeGraphicsDevice();
                }

                return _graphicsDevice;
            }
            set
            {
                if (_graphicsDevice != null)
                    throw new InvalidOperationException();

                _graphicsDevice = value;
                IsGraphicsDeviceInitialized = true;
            }
        }

	    static public bool IsGraphicsDeviceInitialized { get; private set; }

	    public IGameRunner Runner
        {
            get { return _runner; }
            set
            {
                if (_runner == value)
                    return;

                if (_runner != null)
                {
                    _runner.Drawing -= RunnerOnDrawing;
                    _runner.Disposed -= RunnerOnDisposed;
                }

                _runner = value;

                if (_runner != null)
                {
                    _runner.Drawing += RunnerOnDrawing;
                    _runner.Disposed += RunnerOnDisposed;
                }
            }
        }

        #endregion

        #region Methods

        public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			Dispose(true);
            UnitializeImageSource();
            UninitializeGraphicsDevice();
        }

	    protected virtual void Dispose(bool disposing)
	    {
	    }

        private void OnInitialized(object sender, EventArgs eventArgs)
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            InitializeGraphicsDevice();
            InitializeImageSource();
        }

		/// <summary>
		/// Raises the <see cref="FrameworkElement.SizeChanged" /> event, using the specified 
		/// information as part of the eventual event data.
		/// </summary>
		/// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			_resetBackBuffer = true;
			base.OnRenderSizeChanged(sizeInfo);
        }

        private static void InitializeGraphicsDevice()
        {
            if (_graphicsDevice != null)
                return;

            lock (_graphicsDeviceLock)
            {
                _referenceCount++;
                if (_referenceCount == 1)
                {
                    // Create Direct3D 11 device.
                    var presentationParameters = new PresentationParameters
                    {
                        // Do not associate graphics device with window.
                        DeviceWindowHandle = IntPtr.Zero,
                    };
                    _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);
                }
            }

            IsGraphicsDeviceInitialized = true;
        }

        private static void UninitializeGraphicsDevice()
        {
            if (_referenceCount == 0)
                return;

            lock (_graphicsDeviceLock)
            {
                _referenceCount--;
                if (_referenceCount == 0)
                {
                    _graphicsDevice.Dispose();
                    _graphicsDevice = null;
                }
            }
        }

        private void CreateBackBuffer()
		{
			_d3D11Image.SetBackBuffer(null);
			if (RenderTarget != null)
			{
				RenderTarget.Dispose();
				RenderTarget = null;
			}

			int width = Math.Max((int)ActualWidth, 1);
			int height = Math.Max((int)ActualHeight, 1);
			RenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Bgr32, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents, true);
			_d3D11Image.SetBackBuffer(RenderTarget);
		}

		private void InitializeImageSource()
        {
            _d3D11Image = new D3D11Image();
            _d3D11Image.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
            CreateBackBuffer();
            Source = _d3D11Image;
        }

		private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
		{
			if (_d3D11Image.IsFrontBufferAvailable)
			{
				StartRendering();
				_resetBackBuffer = true;
			}
			else
			{
				StopRendering();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs eventArgs)
		{
			if (IsInDesignMode)
				return;
            
			StartRendering();
		}

		private void RunnerOnDrawing(object sender, DrawingEventArgs e)
		{
			if (!IsLoaded || !_isRendering)
				return;

			// Recreate back buffer if necessary.
			if (_resetBackBuffer)
				CreateBackBuffer();
            
			GraphicsDevice.SetRenderTarget(RenderTarget);
			Runner.Draw(this, e.GameTime);
			GraphicsDevice.Flush();

			_d3D11Image.Invalidate(); // Always invalidate D3DImage to reduce flickering
									  // during window resizing.

			_resetBackBuffer = false;
        }

        private void RunnerOnDisposed(object sender, EventArgs eventArgs)
        {
            _runner = null;
        }

        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
		{
			if (IsInDesignMode)
				return;

			StopRendering();
        }

		private void StartRendering()
		{
		    _isRendering = true;
		}

		private void StopRendering()
        {
            _isRendering = false;
        }

		private void UnitializeImageSource()
		{
			Source = null;

			if (_d3D11Image != null)
            {
                _d3D11Image.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
                _d3D11Image.Dispose();
				_d3D11Image = null;
			}
			if (RenderTarget != null)
			{
				RenderTarget.Dispose();
				RenderTarget = null;
			}
		}

		#endregion
	}
}