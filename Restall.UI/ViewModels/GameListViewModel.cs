using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Restall.UI.Messages;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Restall.Application.DTOs;
using Restall.Application.UseCases;

namespace Restall.UI.ViewModels;

public partial class GameListViewModel : ViewModelBase, IRecipient<SelectedGameChangedMessage>
{   
    
    
    private readonly IRefreshLibraryUseCase _refreshLibrary;
    private CancellationTokenSource _messageCts = new();
    
    private bool _suppressMessage;

    [ObservableProperty]
    private ObservableCollection<GameModViewModel> _games = [];

    [ObservableProperty]
    private GameModViewModel? _selectedGame;

    [ObservableProperty] 
    private string? _scanMessage;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshLibraryCommand))]
    private bool _isRefreshing;

    partial void OnSelectedGameChanged(GameModViewModel? value)
    {
        if (!_suppressMessage)
            Messenger.Send(new SelectedGameChangedMessage(value));
    }

    public void Receive(SelectedGameChangedMessage message)
    {
        _suppressMessage = true;
        SelectedGame = message.Value;
        _suppressMessage = false;
    }
    
    public void LoadGames(RefreshLibraryResultDto result)
    {
        Games.Clear();
        foreach (var item in result.Games)
        {
            Games.Add(new GameModViewModel(item.Game)
            {
                CompatibleRenoDXMod = item.CompatibleMod,
                CompatibleRenoDXGenericMod = item.CompatibleGenericMod
            });
        }

        SelectedGame = Games.FirstOrDefault();
    }
    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshLibraryAsync()
    {

        await ExecuteWithDelayedMessageAsync(async () =>
        {
            var result = await _refreshLibrary.ExecuteAsync();
            LoadGames(result);

        });
        
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true);
        
        
    }

    private async Task ExecuteWithDelayedMessageAsync(Func<Task> message)
    {
        await _messageCts.CancelAsync();
        _messageCts = new CancellationTokenSource();
        var token = _messageCts.Token;
        // Protect data and status
        try
        {
            ScanMessage = "Scanning...";
            IsRefreshing = true;
            await message();
            ScanMessage = "Completed!";
        }
        catch (Exception)
        {
            ScanMessage = "Error";
        }
        finally
        {
            IsRefreshing = false;
        }
        
        // Protect UI Timer
        try
        {
            await Task.Delay(2000, token);
            if (!token.IsCancellationRequested)
            {
                ScanMessage = string.Empty;
            }
        }
        catch { }
    }
    
    private bool CanRefresh() => !IsRefreshing;

    public GameListViewModel(IRefreshLibraryUseCase refreshLibrary)
    {
        _refreshLibrary = refreshLibrary;
        IsActive = true;
    }
    
}