using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaIos18Bug;

public class ViewController : UIViewController
{
    private readonly SKCanvasView _canvasView = new();
    private readonly UILabel _label = new();
    private readonly UIImageView _imageView = new();
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
        
        _imageView.Image = CoreGraphicsText($"CoreGraphics: 单° ({_typeface!.FamilyName})", _label.Font);
        _imageView.TranslatesAutoresizingMaskIntoConstraints = false;
        Add(_imageView);
        _imageView.TopAnchor.ConstraintEqualTo(_label.BottomAnchor).Active = true;
        _imageView.LeftAnchor.ConstraintEqualTo(_label.LeftAnchor).Active = true;
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

    private static UIImage CoreGraphicsText(string text, UIFont font)
    {
        var str = new NSString(text);
        var strAttr = new UIStringAttributes
        {
            Font = font
        };

        var expectedTextSize = str.GetSizeUsingAttributes(strAttr);

        var width = Math.Ceiling(expectedTextSize.Width);
        var height = Math.Ceiling(expectedTextSize.Height);
        var size = new CGSize(width, height);

        var renderer = new UIGraphicsImageRenderer(size, new UIGraphicsImageRendererFormat
        {
            Opaque = false,
            Scale = UIScreen.MainScreen.Scale
        });

        return renderer.CreateImage(imageContext =>
        {
            using var context = imageContext.CGContext;
            context.SetFillColor(UIColor.Black.CGColor);
            var fontTopPosition =
                Math.Round((height - expectedTextSize.Height) / 2.0f, MidpointRounding.AwayFromZero);
            var textPoint = new CGPoint(0, fontTopPosition);
            str.DrawString(textPoint, strAttr);
        });
    }
}