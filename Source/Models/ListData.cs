﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Playnite.SDK;
using Playnite.SDK.Models;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DuplicateHider")]
namespace DuplicateHider.Models
{
    public class ListData : DependencyObject
    {
        public ImageSource Icon => DuplicateHiderPlugin.SourceIconCache.GetOrGenerate(Game);
        public Game Game
        {
            get => (Game)GetValue(GameProperty);
            set => SetValue(GameProperty, value);
        }
        public String SourceName => Game?.Source?.Name ?? Constants.UNDEFINED_SOURCE;
        public String DisplayString => DuplicateHiderPlugin.Instance.ExpandDisplayString(Game, DuplicateHiderPlugin.Instance.settings.DisplayString);
        public Boolean IsCurrent
        {
            get => (Boolean)GetValue(IsCurrentProperty);
            set => SetValue(IsCurrentProperty, value);
        }
        public ICommand LaunchCommand { get; set; }
        public ICommand SelectCommand { get; set; }
        public ICommand InstallCommand { get; set; }
        public ICommand UninstallCommand { get; set; }

        public ListData()
        {
            LaunchCommand = new RelayCommand(() => DuplicateHiderPlugin.API.StartGame(Game.Id));
            SelectCommand = new RelayCommand(() => DuplicateHiderPlugin.Instance.SelectGame(Game.Id));
            InstallCommand = new RelayCommand(() => DuplicateHiderPlugin.API.InstallGame(Game.Id));
            UninstallCommand = new RelayCommand(() => DuplicateHiderPlugin.API.InstallGame(Game.Id));
        }

        public ListData(Game game, bool current,
            ICommand launchCommand = null,
            ICommand selectCommand = null,
            ICommand installCommand = null,
            ICommand uninstallCommand = null)
            : this(DuplicateHiderPlugin.SourceIconCache.GetOrGenerate(game), game, current, launchCommand, selectCommand, installCommand, uninstallCommand)
        {

        }

        public ListData(ImageSource image, Game game, bool current = false,
            ICommand launchCommand = null,
            ICommand selectCommand = null,
            ICommand installCommand = null,
            ICommand uninstallCommand = null)
        {
            //Icon = image;
            Game = game;
            IsCurrent = current;
            //SourceName = game.Source?.Name ?? Constants.UNDEFINED_SOURCE;
            //DisplayString = DuplicateHiderPlugin.Instance.ExpandDisplayString(game, DuplicateHiderPlugin.Instance.settings.DisplayString);
            LaunchCommand = launchCommand ?? new RelayCommand(() => DuplicateHiderPlugin.API.StartGame(Game.Id));
            SelectCommand = selectCommand ?? new RelayCommand(() => DuplicateHiderPlugin.Instance.SelectGame(Game.Id));
            InstallCommand = installCommand ?? new RelayCommand(() => DuplicateHiderPlugin.API.InstallGame(Game.Id));
            UninstallCommand = uninstallCommand ?? new RelayCommand(() => DuplicateHiderPlugin.API.InstallGame(Game.Id));
        }


        public static readonly DependencyProperty IsCurrentProperty
            = DependencyProperty.Register(nameof(IsCurrent), typeof(Boolean), typeof(ListData), new PropertyMetadata(false));
        public static readonly DependencyProperty GameProperty
            = DependencyProperty.Register(nameof(Game), typeof(Game), typeof(ListData), new PropertyMetadata(null));
        //public static readonly DependencyProperty IconProperty
        //    = DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(ListData), new PropertyMetadata(null));
        //public static readonly DependencyProperty SourceNameProperty
        //    = DependencyProperty.Register(nameof(SourceName), typeof(String), typeof(ListData), new PropertyMetadata("Playnite"));
    }
}
