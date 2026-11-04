using Evacuation.Mobile.Models;

namespace Evacuation.Mobile.Views;

/// <summary>
/// Rysuje rzut piętra z naniesioną trasą ewakuacji.
/// </summary>
/// <remarks>
/// Pracuje w układzie współrzędnych rzutu — wszystkie punkty trasy są pikselami obrazu.
/// Przeliczeniem na współrzędne ekranu zajmuje się jedna transformacja nałożona na początku
/// rysowania, dzięki czemu przesuwanie i przybliżanie nie wymagają przeliczania punktów.
/// </remarks>
public sealed class PlanMapDrawable : IDrawable
{
    private static readonly Color Route = Color.FromArgb("#E1AD66");
    private static readonly Color RoutePassed = Color.FromArgb("#7D5411");
    private static readonly Color Exit = Color.FromArgb("#2F6B46");
    private static readonly Color Start = Color.FromArgb("#5B7BA8");
    private static readonly Color Background = Color.FromArgb("#201F1D");

    public Microsoft.Maui.Graphics.IImage? Plan { get; set; }

    public IReadOnlyList<RouteStep> Steps { get; set; } = [];

    /// <summary>Indeks aktualnego przystanku. Odcinki przed nim rysowane są przygaszone.</summary>
    public int CurrentStepIndex { get; set; }

    public float Scale { get; set; } = 1;

    public PointF Offset { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Background;
        canvas.FillRectangle(dirtyRect);

        if (Plan is null)
        {
            return;
        }

        canvas.SaveState();
        canvas.Translate(Offset.X, Offset.Y);
        canvas.Scale(Scale, Scale);

        canvas.DrawImage(Plan, 0, 0, Plan.Width, Plan.Height);

        DrawRoute(canvas);
        DrawMarkers(canvas);

        canvas.RestoreState();
    }

    private void DrawRoute(ICanvas canvas)
    {
        if (Steps.Count < 2)
        {
            return;
        }

        // Grubość linii dzielimy przez powiększenie, żeby trasa zachowała stałą szerokość
        // na ekranie niezależnie od tego, jak mocno użytkownik przybliżył rzut.
        canvas.StrokeSize = 4 / Scale;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        for (var i = 1; i < Steps.Count; i++)
        {
            canvas.StrokeColor = i <= CurrentStepIndex ? RoutePassed : Route;

            canvas.DrawLine(Steps[i - 1].X, Steps[i - 1].Y, Steps[i].X, Steps[i].Y);
        }
    }

    private void DrawMarkers(ICanvas canvas)
    {
        if (Steps.Count == 0)
        {
            return;
        }

        var radius = 7 / Scale;

        DrawDot(canvas, Steps[0], Start, radius);
        DrawDot(canvas, Steps[^1], Exit, radius);


        // Aktualny przystanek dostaje obwódkę, żeby było widać, gdzie użytkownik ma być teraz.
        if (CurrentStepIndex >= 0 && CurrentStepIndex < Steps.Count)
        {
            var step = Steps[CurrentStepIndex];

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 3 / Scale;
            canvas.DrawCircle(step.X, step.Y, radius * 1.8f);
        }
    }

    private void DrawDot(ICanvas canvas, RouteStep step, Color color, float radius)
    {
        canvas.FillColor = color;
        canvas.FillCircle(step.X, step.Y, radius);

        // Grubość obwódki, jak każdej linii tutaj, dzielimy przez powiększenie —
        // inaczej przy przybliżaniu rosłaby razem z rysunkiem.
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2 / Scale;
        canvas.DrawCircle(step.X, step.Y, radius);
    }
}
