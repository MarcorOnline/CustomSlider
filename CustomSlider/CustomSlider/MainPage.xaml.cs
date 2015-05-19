using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace CustomSlider
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int Min = 1;
        int Max = 45;
        int Size = 100;
        int Range = 150;
        int left = 0;
        int right = 0;
        Point leftPoint = new Point();
        Point rightPoint = new Point();

        Point initalPoint = new Point(0,0);


        public MainPage()
        {
            this.InitializeComponent();
            
            this.NavigationCacheMode = NavigationCacheMode.Required;

            var pos = SetPosition(Thumb1Position);

            Thumb1Position = 1;
            Thumb2Position = 45;

            CompositeTransform ct = new CompositeTransform();
            ct.TranslateX = pos;
            LeftHandle.RenderTransform = ct;
            leftPoint.X = pos;
            LeftHandleText.Text = Text(pos);

            pos = SetPosition(Thumb2Position);

            CompositeTransform rCt = new CompositeTransform();
            rCt.TranslateX = pos;
            RightHandle.RenderTransform = rCt;
            rightPoint.X = pos;
            RightHandleText.Text = Text(pos);

            FillTrackGrid.Width  = FillTrack(rightPoint, leftPoint);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
        

        private void LeftHandle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var t = (sender as Grid).RenderTransform as CompositeTransform;
            var x = (RightHandle.RenderTransform as CompositeTransform).TranslateX;
            var f = -this.Range;
            var c = x - this.Size * .1;
            double translateVal = Translate(t, e, f, c);
            t.TranslateX = Translate(t, e, f, c);
            LeftHandleText.Text = Text(t.TranslateX);

            CompositeTransform ct = new CompositeTransform();
            ct.TranslateX = Translate(t, e, f, c);
            FillTrackGrid.RenderTransform = ct;
            
            left = Convert.ToInt32(translateVal);

            leftPoint.X = t.TranslateX + e.Delta.Translation.X;
            leftPoint.Y = t.TranslateY + e.Delta.Translation.Y;
            
            FillTrackGrid.Width = FillTrack(rightPoint, leftPoint);
        }

        private void RightHandle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var t = (sender as Grid).RenderTransform as CompositeTransform;
            var x = (LeftHandle.RenderTransform as CompositeTransform).TranslateX;
            var f = x + this.Size * .1;
            var c = this.Range;
            t.TranslateX = Translate(t, e, f, c);
            RightHandleText.Text = Text(t.TranslateX);

            double translateVal = Translate(t, e, f, c);
            right = Convert.ToInt32(translateVal);

            rightPoint.X = t.TranslateX + e.Delta.Translation.X;
            rightPoint.Y = t.TranslateY + e.Delta.Translation.Y;

            FillTrackGrid.Width = FillTrack(rightPoint, leftPoint);

        }

        private double Translate(CompositeTransform s, ManipulationDeltaRoutedEventArgs e, double floor, double ceiling)
        {
            var target = s.TranslateX + e.Delta.Translation.X;           

            if (target < floor)
                return floor;
            if (target > ceiling)
                return ceiling;
            return target;
        }

        private string Text(double x)
        {
            var p = (x - (-this.Range)) / ((this.Range) - (-this.Range)) * 100d;
            var v = (this.Max - this.Min) * p / 100d + this.Min;
            return ((int)v).ToString();
        }

        public double SetPosition(int value) 
        {
            var p = ((value - this.Min) * 100d)/ (this.Max - this.Min);

            var x = (p * (this.Range - (-this.Range)) / 100d) + (-this.Range);

            return x;
        }

        public double FillTrack(Point A, Point B)
        {
            //SliderManager sm = SliderManager.Instance;

            //sm.DeltaValue = Math.Abs(right) + Math.Abs(left);

            //return Math.Abs(right) + Math.Abs(left);

            double a = A.X - B.X;
            double b = A.Y - B.Y;
            double distance = Math.Sqrt(a * a + b * b);
            
            return distance;

        }

        public int Thumb1Position
        {
            get { return thumb1Pos; }
            set
            {
                thumb1Pos = value;
            }
        }

        public int Thumb2Position
        {
            get { return thumb2Pos; }
            set
            {
                thumb2Pos = value;
            }
        }
        
    }
}

