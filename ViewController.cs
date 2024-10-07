using CoreText;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace SkiaIos18Bug;

public class ViewController : UIViewController
{
    private readonly SKCanvasView _canvasView = new();
    private readonly UILabel _label = new();
    private readonly UIImageView _imageViewCg = new();
    private readonly UIImageView _imageViewCt = new();
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
        
        _imageViewCg.Image = CoreGraphics($"CoreGraphics: 单° ({_typeface!.FamilyName})", _label.Font);
        _imageViewCg.TranslatesAutoresizingMaskIntoConstraints = false;
        Add(_imageViewCg);
        _imageViewCg.TopAnchor.ConstraintEqualTo(_label.BottomAnchor).Active = true;
        _imageViewCg.LeftAnchor.ConstraintEqualTo(_label.LeftAnchor).Active = true;
        
        _imageViewCt.Image = CoreText($"CoreText: 单° ({_typeface!.FamilyName})", _label.Font);
        _imageViewCt.TranslatesAutoresizingMaskIntoConstraints = false;
        Add(_imageViewCt);
        _imageViewCt.TopAnchor.ConstraintEqualTo(_imageViewCg.BottomAnchor).Active = true;
        _imageViewCt.LeftAnchor.ConstraintEqualTo(_label.LeftAnchor).Active = true;
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

    private static UIImage CoreGraphics(string text, UIFont font)
    {
        var str = new NSString(text);
        var strAttr = new UIStringAttributes
        {
            Font = font
        };

        var expectedTextSize = str.GetSizeUsingAttributes(strAttr);
        var size = new CGSize(expectedTextSize.Width, expectedTextSize.Height);

        var renderer = new UIGraphicsImageRenderer(size, new UIGraphicsImageRendererFormat
        {
            Opaque = false,
            Scale = UIScreen.MainScreen.Scale
        });

        return renderer.CreateImage(imageContext =>
        {
            using var context = imageContext.CGContext;
            context.SetFillColor(UIColor.Black.CGColor);
            str.DrawString(CGPoint.Empty, strAttr);
        });
    }
    
    private static UIImage CoreText(string text, UIFont font)
    {
        var str = new NSString(text);
        var strAttr = new CTStringAttributes
        {
            ForegroundColorFromContext = true,
            Font = new CTFont(font.FontDescriptor.PostscriptName, font.PointSize)
        };
        var attributedString = new NSAttributedString(str, strAttr);
        
        var size = new CGSize(attributedString.Size.Width, attributedString.Size.Height);
        var textPos = size.Height - (size.Height - strAttr.Font.CapHeightMetric) / 2f;
        var renderer = new UIGraphicsImageRenderer(size, new UIGraphicsImageRendererFormat
        {
            Opaque = false,
            Scale = UIScreen.MainScreen.Scale
        });

        return renderer.CreateImage(imageContext =>
        {
            using var context = imageContext.CGContext;
            context.ScaleCTM(1, -1);
            
            context.SetFillColor(UIColor.Black.CGColor);
            context.TranslateCTM(0, -textPos);

            using var textLine = new CTLine(attributedString);
            textLine.Draw(context);
        });
    }
}