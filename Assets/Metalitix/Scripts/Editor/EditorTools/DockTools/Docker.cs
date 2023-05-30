using System;
using System.Reflection;
using UnityEngine;

namespace Metalitix.Scripts.Editor.EditorTools.DockTools
{
    public static class Docker
    {

        #region Reflection Types
        private class EditorWindow
        {
            private UnityEditor.EditorWindow instance;
            private Type type;

            public EditorWindow( UnityEditor.EditorWindow instance ) {
                this.instance = instance;
                type = instance.GetType();
            }

            public object m_Parent {
                get {
                    var field = type.GetField( "m_Parent", BindingFlags.Instance | BindingFlags.NonPublic );
                    return field.GetValue( instance );
                }
            }
        }

        private class DockArea
        {
            private object instance;
            private Type type;

            public DockArea( object instance ) {
                this.instance = instance;
                type = instance.GetType();
            }

            public object window {
                get {
                    var property = type.GetProperty( "window", BindingFlags.Instance | BindingFlags.Public );
                    return property.GetValue( instance, null );
                }
            }

            public object s_OriginalDragSource {
                set {
                    var field = type.GetField( "s_OriginalDragSource", BindingFlags.Static | BindingFlags.NonPublic );
                    field.SetValue( null, value );
                }
            }
        }

        private class ContainerWindow
        {
            private object instance;
            private Type type;

            public ContainerWindow( object instance ) {
                this.instance = instance;
                type = instance.GetType();
            }


            public object rootSplitView {
                get {
                    var property = type.GetProperty( "rootSplitView", BindingFlags.Instance | BindingFlags.Public );
                    return property.GetValue( instance, null );
                }
            }
        }

        private class SplitView
        {
            private object instance;
            private Type type;

            public SplitView( object instance ) {
                this.instance = instance;
                type = instance.GetType();
            }

            public object DragOver( UnityEditor.EditorWindow child, Vector2 screenPoint ) {
                var method = type.GetMethod( "DragOver", BindingFlags.Instance | BindingFlags.Public );
                return method.Invoke( instance, new object[] { child, screenPoint } );
            }

            public void PerformDrop( UnityEditor.EditorWindow child, object dropInfo, Vector2 screenPoint ) {
                var method = type.GetMethod( "PerformDrop", BindingFlags.Instance | BindingFlags.Public );
                method.Invoke( instance, new object[] { child, dropInfo, screenPoint } );
            }
        }
        #endregion

        public enum DockPosition
        {
            Left,
            Top,
            Right,
            Bottom
        }

        /// <summary>
        /// Docks the second window to the first window at the given position
        /// </summary>
        public static void Dock(this UnityEditor.EditorWindow wnd, UnityEditor.EditorWindow other, DockPosition position ) {
            var mousePosition = GetFakeMousePosition( wnd, position );

            if(wnd.docked) return;

            var parent = new EditorWindow( wnd );
            var child = new EditorWindow( other );
            var dockArea = new DockArea( parent.m_Parent );
            var containerWindow = new ContainerWindow( dockArea.window );
            var splitView = new SplitView( containerWindow.rootSplitView );
            var dropInfo = splitView.DragOver( other, mousePosition );
            dockArea.s_OriginalDragSource = child.m_Parent;
            splitView.PerformDrop( other, dropInfo, mousePosition );
        }

        private static Vector2 GetFakeMousePosition(UnityEditor.EditorWindow wnd, DockPosition position ) {
            Vector2 mousePosition = Vector2.zero;

            // The 20 is required to make the docking work.
            // Smaller values might not work when faking the mouse position.
            switch ( position ) {
                case DockPosition.Left:
                    mousePosition = new Vector2( 20, wnd.position.size.y / 2 );
                    break;
                case DockPosition.Top:
                    mousePosition = new Vector2( wnd.position.size.x / 2, 20 );
                    break;
                case DockPosition.Right:
                    mousePosition = new Vector2( wnd.position.size.x - 20, wnd.position.size.y / 2 );
                    break;
                case DockPosition.Bottom:
                    mousePosition = new Vector2( wnd.position.size.x / 2, wnd.position.size.y - 20 );
                    break;
            }

            return GUIUtility.GUIToScreenPoint( mousePosition );
        }
    }
}
