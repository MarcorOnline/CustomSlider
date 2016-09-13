using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace CustomSlider.Controls
{
    public class RangeSlider : Control
    {
        private bool loaded = true;

        private const int defaultMin = 1;
        private const int defaultMax = 100;

        public int Min
        {
            get { return (int)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.RegisterAttached("Min", typeof(int), typeof(RangeSlider), new PropertyMetadata(defaultMin, OnMinChanged));

        public int Max
        {
            get { return (int)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.RegisterAttached("Max", typeof(int), typeof(RangeSlider), new PropertyMetadata(defaultMax, OnMaxChanged));

        private static void OnMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RangeSlider;
            control.min = (int)e.NewValue;
            control.Draw();
        }

        private static void OnMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RangeSlider;
            control.max = (int)e.NewValue;
            control.Draw();
        }

        public int Value1
        {
            get { return (int)GetValue(Value1Property); }
            set { SetValue(Value1Property, value); }
        }

        public static readonly DependencyProperty Value1Property =
            DependencyProperty.RegisterAttached("Value1", typeof(int), typeof(RangeSlider), new PropertyMetadata(defaultMin, OnValue1Changed));

        public int Value2
        {
            get { return (int)GetValue(Value2Property); }
            set { SetValue(Value2Property, value); }
        }

        public static readonly DependencyProperty Value2Property =
            DependencyProperty.RegisterAttached("Value2", typeof(int), typeof(RangeSlider), new PropertyMetadata(defaultMax, OnValue2Changed));

        private static void OnValue1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RangeSlider;

            if (control.leftValue != (int)e.NewValue)
            {
                control.leftValue = (int)e.NewValue;
                control.Draw();
            }
        }

        private static void OnValue2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RangeSlider;

            if (control.rightValue != (int)e.NewValue)
            {
                control.rightValue = (int)e.NewValue;
                control.Draw();
            }
        }

        private int? min;
        private int? max;
        private int? leftValue;
        private int? rightValue;

        private CompositeTransform leftTransform;
        private CompositeTransform rightTransform;
        private CompositeTransform fillTransform;

        private Rectangle Track;
        private Rectangle FillTrackGrid;
        private Grid LeftHandle;
        private Grid RightHandle;
        private TextBlock LeftHandleText;
        private TextBlock RightHandleText;

        public RangeSlider()
        {
            this.DefaultStyleKey = typeof(RangeSlider);

            this.SizeChanged += RangeSlider_SizeChanged;

            this.Loaded += RangeSlider_Loaded;
            this.Unloaded += RangeSlider_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //get ui elements from template
            LeftHandle = (Grid)GetTemplateChild("LeftHandle");
            LeftHandle.ManipulationMode = ManipulationModes.TranslateX;

            RightHandle = (Grid)GetTemplateChild("RightHandle");
            RightHandle.ManipulationMode = ManipulationModes.TranslateX;

            Track = (Rectangle)GetTemplateChild("Track");
            FillTrackGrid = (Rectangle)GetTemplateChild("FillTrackGrid");
            LeftHandleText = (TextBlock)GetTemplateChild("LeftHandleText");
            RightHandleText = (TextBlock)GetTemplateChild("RightHandleText");

            leftTransform = LeftHandle.RenderTransform as CompositeTransform;
            if (leftTransform == null)
                LeftHandle.RenderTransform = leftTransform = new CompositeTransform();

            rightTransform = RightHandle.RenderTransform as CompositeTransform;
            if (rightTransform == null)
                RightHandle.RenderTransform = rightTransform = new CompositeTransform();

            fillTransform = FillTrackGrid.RenderTransform as CompositeTransform;
            if (fillTransform == null)
                FillTrackGrid.RenderTransform = fillTransform = new CompositeTransform();

            Draw();
        }

        private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;

            if (LeftHandle == null || RightHandle == null)
                return;

            LeftHandle.ManipulationDelta += LeftHandle_ManipulationDelta;
            RightHandle.ManipulationDelta += RightHandle_ManipulationDelta;

            Draw();
        }

        private void RangeSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            loaded = false;

            if (LeftHandle == null || RightHandle == null)
                return;

            LeftHandle.ManipulationDelta -= LeftHandle_ManipulationDelta;
            RightHandle.ManipulationDelta -= RightHandle_ManipulationDelta;
        }

        private void RangeSlider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private bool CheckValuesIntegrity()
        {
            //apply default values if necessary
            if (!leftValue.HasValue)
                leftValue = Value1;

            if (!rightValue.HasValue)
                rightValue = Value2;

            if (!min.HasValue)
                min = Min;

            if (!max.HasValue)
                max = Max;

            //check integrity
            if (leftValue < min)
                if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                    throw new Exception(string.Format("RangeSlider: 'Value1' must be greater or equal than 'Min'. {0} is lower than {1}", leftValue, Min));

            if (rightValue > max)
                if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                    throw new Exception(string.Format("RangeSlider: 'Value2' must be lower or equal than 'Max'. {0} is greater than {1}", rightValue, Max));

            if (min >= max)
                if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                    throw new Exception(string.Format("RangeSlider: 'Min' and 'Max' values must be different and 'Min' must be lower than 'Max'. {0} is greater or equal than {1}", Min, Max));

            if (leftValue > rightValue)
                if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                    throw new Exception(string.Format("RangeSlider: 'Value1' must be lower or equal than 'Value2'. {0} is greater than {1}", leftValue, rightValue));

            return true;
        }

        private void Draw()
        {
            if (!loaded || FillTrackGrid == null || LeftHandleText == null || RightHandleText == null || FillTrackGrid.ActualWidth == 0)
                return;

            if (CheckValuesIntegrity())
            {
                //LEFT
                var pos = SetPosition(leftValue.Value);
                leftTransform.TranslateX = pos;
                LeftHandleText.Text = leftValue.ToString();

                //RIGHT
                pos = SetPosition(rightValue.Value);
                rightTransform.TranslateX = pos;
                RightHandleText.Text = rightValue.ToString();

                //FILL
                FillTrack();
            }
        }

        //Changes left thumb
        private void LeftHandle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var translate = Translate(leftTransform, e.Delta.Translation.X, true);
            leftTransform.TranslateX = translate;
            leftValue = CalculateValue(translate);      //apply to leftValue to not call Redraw when Value1 is set
            Value1 = leftValue.Value;
            LeftHandleText.Text = leftValue.ToString();

            FillTrack();
        }

        //Changes right thumb
        private void RightHandle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var translate = Translate(rightTransform, e.Delta.Translation.X, false);
            rightTransform.TranslateX = translate;
            rightValue = CalculateValue(translate);     //apply to rightValue to not call Redraw when Value2 is set
            Value2 = rightValue.Value;
            RightHandleText.Text = rightValue.ToString();

            FillTrack();
        }

        private double Translate(CompositeTransform s, double deltaTranslateX, bool left)
        {
            var uiRange = Track.ActualWidth;
            var minimum = left ? -uiRange / 2 : leftTransform.TranslateX;
            var maximum = left ? rightTransform.TranslateX : uiRange / 2;

            var target = s.TranslateX + deltaTranslateX;

            if (target < minimum)
                return minimum;
            if (target > maximum)
                return maximum;

            return target;
        }

        private int CalculateValue(double xTranslation)
        {
            var valueRange = max.Value - min.Value;
            var uiRange = Track.ActualWidth;

            var value = (xTranslation + uiRange / 2) * valueRange / uiRange;

            var rounded = (int)Math.Round(value, 0) + min.Value;

            if (rounded > max)
                return max.Value;
            if (rounded < min)
                return min.Value;

            return rounded;
        }

        public double SetPosition(int value)
        {
            var valueRange = max.Value - min.Value;
            var uiRange = Track.ActualWidth;

            var uiPosition = (value - min.Value) * uiRange / valueRange;

            return uiPosition - (uiRange / 2);
        }

        public void FillTrack()
        {
            var fillWidth = rightTransform.TranslateX - leftTransform.TranslateX;
            FillTrackGrid.Width = fillWidth > 0 ? fillWidth : 0;

            var x = (Math.Abs(rightTransform.TranslateX) - Math.Abs(leftTransform.TranslateX)) / 2;

            if (leftTransform.TranslateX > 0)
                x += leftTransform.TranslateX;
            else if (rightTransform.TranslateX < 0)
                x += rightTransform.TranslateX;

            fillTransform.TranslateX = x;
        }
    }
}