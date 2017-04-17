﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Diagnostics;
using YTY.DrsLib;

namespace YTY.amt.Model
{
  public class ModResourceModel : WorkshopResourceModel
  {
    /// <summary>
    /// Dummy ID for age2_x1.0c.exe
    /// </summary>
    public const int AGE2_1C = -2;

    private static readonly Regex regexXmlPath = new Regex(@"Games\\\w+\.xml", RegexOptions.IgnoreCase);
    private static readonly Regex regexExePath = new Regex(@"age2_x1\\\w+\.exe", RegexOptions.IgnoreCase);
    private static readonly string[] modOpenFolders_AGE2_1C ={
      "Campaign",
      "Scenario",
      "AI",
      "SaveGame",
      @"Sound\Scenario",
      "Data",
      "Random",
      "Screenshots",};
    private static readonly string[] modOpenFolders ={
      "Scenario",
      "Scenario",
      "Script.AI",
      "SaveGame",
      @"Sound\Scenario",
      "Data",
      "Script.RM",
      "Screenshots" };

    public int Index => ProgramModel.Mods.IndexOf(this);

    public string ExePath { get; set; }

    public string XmlPath { get; set; }

    public string FolderPath { get; set; }

    public bool CanMoveUp => Index > 0;

    public bool CanMoveDown => -1 < Index && Index < ProgramModel.Mods.Count - 1;

    protected override void AfterDownload()
    {
      try
      {
        XmlPath = Files.First(f => regexXmlPath.IsMatch(f.Path)).Path;
      }
      catch (InvalidOperationException)
      {
        throw new InvalidOperationException("该 MOD 缺少 XML 配置文件，本资源属于无效资源，请联系上传者。");
      }

      try
      {
        ExePath = Files.First(f => regexExePath.IsMatch(f.Path)).Path;
      }
      catch (InvalidOperationException)
      {
        throw new InvalidOperationException("该 MOD 缺少游戏主程序 EXE 文件，本资源属于无效资源，请联系上传者。");
      }

      var root = XElement.Load(ProgramModel.MakeHawkempirePath(XmlPath));
      FolderPath = @"Games\" + (root.Element("path")?.Value ?? throw new InvalidOperationException("该 MOD 的 XML 配置文件格式错误，本资源属于无效资源，请联系上传者。"));
      ProgramModel.Mods.Add(this);
    }

    public void MoveUp()
    {
      var index = Index;
      ProgramModel.Mods.Move(index, index - 1);
    }

    public void MoveDown()
    {
      var index = Index;
      ProgramModel.Mods.Move(index, index + 1);
    }

    public void OpenFolder(ModOpenFolder which)
    {
      var folder = ProgramModel.MakeHawkempirePath(Path.Combine(FolderPath,
        Id == AGE2_1C ? modOpenFolders_AGE2_1C[(int)which] : modOpenFolders[(int)which]));
      Directory.CreateDirectory(folder);
      Process.Start(folder);
    }

    public void CopyExe()
    {
      File.Copy(ProgramModel.MakeHawkempirePath(ExePath), ProgramModel.MakeHawkempirePath(@"age2_x1\age2_x1.exe"),true);
    }

    public void Run()
    {
      CopyExe();
      ApplyDrses();
      Process.Start(ProgramModel.MakeHawkempirePath(@"age2_x1\age2_x1.exe"));
    }

    private void ApplyDrses()
    {
      var dic = builtInDrsFiles.ToDictionary(f => f, f => DrsFile.Load(ProgramModel.MakeHawkempirePath(f)));
      foreach (var drs in ProgramModel.ActiveDrses.Reverse())
      {
        foreach (var file in drs.Files)
        {
          var extension = Path.GetExtension(file.Path).TrimStart('.').ToLowerInvariant();
          var id = int.Parse(Path.GetFileNameWithoutExtension(file.Path));
          var drsName = Path.GetFileName(Path.GetDirectoryName(file.Path)).ToLowerInvariant();
          dic[drsName][(DrsTableClass)Array.IndexOf(drsTables, extension)][id] = File.ReadAllBytes(ProgramModel.MakeHawkempirePath(file.Path));
        }
      }
    }

    internal static readonly ModResourceModel[] BuiltInGames =
    {
      new ModResourceModel
      {
        Id = -1,
        Name = "帝国时代Ⅱ 1.5",
        ExePath = @"Manager\exe\age2_x1.5.exe",
        FolderPath=@"Games\The Conquerors 1.4",
      },
      new ModResourceModel
      {
        Id = AGE2_1C,
        Name = "帝国时代Ⅱ 1.0C",
        ExePath = @"Manager\exe\age2_x1.0c.exe",
        FolderPath="",
      },
      new ModResourceModel
      {
        Id = -3,
        Name = "被遗忘的帝国",
        ExePath = @"Manager\exe\age2_x2.exe",
        FolderPath=@"Games\Forgotten Empires",
      },
      new ModResourceModel
      {
        Id = -4,
        Name = "WAIFor 触发扩展版",
        ExePath = @"Manager\exe\age2_wtep.exe",
        FolderPath=@"Games\The Conquerors 1.4",
      },
    };

    private static readonly string[] builtInDrsFiles =
    {
      "gamedata.drs",
      "graphics.drs",
      "interfac.drs",
      "sounds.drs",
      "terrain.drs",
    };

    private static readonly string[] drsTables =
    {
      "bina","shp","slp","wav",
    };
  }

  public enum ModOpenFolder
  {
    Campaign,
    Scenario,
    Ai,
    SaveGame,
    ScenarioSound,
    Data,
    Rms,
    Screenshot,
  }
}