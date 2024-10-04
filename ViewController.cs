using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaIos18Bug;

public class ViewController : UIViewController
{
    private readonly SKCanvasView _canvasView = new();
    private readonly UILabel _label = new();
    private SKTypeface? _typeface;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        
        _typeface = SKFontManager.Default.MatchCharacter('单'); // Should return PingFang SC

        _canvasView.TranslatesAutoresizingMaskIntoConstraints = false;
        Add(_canvasView);
        _canvasView.TopAnchor.ConstraintEqualTo(View!.TopAnchor).Active = true;
        _canvasView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor).Active = true;
        _canvasView.RightAnchor.ConstraintEqualTo(View.RightAnchor).Active = true;
        _canvasView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        _canvasView.PaintSurface += OnPainting;
        
        _label.TranslatesAutoresizingMaskIntoConstraints = false;
        Add(_label);
        _label.TopAnchor.ConstraintEqualTo(View.TopAnchor, 130).Active = true;
        _label.LeftAnchor.ConstraintEqualTo(View.LeftAnchor, 30).Active = true;
        
        _label.TextColor = UIColor.Black;
        _label.Font = UIFont.FromName(_typeface.FamilyName, 40);
        _label.Text = $"UILabel: 单° ({_typeface!.FamilyName})";
    }
    
    private void OnPainting(object? sender, SKPaintSurfaceEventArgs e)
    {
        // we get the current surface from the event args
        var surface = e.Surface;
        // then we get the canvas that we can draw on
        var canvas = surface.Canvas;
        // clear the canvas / view
        canvas.Clear(SKColors.White);
        
        // create the paint for the text
        var textPaint = new SKPaint
        {
            IsAntialias = true,
            Typeface = _typeface,
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black,
            TextSize = 80
        };
        // draw the text (from the baseline)
        canvas.DrawText($"Skia: 单° ({_typeface!.FamilyName})", 60, 160 + 80, textPaint);
    }
}