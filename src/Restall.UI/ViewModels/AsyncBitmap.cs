// SPDX-FileCopyrightText: 2026 Johan Lager & Kristofer Sell & Filip Klaic
// SPDX-License-Identifier: GPL-3.0-or-later

using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Restall.UI.ViewModels;

public sealed class AsyncBitmap : ObservableObject, IDisposable
{
    private readonly string? _path;
    private readonly int _targetWidth;

    private Bitmap? _bitmap;
    private LoadState _loadState = LoadState.NotLoaded;

    public AsyncBitmap(string? path, int targetWidth)
    {
        _path = path;
        _targetWidth = targetWidth;
    }

    public Bitmap? Value
    {
        get
        {
            if (_loadState is LoadState.NotLoaded)
                StartLoad();

            return _bitmap;
        }
    }

    public void Dispose()
    {
        Dispatcher.UIThread.VerifyAccess();

        if (_loadState is LoadState.Disposed)
            return;

        _loadState = LoadState.Disposed;

        var bitmap = _bitmap;

        _bitmap = null;

        OnPropertyChanged(nameof(Value));

        bitmap?.Dispose();
    }

    private async void StartLoad()
    {
        Dispatcher.UIThread.VerifyAccess();

        if (string.IsNullOrWhiteSpace(_path))
        {
            _loadState = LoadState.Loaded;
            return;
        }

        _loadState = LoadState.Loading;

        var path = _path;
        var bitmap = await Task.Run(() => DecodeBitmap(path, _targetWidth));

        if (_loadState is LoadState.Disposed)
        {
            bitmap?.Dispose();
            return;
        }

        _bitmap = bitmap;
        _loadState = LoadState.Loaded;

        OnPropertyChanged(nameof(Value));
    }

    private static Bitmap? DecodeBitmap(string path, int targetWidth)
    {
        try
        {
            using var stream = File.OpenRead(path);

            return DecodeToWidthOrNull(stream, targetWidth);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static Bitmap? DecodeToWidthOrNull(Stream stream, int targetWidth)
    {
        try
        {
            return Bitmap.DecodeToWidth(stream, targetWidth);
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private enum LoadState
    {
        NotLoaded,
        Loading,
        Loaded,
        Disposed
    }
}
