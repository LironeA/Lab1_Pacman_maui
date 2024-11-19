using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_Pacman_maui
{
    public static class TilesModel
    {
        public static int Size = 25;
        public static int StrokeWidth = Size / 10;

        public static SKColor WallColor = SKColor.Parse("#b2b2b2");
        public static SKColor Straight = SKColor.Parse("#ff5722");
        public static SKColor Corner = SKColor.Parse("#3f51b5");
        public static SKColor TJunction = SKColor.Parse("##2196f3");
        public static SKColor Cross = SKColor.Parse("##00bcd4");

        public static SKPaint WallPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = WallColor
        };

        public static SKPaint PathPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        public static SKPaint StraightPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = StrokeWidth,
            Color = Straight
        };

        public static SKPaint CornerPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = StrokeWidth,
            Color = Corner
        };

        public static SKPaint TJunctionPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = StrokeWidth,
            Color = TJunction
        };

        public static SKPaint CrossPaint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = StrokeWidth,
            Color = Cross
        };

        public static SKPaint BorderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = StrokeWidth,
            Color = SKColors.Black
        };
    }
}
