﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;

namespace YTY.amt
{
  public class MainWindowViewModel : INotifyPropertyChanged
  {
    private ConfigModel config;
    private ObservableCollection<GameVersionModel> gameVersionList;
    private List<GameLanguageModel> gameLanguages;
    private ResolutionModel fullScreen = new ResolutionModel { X = (int)SystemParameters.PrimaryScreenWidth, Y = (int)SystemParameters.PrimaryScreenHeight };
    private ResolutionModel workingArea = new ResolutionModel { X = (int)(SystemParameters.WorkArea.Width - 2 * SystemParameters.FixedFrameVerticalBorderWidth), Y = (int)(SystemParameters.WorkArea.Height - 2 * SystemParameters.FixedFrameHorizontalBorderHeight) };
    
    public ConfigModel Config { get { return config; } }

    public bool WorkshopShown
    {
      set
      {
        if (value) My.WorkshopWindow.Show();
      }
    }

    public bool IsValidHawkempirePath
    {
      get
      {
        return File.Exists(Path.Combine(config.HawkempirePath, "empires2.exe"));
      }
    }

    public ObservableCollection<GameVersionModel> GameVersionList
    {
      get
      {
        if (gameVersionList == null)
        {
          gameVersionList = new ObservableCollection<GameVersionModel>(DAL.GetGameVersions());
        }
        return gameVersionList;
      }
    }

    public GameVersionModel CurrentGameVersion
    {
      get { return gameVersionList.FirstOrDefault(g => g.ResourceId == config.CurrentGameVersion); }
      set
      {
        config.CurrentGameVersion = value.ResourceId;
        OnPropertyChanged(nameof(CurrentGameVersion));
      }
    }

    public List<GameLanguageModel> GameLanguages
    {
      get
      {
        if (gameLanguages == null)
        {
          gameLanguages = new List<GameLanguageModel>()
          {
            new GameLanguageModel(){ ResourceId=-1,Name="中文" },
            new GameLanguageModel(){ ResourceId=-2,Name="英语" },
          };
        }
        return gameLanguages;
      }
    }

    public GameLanguageModel CurrentGameLanguage
    {
      get { return GameLanguages.First(g => g.ResourceId == config.CurrentGameLanguage); }
      set
      {
        config.CurrentGameLanguage = value.ResourceId;
        OnPropertyChanged(nameof(CurrentGameLanguage));
      }
    }

    public List<ResolutionModel> ScreenResolutions => Util.GetScreenResolutions();

    public ResolutionModel CurrentResolution
    {
      get { return new ResolutionModel { X = config.ResolutionX, Y = config.ResolutionY }; }
      set
      {
        config.ResolutionX = value.X;
        config.ResolutionY = value.Y;
        OnPropertyChanged(nameof(CurrentResolution));
      }
    }

    public bool FullScreen
    {
      get { return fullScreen.X == config.ResolutionX && fullScreen.Y == config.ResolutionY; }
      set
      {
        config.ResolutionX = fullScreen.X;
        config.ResolutionY = fullScreen.Y;
      }
    }

    public bool WorkingArea
    {
      get { return workingArea.X == config.ResolutionX && workingArea.Y == config.ResolutionY; }
      set
      {
        config.ResolutionX = workingArea.X;
        config.ResolutionY = workingArea.Y;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public MainWindowViewModel()
    {
      config = ConfigModel.CurrentConfig;
      config.PropertyChanged += Config_PropertyChanged;
    }

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(ConfigModel.HawkempirePath):
          OnPropertyChanged(nameof(IsValidHawkempirePath));
          break;
        case nameof(ConfigModel.ResolutionX):
        case nameof(ConfigModel.ResolutionY):
          OnPropertyChanged(nameof(CurrentResolution));
          OnPropertyChanged(nameof(FullScreen));
          OnPropertyChanged(nameof(WorkingArea));
          break;
      }
    }
  }
}