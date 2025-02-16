using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using DuplicateHider.Models;
using Playnite.SDK;
using Playnite.SDK.Models;

// <ContentControl x:Name="DuplicateHider_SourceSelector" DockPanel.Dock="Right" MaxHeight="{Binding ElementName=PART_ImageIcon, Path=Height}" />
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DuplicateHider")]

namespace DuplicateHider.Controls
{
    /// <summary>
    /// Interaktionslogik für DHSourceSelector.xaml
    /// </summary>
    public partial class SourceSelector : Playnite.SDK.Controls.PluginUserControl, INotifyPropertyChanged
    {

        #region Public Fields

        public static DependencyProperty MaxNumberOfIconsProperty
            = DependencyProperty.Register(nameof(MaxNumberOfIcons), typeof(Int32), typeof(SourceSelector), new PropertyMetadata(4));

        #endregion Public Fields

        #region Internal Fields

        internal static readonly UnboundedCache<ContentControl>[] ButtonCaches
                    = new UnboundedCache<ContentControl>[Constants.NUMBEROFSOURCESELECTORS];

        internal int selectorNumber = 0;

        #endregion Internal Fields

        #region Protected Fields

        protected static DropShadowEffect dropShadow = new DropShadowEffect
        {
            Color = Colors.White,
            ShadowDepth = 0,
            BlurRadius = 10
        };

        #endregion Protected Fields

        #region Public Constructors

        public SourceSelector()
        {
            InitializeComponent();
            Unloaded += SourceSelector_Unloaded;
            IsVisibleChanged += SourceSelector_IsVisibleChanged;
            IconStackPanel.Unloaded += IconStackPanel_Unloaded;
        }

        private void DHP_GameSelected(object sender, DuplicateHiderPlugin.GameSelectedArgs e)
        {
            Game currentGame = null;
            foreach (ContentControl control in IconStackPanel.Children)
            {
                if (control.DataContext is ListData data)
                {
                    data.IsCurrent = data.Game.Id == e.newId;
                    if (data.IsCurrent)
                    {
                        currentGame = data.Game;
                    }
                }
            }
            Current = currentGame;
        }

        public SourceSelector(int number, Orientation orientation = Orientation.Horizontal) : this()
        {
            selectorNumber = number;

            if (ButtonCaches[selectorNumber] == null)
            {
                ButtonCaches[selectorNumber] = new UnboundedCache<ContentControl>(CreateSourceIcon);
            }

            string key = "DuplicateHider_IconStackPanelStyle".Suffix(number);
            if (DuplicateHiderPlugin.API.Resources.
                GetResource(key) is Style style && style.TargetType == typeof(StackPanel))
            {
                IconStackPanel.SetResourceReference(StackPanel.StyleProperty, key);
            }
            else
            {
                IconStackPanel.Orientation = orientation;
            }
            if (ResourceProvider.GetResource("DuplicateHider_MaxNumberOfIcons".Suffix(number)) != null)
            {
                SetResourceReference(MaxNumberOfIconsProperty, "DuplicateHider_MaxNumberOfIcons".Suffix(number));
            }
        }

        #endregion Public Constructors

        #region Public Properties


        public event PropertyChangedEventHandler PropertyChanged;

        private Game current = null;
        public Game Current { get => current; set { current = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Current))); } }

        [Description("Orientation of the icon stack."), Category("Appearance")]
        public Orientation IconStackOrientation
        {
            get => IconStackPanel.Orientation;
            set => IconStackPanel.Orientation = value;
        }

        [Description("Maximum number of icons displayed."), Category("Appearance")]
        public Int32 MaxNumberOfIcons
        {
            get => (Int32)GetValue(MaxNumberOfIconsProperty);
            set => SetValue(MaxNumberOfIconsProperty, value);
        }

        #endregion Public Properties

        #region Public Methods

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            if (IsVisible)
            {
                UpdateGameSourceIcons(GameContext);
            }
            base.GameContextChanged(oldContext, newContext);
        }

        #endregion Public Methods

        #region Internal Methods

        internal void CreateGameSourceIcons()
        {
            if (!DuplicateHiderPlugin.Instance.settings.EnableUiIntegration)
                return;
            for (int i = 0; i < MaxNumberOfIcons; ++i)
            {
                IconStackPanel.Children.Add(ButtonCaches[selectorNumber].Get());
            }
        }

        internal void UpdateGameSourceIcons(Game context)
        {
            if (context is Game)
            {
                var games = GetGames(context).ToList();

                if (games.Count < 2 && !DuplicateHiderPlugin.Instance.settings.ShowSingleIcon)
                {
                    ButtonCaches[selectorNumber].Consume(IconStackPanel.Children);
                    return;
                }

                Game currentGame = null;

                for (int i = 0; i < games.Count; ++i)
                {
                    ContentControl button = null;
                    if (i < IconStackPanel.Children.Count)
                    {
                        button = IconStackPanel.Children[i] as ContentControl;
                    }
                    else
                    {
                        button = ButtonCaches[selectorNumber].Get();
                        IconStackPanel.Children.Add(button);
                    }
                    Game game = games[i];
                    button.Visibility = Visibility.Visible;
                    bool isCurrent = DuplicateHiderPlugin.Instance.CurrentlySelected == game.Id;
                    var listData = button.DataContext as ListData;
                    if (listData == null)
                    {
                        listData = new ListData();
                    }

                    listData.Game = game;
                    listData.IsCurrent = isCurrent;

                    if (isCurrent)
                    {
                        currentGame = game;
                    }

                    button.DataContext = null;
                    button.DataContext = listData;

                    if (button.Style == null)
                    {
                        if (button.Content is Image img)
                        {
                            img.Opacity = game.IsInstalled ? 1 : 0.5;
                        }
                    }
                }
                for (int i = IconStackPanel.Children.Count - 1; i > games.Count - 1; --i)
                {
                    ContentControl button = IconStackPanel.Children[i] as ContentControl;
                    ButtonCaches[selectorNumber].Push(button);
                    IconStackPanel.Children.RemoveAt(i);
                }

                Current = currentGame;
            }
            else
            {
                if (IconStackPanel.Children.Count > 0)
                {
                    ButtonCaches[selectorNumber].Consume(IconStackPanel.Children);
                }
            }
        }


        #endregion Internal Methods

        #region Protected Methods

        protected ImageSource GetSourceIcon(Game game)
        {
            return DuplicateHiderPlugin.SourceIconCache.GetOrGenerate(game);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (Parent is null && !(oldParent is null))
            {
                if (IconStackPanel.Children.Count > 0)
                {
                    ButtonCaches[selectorNumber].Consume(IconStackPanel.Children);
                }
            }
            base.OnVisualParentChanged(oldParent);
        }

        #endregion Protected Methods

        #region Private Methods

        private static void Bt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button bt)
            {
                ListData listData = (bt.DataContext as ListData);
                DuplicateHiderPlugin.Instance.PlayniteApi.MainView.SelectGame(listData.Game.Id);
                if (bt.Parent is ContentControl cc && cc.Parent is StackPanel sp && sp.Parent is SourceSelector ss)
                {
                    ss.Current = listData.Game;
                }
            }
        }

        private static void Bt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentControl control)
            {
                if (control.DataContext is ListData data)
                {
                    data.LaunchCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private static void Bt_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // TODO: bring up context menu for each individual game
            e.Handled = true;
        }

        private static void Icon_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ContentControl bt)
            {
                bt.Effect = dropShadow;
            }
        }

        private static void Icon_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ContentControl bt)
            {
                bt.Effect = null;
            }
        }

        private void Bt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Bt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentControl control)
            {
                if (control.DataContext is ListData data)
                {
                    if (control.Parent is StackPanel sp && sp.Parent is SourceSelector ss)
                    {
                        ss.Current = data.Game;
                    }
                    data.SelectCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private ContentControl CreateSourceIcon()
        {
            var bt = new ContentControl();
            string key = $"DuplicateHider_IconContentControlStyle".Suffix(selectorNumber);
            if (DuplicateHiderPlugin.API.Resources.
                GetResource(key) is Style style && style.TargetType == typeof(ContentControl))
            {
                bt.SetResourceReference(StyleProperty, key);
            }
            else
            {
                bt.BorderBrush = System.Windows.Media.Brushes.Transparent;
                bt.Foreground = System.Windows.Media.Brushes.Transparent;
                bt.Background = System.Windows.Media.Brushes.Transparent;
                bt.Padding = new Thickness(0);
                bt.Margin = new Thickness(2, 0, 2, 0);
                bt.BorderThickness = new Thickness(0);
                bt.HorizontalAlignment = HorizontalAlignment.Stretch;
                bt.VerticalAlignment = VerticalAlignment.Center;
                bt.MouseEnter += Icon_MouseEnter;
                bt.MouseLeave += Icon_MouseLeave;
                if (DuplicateHiderPlugin.API.ApplicationInfo.Mode == ApplicationMode.Desktop)
                {
                    var tb = new TextBox();
                    tb.SetBinding(TextBox.TextProperty, new Binding("DisplayString") { Mode = BindingMode.OneWay });
                    bt.ToolTip = tb;
                }
            }


            bt.DataContext = new ListData();

            bt.MouseLeftButtonUp += Bt_MouseLeftButtonUp;
            bt.MouseDoubleClick += Bt_MouseDoubleClick;
            bt.MouseRightButtonUp += Bt_MouseRightButtonUp;
            bt.MouseDown += Bt_MouseDown;

            Image icon = new Image()
            {
                Stretch = Stretch.Uniform
            };
            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.Fant);

            bt.Content = icon;
            icon.SetBinding(Image.SourceProperty, new Binding("Icon") { Mode = BindingMode.OneWay });
            bt.Visibility = Visibility.Collapsed;
            return bt;
        }

        private void DuplicateHider_GroupUpdated(object sender, IEnumerable<Guid> e)
        {
            if (IsVisible)
            {
                if (GameContext is Game game)
                {
                    if (e.Any(id => game.Id == id))
                    {
                        UpdateGameSourceIcons(game);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Called Group Update");
#endif
                    }
                }
            }

        }

        private IEnumerable<Game> GetGames(Game game)
        {
            if (DuplicateHiderPlugin.Instance is DuplicateHiderPlugin dh)
            {
                if (game != null)
                {
                    var copys = dh.GetCopies(game);
                    if (MaxNumberOfIcons > 0)
                        return copys.Take(MaxNumberOfIcons);
                    else
                        return copys;
                }
            }

            return new Game[] { };
        }

        private void IconStackPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel panel)
            {
                if (IconStackPanel.Children.Count > 0)
                {
                    ButtonCaches[selectorNumber].Consume(IconStackPanel.Children);
                }
            }
        }

        private void SourceSelector_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                DuplicateHiderPlugin.Instance.GroupUpdated += DuplicateHider_GroupUpdated;
                DuplicateHiderPlugin.Instance.GameSelected += DHP_GameSelected;
                UpdateGameSourceIcons(GameContext);
            }
            else
            {
                DuplicateHiderPlugin.Instance.GameSelected -= DHP_GameSelected;
                DuplicateHiderPlugin.Instance.GroupUpdated -= DuplicateHider_GroupUpdated;
                if (IconStackPanel.Children.Count > 0)
                {
                    ButtonCaches[selectorNumber].Consume(IconStackPanel.Children);
                }
            }
        }

        private void SourceSelector_Unloaded(object sender, RoutedEventArgs e)
        {
            if (e.Source is SourceSelector selector && selector.IconStackPanel.Children.Count > 0)
            {
                ButtonCaches[selectorNumber].Consume(selector.IconStackPanel.Children);
            }
        }

        #endregion Private Methods

    }
}
