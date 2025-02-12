using System.Windows;
using System.Windows.Controls;
using Playnite.SDK.Models;

namespace DuplicateHider.Controls
{
    /// <summary>
    /// Interaktionslogik für DHWrapper.xaml
    /// </summary>
    public partial class DHWrapper : Playnite.SDK.Controls.PluginUserControl
    {
        public DHWrapper()
        {
            InitializeComponent();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (DH_ContentControl.Style == null)
            {
                if (Parent is ContentControl parent)
                {
                    if (parent.Tag is Style style)
                    {
                        DH_ContentControl.Style = style;
                    }
                }
            }
            base.OnVisualParentChanged(oldParent);
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            DH_ContentControl.GameContext = newContext;
            DH_ContentControl.GameContextChanged(oldContext, newContext);
            base.GameContextChanged(oldContext, newContext);
        }

    }
}
