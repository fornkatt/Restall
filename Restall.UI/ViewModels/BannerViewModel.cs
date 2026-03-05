using Avalonia.Media.Imaging;
using Restall.Domain.Entities;

namespace Restall.UI.ViewModels;

public class BannerViewModel : ViewModelBase
{
    private readonly AppState _appState;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public Game? SelectedGame => _mainWindowViewModel.SelectedGame;
    
    public Bitmap? BannerBitmap =>
        !string.IsNullOrWhiteSpace(SelectedGame.BannerPathString)
            ? new Bitmap(SelectedGame.BannerPathString)
            : null;

    public Bitmap? LogoBitmap =>
        !string.IsNullOrWhiteSpace(SelectedGame.LogoPathString)
            ? new Bitmap(SelectedGame.LogoPathString)
            : null;

    public Bitmap? ThumbnailBitmap =>
        !string.IsNullOrWhiteSpace(SelectedGame.ThumbnailPathString)
            ? new Bitmap(SelectedGame.ThumbnailPathString)
            : null;
    
    public BannerViewModel(MainWindowViewModel mainWindowViewModel, AppState appState)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _appState = appState;

        _mainWindowViewModel.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(_mainWindowViewModel.SelectedGame):
                    OnPropertyChanged(nameof(SelectedGame));
                    break;
            }
        };
    }
}