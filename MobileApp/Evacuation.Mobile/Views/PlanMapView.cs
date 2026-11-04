using System.Windows.Input;
using Evacuation.Mobile.Models;
using Microsoft.Maui.Graphics.Platform;

namespace Evacuation.Mobile.Views;

/// <summary>
/// Interaktywny podgląd rzutu: pobiera obraz z API, rysuje na nim trasę
/// i obsługuje przesuwanie, przybliżanie oraz wskazywanie pomieszczenia dotknięciem.
/// </summary>
public sealed class PlanMapView : ContentView
{
    private const float MinScale = 0.2f;
    private const float MaxScale = 12f;

    private readonly GraphicsView _surface;
    private readonly PlanMapDrawable _drawable = new();

    private PointF _panOrigin;
    private float _pinchOrigin = 1;
    private bool _fitted;

    public PlanMapView()
    {
        _surface = new GraphicsView { Drawable = _drawable };

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPan;

        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinch;

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;

        _surface.GestureRecognizers.Add(pan);
        _surface.GestureRecognizers.Add(pinch);
        _surface.GestureRecognizers.Add(tap);

        // Dopasowanie liczymy dopiero, gdy kontrolka zna swój rozmiar.
        _surface.SizeChanged += (_, _) => FitToView();

        Content = _surface;
    }

    public static readonly BindableProperty PlanImageUrlProperty = BindableProperty.Create(
        nameof(PlanImageUrl), typeof(string), typeof(PlanMapView), propertyChanged: OnPlanUrlChanged);

    public static readonly BindableProperty StepsProperty = BindableProperty.Create(
        nameof(Steps), typeof(IReadOnlyList<RouteStep>), typeof(PlanMapView),
        defaultValue: Array.Empty<RouteStep>(), propertyChanged: OnStepsChanged);

    public static readonly BindableProperty CurrentStepIndexProperty = BindableProperty.Create(
        nameof(CurrentStepIndex), typeof(int), typeof(PlanMapView),
        defaultValue: 0, propertyChanged: OnCurrentStepChanged);

    /// <summary>Wywoływane punktem rzutu, w który stuknął użytkownik.</summary>
    public static readonly BindableProperty PointTappedCommandProperty = BindableProperty.Create(
        nameof(PointTappedCommand), typeof(ICommand), typeof(PlanMapView));

    public string? PlanImageUrl
    {
        get => (string?)GetValue(PlanImageUrlProperty);
        set => SetValue(PlanImageUrlProperty, value);
    }

    public IReadOnlyList<RouteStep> Steps
    {
        get => (IReadOnlyList<RouteStep>)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    public int CurrentStepIndex
    {
        get => (int)GetValue(CurrentStepIndexProperty);
        set => SetValue(CurrentStepIndexProperty, value);
    }

    public ICommand? PointTappedCommand
    {
        get => (ICommand?)GetValue(PointTappedCommandProperty);
        set => SetValue(PointTappedCommandProperty, value);
    }

    private static void OnPlanUrlChanged(BindableObject bindable, object oldValue, object newValue) =>
        _ = ((PlanMapView)bindable).LoadPlanAsync(newValue as string);

    private static void OnStepsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (PlanMapView)bindable;

        view._drawable.Steps = newValue as IReadOnlyList<RouteStep> ?? [];
        view._surface.Invalidate();
    }

    private static void OnCurrentStepChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (PlanMapView)bindable;

        view._drawable.CurrentStepIndex = (int)newValue;
        view.CenterOnCurrentStep();
    }

    private async Task LoadPlanAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            await using var stream = await http.GetStreamAsync(url);

            // Obraz trzeba zdekodować do pamięci przed przekazaniem na kanwę.
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            buffer.Position = 0;

            _drawable.Plan = PlatformImage.FromStream(buffer);
            _fitted = false;

            MainThread.BeginInvokeOnMainThread(FitToView);
        }
        catch (Exception)
        {
            // Brak rzutu nie może wywrócić ekranu trasy — lista kroków jest użyteczna i bez mapy.
            _drawable.Plan = null;
            MainThread.BeginInvokeOnMainThread(_surface.Invalidate);
        }
    }

    /// <summary>Skaluje rzut tak, by zmieścił się w całości, i centruje go.</summary>
    private void FitToView()
    {
        if (_drawable.Plan is null || _surface.Width <= 0 || _surface.Height <= 0)
        {
            return;
        }

        var scale = Math.Min(
            _surface.Width / _drawable.Plan.Width,
            _surface.Height / _drawable.Plan.Height);

        _drawable.Scale = (float)scale;
        _drawable.Offset = new PointF(
            (float)((_surface.Width - (_drawable.Plan.Width * scale)) / 2),
            (float)((_surface.Height - (_drawable.Plan.Height * scale)) / 2));

        _fitted = true;
        _surface.Invalidate();
    }

    private void OnPan(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panOrigin = _drawable.Offset;
                break;

            case GestureStatus.Running:
                _drawable.Offset = new PointF(
                    _panOrigin.X + (float)e.TotalX,
                    _panOrigin.Y + (float)e.TotalY);
                _surface.Invalidate();
                break;
        }
    }

    private void OnPinch(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _pinchOrigin = _drawable.Scale;
            return;
        }

        if (e.Status != GestureStatus.Running)
        {
            return;
        }

        var scale = Math.Clamp(_pinchOrigin * (float)e.Scale, MinScale, MaxScale);

        // Punkt pod palcami ma zostać w miejscu, więc przesunięcie korygujemy o zmianę skali.
        var anchor = new PointF(
            (float)(e.ScaleOrigin.X * _surface.Width),
            (float)(e.ScaleOrigin.Y * _surface.Height));

        var ratio = scale / _drawable.Scale;

        _drawable.Offset = new PointF(
            anchor.X - ((anchor.X - _drawable.Offset.X) * ratio),
            anchor.Y - ((anchor.Y - _drawable.Offset.Y) * ratio));

        _drawable.Scale = scale;
        _surface.Invalidate();
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (PointTappedCommand is null || _drawable.Plan is null)
        {
            return;
        }

        var position = e.GetPosition(_surface);

        if (position is not { } point)
        {
            return;
        }

        // Z współrzędnych ekranu z powrotem na piksele rzutu.
        var planPoint = new Point(
            (point.X - _drawable.Offset.X) / _drawable.Scale,
            (point.Y - _drawable.Offset.Y) / _drawable.Scale);

        if (planPoint.X < 0 || planPoint.Y < 0
            || planPoint.X >= _drawable.Plan.Width || planPoint.Y >= _drawable.Plan.Height)
        {
            return;
        }

        if (PointTappedCommand.CanExecute(planPoint))
        {
            PointTappedCommand.Execute(planPoint);
        }
    }

    /// <summary>Przesuwa widok tak, by aktualny przystanek znalazł się na środku.</summary>
    private void CenterOnCurrentStep()
    {
        if (!_fitted || _drawable.Plan is null || _surface.Width <= 0)
        {
            _surface.Invalidate();
            return;
        }

        var steps = _drawable.Steps;

        if (CurrentStepIndex < 0 || CurrentStepIndex >= steps.Count)
        {
            _surface.Invalidate();
            return;
        }

        var step = steps[CurrentStepIndex];

        _drawable.Offset = new PointF(
            (float)(_surface.Width / 2) - (step.X * _drawable.Scale),
            (float)(_surface.Height / 2) - (step.Y * _drawable.Scale));

        _surface.Invalidate();
    }
}
