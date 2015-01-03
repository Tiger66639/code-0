// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TiltWheelProvider.cs" company="">
//   
// </copyright>
// <summary>
//   a <see langword="delegate" /> for mouse tilt wheel events.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     a <see langword="delegate" /> for mouse tilt wheel events.
    /// </summary>
    public delegate void MouseTiltEventHandler(object sender, MouseTiltEventArgs e);

    /// <summary>
    ///     The arguments of the event.
    /// </summary>
    public class MouseTiltEventArgs : System.Windows.RoutedEventArgs
    {
        /// <summary>Gets or sets the tilt.</summary>
        public int Tilt { get; set; }

        /// <summary>Gets or sets the keys.</summary>
        public int Keys { get; set; }

        /// <summary>Gets or sets the x.</summary>
        public int X { get; set; }

        /// <summary>Gets or sets the y.</summary>
        public int Y { get; set; }
    }

    /// <summary>
    ///     A helper class to implement the mouse tilt wheel. Use a new object for
    ///     each window.
    /// </summary>
    /// <remarks>
    ///     You need a new object for each window since this object hooks into the
    ///     window event procedure and can therefor only <see langword="catch" />
    ///     events for a single window.
    /// </remarks>
    public class TiltWheelProvider : System.Windows.DependencyObject
    {
        #region Fields

        /// <summary>The f is hooked.</summary>
        private bool fIsHooked; // this is to find out if we have already hooked up with the parent window or not. 

        #endregion

        #region internal types

        /// <summary>The win 32 messages.</summary>
        private abstract class Win32Messages
        {
            /// <summary>The w m_ mousehwheel.</summary>
            public const int WM_MOUSEHWHEEL = 0x020E;
        }

        /// <summary>The utils.</summary>
        private abstract class Utils
        {
            /// <summary>The hiword.</summary>
            /// <param name="ptr">The ptr.</param>
            /// <returns>The <see cref="int"/>.</returns>
            internal static int HIWORD(System.IntPtr ptr)
            {
                var val32 = ptr.ToInt32();
                return (val32 >> 16) & 0xFFFF;
            }

            /// <summary>The loword.</summary>
            /// <param name="ptr">The ptr.</param>
            /// <returns>The <see cref="int"/>.</returns>
            internal static int LOWORD(System.IntPtr ptr)
            {
                var val32 = ptr.ToInt32();
                return val32 & 0xFFFF;
            }
        }

        #endregion

        #region TiltWheelHandler

        /// <summary>
        ///     TiltWheelHandler Attached Dependency Property.
        /// </summary>
        public static readonly System.Windows.DependencyProperty TiltWheelHandlerProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "TiltWheelHandler", 
                typeof(TiltWheelProvider), 
                typeof(TiltWheelProvider), 
                new System.Windows.FrameworkPropertyMetadata(null, OnTiltWheelHandlerChanged));

        /// <summary>Gets the TiltWheelHandler property. This attached property indicates
        ///     the object to use for handling mouse wheel tilt events. Best to attach
        ///     to a window.</summary>
        /// <param name="d">The d.</param>
        /// <returns>The <see cref="TiltWheelProvider"/>.</returns>
        public static TiltWheelProvider GetTiltWheelHandler(System.Windows.DependencyObject d)
        {
            return (TiltWheelProvider)d.GetValue(TiltWheelHandlerProperty);
        }

        /// <summary>Sets the TiltWheelHandler property. This attached property indicates
        ///     the object to use for handling mouse wheel tilt events.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetTiltWheelHandler(System.Windows.DependencyObject d, TiltWheelProvider value)
        {
            d.SetValue(TiltWheelHandlerProperty, value);
        }

        /// <summary>Handles changes to the TiltWheelHandler property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnTiltWheelHandlerChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var iProvider = (TiltWheelProvider)e.NewValue;
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background, 
                    new System.Action<System.Windows.DependencyObject, TiltWheelProvider>(AttachHookAsync), 
                    d, 
                    iProvider);
            }
        }

        /// <summary>Attaches the hook async. We do this cause if this is called when the
        ///     app is loaded, the window might not yet be loaded completely and
        ///     therefor not yet have a handle, so we do async.</summary>
        /// <param name="attachTo">The attach to.</param>
        /// <param name="toAttach">To attach.</param>
        private static void AttachHookAsync(System.Windows.DependencyObject attachTo, TiltWheelProvider toAttach)
        {
            if (toAttach.fIsHooked == false)
            {
                var iWin = System.Windows.Window.GetWindow(attachTo);
                if (iWin != null && iWin.IsLoaded)
                {
                    // could be something went wrong while trying to load the window.
                    toAttach.fIsHooked = true;
                    var source =
                        System.Windows.Interop.HwndSource.FromHwnd(
                            new System.Windows.Interop.WindowInteropHelper(iWin).Handle);
                    source.AddHook(toAttach.WndProc);
                }
            }
        }

        #endregion

        #region TiltWheel

        /// <summary>
        ///     TiltWheel Attached Routed Event
        /// </summary>
        public static readonly System.Windows.RoutedEvent TiltWheelEvent =
            System.Windows.EventManager.RegisterRoutedEvent(
                "TiltWheel", 
                System.Windows.RoutingStrategy.Bubble, 
                typeof(MouseTiltEventHandler), 
                typeof(TiltWheelProvider));

        /// <summary>Adds a <paramref name="handler"/> for the TiltWheel attached event</summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be added</param>
        public static void AddTiltWheelHandler(System.Windows.DependencyObject element, MouseTiltEventHandler handler)
        {
            AddHandler(element, TiltWheelEvent, handler);
        }

        /// <summary>Removes a <paramref name="handler"/> for the TiltWheel attached event</summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="handler">Event handler to be removed</param>
        public static void RemoveTiltWheelHandler(
            System.Windows.DependencyObject element, 
            MouseTiltEventHandler handler)
        {
            RemoveHandler(element, TiltWheelEvent, handler);
        }

        /// <summary>A <see langword="static"/> helper method to raise the TiltWheel event
        ///     on a <paramref name="target"/> element.</summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        /// <param name="values">the values for the tilt event</param>
        /// <returns>The <see cref="MouseTiltEventArgs"/>.</returns>
        internal static MouseTiltEventArgs RaiseTiltWheelEvent(
            System.Windows.DependencyObject target, 
            MouseTiltEventArgs values)
        {
            if (target == null)
            {
                return null;
            }

            values.RoutedEvent = TiltWheelEvent;
            RaiseEvent(target, values);
            return values;
        }

        #endregion

        #region functions

        /// <summary>The window proc that handles the actual window message.</summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="wParam">The w param.</param>
        /// <param name="lParam">The l param.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        /// <returns>The <see cref="IntPtr"/>.</returns>
        private System.IntPtr WndProc(
            System.IntPtr hwnd, 
            int msg, 
            System.IntPtr wParam, 
            System.IntPtr lParam, 
            ref bool handled)
        {
            switch (msg)
            {
                case Win32Messages.WM_MOUSEHWHEEL:
                    MouseWheelTilt(wParam, lParam);
                    handled = true;
                    break;
                default:
                    break;
            }

            return System.IntPtr.Zero;
        }

        /// <summary>The mouse wheel tilt.</summary>
        /// <param name="wParam">The w param.</param>
        /// <param name="lParam">The l param.</param>
        private void MouseWheelTilt(System.IntPtr wParam, System.IntPtr lParam)
        {
            int tilt = (System.Int16)Utils.HIWORD(wParam);
            var keys = Utils.LOWORD(wParam);
            var x = Utils.LOWORD(lParam);
            var y = Utils.HIWORD(lParam);

            var iInstanceUnderMouse = System.Windows.Input.Mouse.DirectlyOver as System.Windows.DependencyObject;

            // call an event on active instance of this object
            if (iInstanceUnderMouse != null)
            {
                var iArgs = new MouseTiltEventArgs();
                iArgs.X = x;
                iArgs.Y = y;
                iArgs.Keys = keys;
                iArgs.Tilt = tilt;
                RaiseTiltWheelEvent(iInstanceUnderMouse, iArgs);
            }
        }

        #endregion

        #region RoutedEvent Helper Methods

        /// <summary>A <see langword="static"/> helper method to raise a routed event on a<paramref name="target"/> UIElement or ContentElement.</summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        /// <param name="args">RoutedEventArgs to use when raising the event</param>
        private static void RaiseEvent(System.Windows.DependencyObject target, System.Windows.RoutedEventArgs args)
        {
            if (target is System.Windows.UIElement)
            {
                (target as System.Windows.UIElement).RaiseEvent(args);
            }
            else if (target is System.Windows.ContentElement)
            {
                (target as System.Windows.ContentElement).RaiseEvent(args);
            }
        }

        /// <summary>A <see langword="static"/> helper method that adds a<paramref name="handler"/> for a routed event to a target UIElement or
        ///     ContentElement.</summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="routedEvent">Event that will be handled</param>
        /// <param name="handler">Event handler to be added</param>
        private static void AddHandler(
            System.Windows.DependencyObject element, 
            System.Windows.RoutedEvent routedEvent, 
            System.Delegate handler)
        {
            var uie = element as System.Windows.UIElement;
            if (uie != null)
            {
                uie.AddHandler(routedEvent, handler);
            }
            else
            {
                var ce = element as System.Windows.ContentElement;
                if (ce != null)
                {
                    ce.AddHandler(routedEvent, handler);
                }
            }
        }

        /// <summary>A <see langword="static"/> helper method that removes a<paramref name="handler"/> for a routed event from a target UIElement
        ///     or ContentElement.</summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="routedEvent">Event that will no longer be handled</param>
        /// <param name="handler">Event handler to be removed</param>
        private static void RemoveHandler(
            System.Windows.DependencyObject element, 
            System.Windows.RoutedEvent routedEvent, 
            System.Delegate handler)
        {
            var uie = element as System.Windows.UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(routedEvent, handler);
            }
            else
            {
                var ce = element as System.Windows.ContentElement;
                if (ce != null)
                {
                    ce.RemoveHandler(routedEvent, handler);
                }
            }
        }

        #endregion
    }
}