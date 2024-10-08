﻿using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Script;
using BetterGenshinImpact.Core.Script.Group;
using BetterGenshinImpact.Core.Script.Project;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.AutoPathing;
using BetterGenshinImpact.GameTask.AutoPathing.Model;
using BetterGenshinImpact.GameTask.Model.Enum;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls;

namespace BetterGenshinImpact.ViewModel.Pages;

public partial class MapPathingViewModel(IScriptService scriptService) : ObservableObject, INavigationAware, IViewModel
{
    private readonly ILogger<MapPathingViewModel> _logger = App.GetLogger<MapPathingViewModel>();
    public static readonly string PathJsonPath = Global.Absolute(@"User\AutoPathing");

    [ObservableProperty]
    private ObservableCollection<PathingTask> _pathItems = [];

    private MapViewer? _mapViewer;

    private void InitScriptListViewData()
    {
        _pathItems.Clear();
        var fileInfos = LoadScriptFolder(PathJsonPath);
        foreach (var f in fileInfos)
        {
            try
            {
                _pathItems.Add(PathingTask.BuildFromFilePath(f.FullName));
            }
            catch (Exception e)
            {
                Toast.Warning($"地图追踪任务 {f.Name} 载入失败：{e.Message}");
            }
        }
    }

    private IEnumerable<FileInfo> LoadScriptFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var files = Directory.GetFiles(folder, "*.*",
            SearchOption.AllDirectories);

        return files.Select(file => new FileInfo(file)).ToList();
    }

    public void OnNavigatedTo()
    {
        InitScriptListViewData();
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    public void OnOpenScriptsFolder()
    {
        if (!Directory.Exists(PathJsonPath))
        {
            Directory.CreateDirectory(PathJsonPath);
        }
        Process.Start("explorer.exe", PathJsonPath);
    }

    [RelayCommand]
    public void OnOpenScriptProjectFolder(ScriptProject? item)
    {
        if (item == null)
        {
            return;
        }
        Process.Start("explorer.exe", item.ProjectPath);
    }

    [RelayCommand]
    public async void OnStart(PathingTask? item)
    {
        if (item == null)
        {
            return;
        }

        // new TaskRunner(DispatcherTimerOperationEnum.UseCacheImageWithTriggerEmpty)
        // .FireAndForget(async () =>
        // {
        //     TaskTriggerDispatcher.Instance().AddTrigger("AutoPick", null);
        //     await new PathExecutor(CancellationContext.Instance.Cts).Pathing(item);
        // });

        var fileInfo = new FileInfo(item.FullPath);
        var project = ScriptGroupProject.BuildPathingProject(fileInfo.Name, fileInfo.DirectoryName!);
        await scriptService.RunMulti([project]);
    }

    [RelayCommand]
    public void OnOpenMapViewer()
    {
        if (_mapViewer == null || !_mapViewer.IsVisible)
        {
            _mapViewer = new MapViewer();
            _mapViewer.Closed += (s, e) => _mapViewer = null;
            _mapViewer.Show();
        }
        else
        {
            _mapViewer.Activate();
        }
    }

    [RelayCommand]
    public void OnOpenMapEditor()
    {
        PathRecorder.Instance.OpenEditorInWebView();
    }

    [RelayCommand]
    public void OnGoToPathingUrl()
    {
        Process.Start(new ProcessStartInfo("https://bgi.huiyadan.com/autos/pathing.html") { UseShellExecute = true });
    }

    [RelayCommand]
    public void OnRefresh(PathingTask? item)
    {
        InitScriptListViewData();
    }
}
