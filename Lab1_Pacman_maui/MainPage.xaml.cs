using Microsoft.Maui.ApplicationModel;
using SkiaSharp;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Lab1_Pacman_maui
{
    public partial class MainPage : ContentPage
    {
        
        GameManager gameManager = new GameManager();

        public MainPage()
        {
            gameManager.DrawAction = Draw;
            InitializeComponent();
        }

        private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.White);
            gameManager.DrawMap(sender, e);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            gameManager.Restart();
        }

        private void Draw()
        {
            cnvs.InvalidateSurface();
        }
    }



   


}
