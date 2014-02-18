// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LabeledSlider.xaml.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// A slider with a text box and labels
    /// </summary>
    public partial class LabeledSlider
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(LabeledSlider), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(double), typeof(LabeledSlider), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(double), typeof(LabeledSlider), new PropertyMetadata(10.0));

        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(
            "SmallChange", typeof(double), typeof(LabeledSlider), new PropertyMetadata(0.01));

        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(
            "LargeChange", typeof(double), typeof(LabeledSlider), new PropertyMetadata(0.1));

        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
            "TickFrequency", typeof(double), typeof(LabeledSlider), new PropertyMetadata(null));

        public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register(
            "IsSnapToTickEnabled", typeof(bool), typeof(LabeledSlider), new PropertyMetadata(null));

        public static readonly DependencyProperty SliderValueProperty = DependencyProperty.Register(
            "SliderValue", typeof(double), typeof(LabeledSlider), new FrameworkPropertyMetadata(1.0) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty UnitsProperty =
            DependencyProperty.Register("Units", typeof(string), typeof(LabeledSlider), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledSlider"/> class. 
        /// </summary>
        public LabeledSlider()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        /// <summary>
        /// Display title of the slider.
        /// </summary>
        public string Title
        {
            get
            {
                return (string)this.GetValue(TitleProperty);
            }

            set
            {
                this.SetValue(TitleProperty, value);
            }
        }

        /// <summary>
        /// Minimum value of the slider.
        /// </summary>
        public double Minimum
        {
            get
            {
                return (double)this.GetValue(MinimumProperty);
            }

            set
            {
                this.SetValue(MinimumProperty, value);
            }
        }

        /// <summary>
        /// Maximum value of the slider.
        /// </summary>
        public double Maximum
        {
            get
            {
                return (double)this.GetValue(MaximumProperty);
            }

            set
            {
                this.SetValue(MaximumProperty, value);
            }
        }

        /// <summary>
        /// Small change increment of the slider.
        /// </summary>
        public double SmallChange
        {
            get
            {
                return (double)this.GetValue(SmallChangeProperty);
            }

            set
            {
                this.SetValue(SmallChangeProperty, value);
            }
        }

        /// <summary>
        /// Large change increment of the slider.
        /// </summary>
        public double LargeChange
        {
            get
            {
                return (double)this.GetValue(LargeChangeProperty);
            }

            set
            {
                this.SetValue(LargeChangeProperty, value);
            }
        }

        /// <summary>
        /// Tick frequency of the slider.
        /// </summary>
        public double TickFrequency
        {
            get
            {
                return (double)this.GetValue(TickFrequencyProperty);
            }

            set
            {
                this.SetValue(TickFrequencyProperty, value);
            }
        }

        /// <summary>
        /// IsSnapToTickEnabled for the slider.
        /// </summary>
        public bool IsSnapToTickEnabled
        {
            get
            {
                return (bool)this.GetValue(IsSnapToTickEnabledProperty);
            }

            set
            {
                this.SetValue(IsSnapToTickEnabledProperty, value);
            }
        }

        /// <summary>
        /// Actual value of the slider and text box.
        /// </summary>
        public double SliderValue
        {
            get
            {
                return (double)this.GetValue(SliderValueProperty);
            }

            set
            {
                this.SetValue(SliderValueProperty, value);
            }
        }

        /// <summary>
        /// String to put in the units box.
        /// </summary>
        public string Units
        {
            get
            {
                return (string)this.GetValue(UnitsProperty);
            }

            set
            {
                this.SetValue(UnitsProperty, value);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                // Force binding update when user presses the Enter key.
                this.textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-click selects all text, not just a "word"
            this.textBox.SelectAll();
        }
    }
}
