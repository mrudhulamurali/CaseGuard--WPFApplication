/* Application : Spray Paint Application using WPF
 * Author      : Mrudhula Murali.   
 * Created on  : 24th June 2021  */

/* Refrences Used:
** WPF tutorial in LinkedIn by Walt Ritscher
** https://stackoverflow.com/questions/2906691/simulate-a-spray
**  https://www.godo.dev/tutorials/wpf-load-external-image/  */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CaseGuardWpfApp
{
    /// <summary>
    /// This application will implements the below use cases
    ///     * allows users to select and load an image
    ///     * ability to spray paint the image using the mouse
    ///     * able to change the color and the density of the paint
    ///     * able to erase some or all of the spray using the mouse
    ///     * able to save the changes and close
    /// </summary>
    public partial class MainWindow : Window
    {
        // Initialise variables
        Point currentPoint = new Point();
        List<Ellipse> ellipseList = new List<Ellipse>();

        byte selectedColorR = 0;
        byte selectedColorG = 0;
        byte selectedColorB = 0;
        double thicknessValue = 1;
        bool IsMouseErase = false;        
        string CurrentImagePath = "";
        bool saveSettings = false;

        string imgFilters = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
        
        //Logger Declaration
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        // Initial Window close event
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (!saveSettings)
            {
                imgPhoto.Source = null;
            }
            Log.Info("Application end time:" + DateTime.Now);
        }

        // Initial Window load event
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Info("Application start time:" + DateTime.Now);
               imgPhoto.Source = new BitmapImage(new Uri(CurrentImagePath));                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }


        // Loads the new image
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog op = new OpenFileDialog();
                op.Title = "Select a picture";
                op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                  "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                  "Portable Network Graphic (*.png)|*.png";
                if (op.ShowDialog() == true)
                {
                    imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        // Here the movement of mouse is detected and passed to the draw or erase spray paint method.
        private void paintArea_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && !IsMouseErase)
                {
                    paintArea.Cursor = Cursors.Cross;
                    currentPoint = e.GetPosition(this);

                    drawEllipse(e);
                }
                else if (e.LeftButton == MouseButtonState.Pressed && IsMouseErase)
                {
                    paintArea.Cursor = Cursors.IBeam;
                    currentPoint = e.GetPosition(this);
                    removeEllipse(e);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

        }

        /* This method helps in erasing the spray paint from the image, here current point in the image is calculated and the 
         * overlapping area is removed from the paint area.*/
        private void removeEllipse(MouseEventArgs e)
        {
            try
            {
                SolidColorBrush color = new SolidColorBrush();
                color.Color = Color.FromRgb(selectedColorR, selectedColorG, selectedColorB);

                Ellipse currentDot = new Ellipse();
                currentDot.Stroke = Brushes.Transparent;
                currentDot.Opacity = .04d;

                currentDot.StrokeThickness = 3;
                Canvas.SetZIndex(currentDot, 3);

                currentDot.Height = thicknessValue;
                currentDot.Width = thicknessValue;
                currentDot.Margin = new Thickness(currentPoint.X,
                    currentPoint.Y,
                    e.GetPosition(this).X,
                    e.GetPosition(this).Y);

               foreach(var list in ellipseList)
                {
                    if(currentDot.Margin.Right>list.Margin.Left && currentDot.Margin.Left < list.Margin.Right
                        && currentDot.Margin.Bottom > list.Margin.Top && currentDot.Margin.Top < list.Margin.Bottom)
                    {
                        paintArea.Children.Remove(list);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

        }

        /* This method helps in spray painting the image using ellipse, here a random generator is used to simulate the 
           spray effect, the different coordinate values that we get is added to the canvas(paintarea) to get the color on the image.*/
        private void drawEllipse(MouseEventArgs e)
        {
            try
            {
                Random _rnd = new Random();
                int radius = 15;

                SolidColorBrush color = new SolidColorBrush();
                color.Color = Color.FromRgb(selectedColorR, selectedColorG, selectedColorB);

                Ellipse currentDot = new Ellipse();
                for (int i = 0; i < 200; ++i)
                {
                    currentDot.Opacity = .4d;

                    currentDot.StrokeThickness = 3;
                    Canvas.SetZIndex(currentDot, 3);

                    currentDot.Height = thicknessValue;
                    currentDot.Width = thicknessValue;
                    currentDot.Fill = color;

                    double theta = _rnd.NextDouble() * (Math.PI * 2);
                    double r = _rnd.NextDouble() * radius;

                    double x = currentPoint.X + Math.Cos(theta) * r;
                    double y = currentPoint.Y - Math.Sin(theta) * r;
                    double z = e.GetPosition(this).X + Math.Cos(theta) * r;
                    double w = e.GetPosition(this).Y - Math.Sin(theta) * r;


                    currentDot.Margin = new Thickness(x - 45,
                        y - 45, z, w);

                    paintArea.Children.Add(currentDot);

                    ellipseList.Add(currentDot);
                }
                    
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

        }


        // Event triggered when the selected color changes
        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                selectedColorR = ClrPcker_Background.SelectedColor.Value.R;
                selectedColorG = ClrPcker_Background.SelectedColor.Value.G;
                selectedColorB = ClrPcker_Background.SelectedColor.Value.B;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

        }       

        // Event triggered when the thickness value in the slider changes
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                thicknessValue = Convert.ToDouble(Slider.Value);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

        }

        // On the button Draw click event, the image can be spray painted with the mouse pointer using the color selected
        // from the dropdown.
        private void Draw_Click_(object sender, RoutedEventArgs e)
        {
            try
            {
                IsMouseErase = false;
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
            
        }

        // On the button Erase click event, the spray paint near the mouse pointer will be erased. 
        private void Erase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsMouseErase = true;
                paintArea.Cursor = Cursors.IBeam;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }


        // On the button Save click event, the image will be saved to the location provided by the user
        // with the current state displayed in UI  
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)paintArea.RenderSize.Width,
                (int)paintArea.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
                rtb.Render(paintArea);

                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = imgFilters;
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == true)
                {
                    using (var fs = File.OpenWrite(saveFileDialog1.FileName))
                    {
                        pngEncoder.Save(fs);
                    }
                }

                MessageBox.Show("Image Saved");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
