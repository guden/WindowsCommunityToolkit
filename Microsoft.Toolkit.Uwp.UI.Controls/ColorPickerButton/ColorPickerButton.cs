﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    [TemplatePart(Name = nameof(ColorPickerButton.AlphaChannelSlider),          Type = typeof(Slider))]
    [TemplatePart(Name = nameof(ColorPickerButton.AlphaChannelTextBox),         Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(ColorPickerButton.Channel1Slider),              Type = typeof(Slider))]
    [TemplatePart(Name = nameof(ColorPickerButton.Channel1TextBox),             Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(ColorPickerButton.Channel2Slider),              Type = typeof(Slider))]
    [TemplatePart(Name = nameof(ColorPickerButton.Channel2TextBox),             Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(ColorPickerButton.Channel3Slider),              Type = typeof(Slider))]
    [TemplatePart(Name = nameof(ColorPickerButton.Channel3TextBox),             Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground1Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground2Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground3Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground4Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground5Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground6Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground7Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground8Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground9Border),  Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.CheckeredBackground10Border), Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.ColorSpectrum),               Type = typeof(ColorSpectrum))]
    [TemplatePart(Name = nameof(ColorPickerButton.ColorSpectrumAlphaSlider),    Type = typeof(Slider))]
    [TemplatePart(Name = nameof(ColorPickerButton.ColorSpectrumThirdDimensionSlider), Type = typeof(Slider))]
    [TemplatePart(Name = nameof(ColorPickerButton.ColorRepresentationComboBox), Type = typeof(ComboBox))]
    [TemplatePart(Name = nameof(ColorPickerButton.HexInputTextBox),             Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(ColorPickerButton.PaletteGridView),             Type = typeof(GridView))]
    [TemplatePart(Name = nameof(ColorPickerButton.P1PreviewBorder),             Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.P2PreviewBorder),             Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.N1PreviewBorder),             Type = typeof(Border))]
    [TemplatePart(Name = nameof(ColorPickerButton.N2PreviewBorder),             Type = typeof(Border))]
    public partial class ColorPickerButton : Windows.UI.Xaml.Controls.ColorPicker
    {
        /// <summary>
        /// The period that scheduled color updates will be applied.
        /// This is only used when updating colors using the ScheduleColorUpdate() method.
        /// Color changes made directly to the Color property will apply instantly.
        /// </summary>
        private const int ColorUpdateInterval = 30; // Milliseconds

        /// <summary>
        /// Defines how colors are represented.
        /// </summary>
        private enum ColorRepresentation
        {
            /// <summary>
            /// Color is represented by hue, saturation, value and alpha channels.
            /// </summary>
            Hsva,

            /// <summary>
            /// Color is represented by red, green, blue and alpha channels.
            /// </summary>
            Rgba
        };

        /// <summary>
        /// Defines a specific channel within a color representation.
        /// </summary>
        private enum ColorChannel
        {
            /// <summary>
            /// Represents the alpha channel.
            /// </summary>
            Alpha,

            /// <summary>
            /// Represents the first color channel which is Red when RGB or Hue when HSV.
            /// </summary>
            Channel1,

            /// <summary>
            /// Represents the second color channel which is Green when RGB or Saturation when HSV.
            /// </summary>
            Channel2,

            /// <summary>
            /// Represents the third color channel which is Blue when RGB or Value when HSV.
            /// </summary>
            Channel3
        }

        private long tokenColor;
        private long tokenCustomPalette;

        private Dictionary<Slider, Size> cachedSliderSizes      = new Dictionary<Slider, Size>();
        private bool                     callbacksConnected     = false;
        private Color                    checkerBackgroundColor = Color.FromArgb(0x19, 0x80, 0x80, 0x80); // Overridden later
        private bool                     eventsConnected        = false;
        private bool                     isInitialized          = false;

        // Color information for updates
        private HsvColor?       savedHsvColor              = null;
        private Color?          savedHsvColorRgbEquivalent = null;
        private Color?          updatedRgbColor            = null;
        private DispatcherTimer dispatcherTimer            = null;

        private ColorSpectrum ColorSpectrum;
        private Slider        ColorSpectrumAlphaSlider;
        private Slider        ColorSpectrumThirdDimensionSlider;
        private ComboBox      ColorRepresentationComboBox;
        private TextBox       HexInputTextBox;
        private GridView      PaletteGridView;

        private TextBox Channel1TextBox;
        private TextBox Channel2TextBox;
        private TextBox Channel3TextBox;
        private TextBox AlphaChannelTextBox;
        private Slider    Channel1Slider;
        private Slider    Channel2Slider;
        private Slider    Channel3Slider;
        private Slider    AlphaChannelSlider;

        private Border N1PreviewBorder;
        private Border N2PreviewBorder;
        private Border P1PreviewBorder;
        private Border P2PreviewBorder;

        // Up to 8 checkered backgrounds may be used by name anywhere in the template
        private Border CheckeredBackground1Border;
        private Border CheckeredBackground2Border;
        private Border CheckeredBackground3Border;
        private Border CheckeredBackground4Border;
        private Border CheckeredBackground5Border;
        private Border CheckeredBackground6Border;
        private Border CheckeredBackground7Border;
        private Border CheckeredBackground8Border;
        private Border CheckeredBackground9Border;
        private Border CheckeredBackground10Border;

        /***************************************************************************************
         *
         * Constructor/Destructor
         *
         ***************************************************************************************/

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorPickerButton()
        {
            this.DefaultStyleKey = typeof(ColorPickerButton);

            // Setup collections
            base.SetValue(CustomPaletteColorsProperty, new ObservableCollection<Color>());
            this.CustomPaletteColors.CollectionChanged += CustomPaletteColors_CollectionChanged;

            this.Loaded += ColorPickerButton_Loaded;

            // Checkered background color is found only one time for performance
            // This may need to change in the future if theme changes should be supported
            this.checkerBackgroundColor = (Color)Application.Current.Resources["SystemListLowColor"];

            this.SetDefaultPalette();

            this.ConnectCallbacks(true);
            this.StartDispatcherTimer();
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~ColorPickerButton()
        {
            this.StopDispatcherTimer();
            this.CustomPaletteColors.CollectionChanged -= CustomPaletteColors_CollectionChanged;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/



        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Overrides when a template is applied in order to get the required controls.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            this.ColorSpectrum                     = this.GetTemplateChild<ColorSpectrum>("ColorSpectrum", false);
            this.ColorSpectrumAlphaSlider          = this.GetTemplateChild<Slider>("ColorSpectrumAlphaSlider", false);
            this.ColorSpectrumThirdDimensionSlider = this.GetTemplateChild<Slider>("ColorSpectrumThirdDimensionSlider", false);

            this.PaletteGridView = this.GetTemplateChild<GridView>("PaletteGridView", false);

            this.ColorRepresentationComboBox = this.GetTemplateChild<ComboBox>("ColorRepresentationComboBox", false);
            this.HexInputTextBox             = this.GetTemplateChild<TextBox>("HexInputTextBox", false);

            this.Channel1TextBox     = this.GetTemplateChild<TextBox>("Channel1TextBox", false);
            this.Channel2TextBox     = this.GetTemplateChild<TextBox>("Channel2TextBox", false);
            this.Channel3TextBox     = this.GetTemplateChild<TextBox>("Channel3TextBox", false);
            this.AlphaChannelTextBox = this.GetTemplateChild<TextBox>("AlphaChannelTextBox", false);

            this.Channel1Slider     = this.GetTemplateChild<Slider>("Channel1Slider", false);
            this.Channel2Slider     = this.GetTemplateChild<Slider>("Channel2Slider", false);
            this.Channel3Slider     = this.GetTemplateChild<Slider>("Channel3Slider", false);
            this.AlphaChannelSlider = this.GetTemplateChild<Slider>("AlphaChannelSlider", false);

            this.N1PreviewBorder = this.GetTemplateChild<Border>("N1PreviewBorder", false);
            this.N2PreviewBorder = this.GetTemplateChild<Border>("N2PreviewBorder", false);
            this.P1PreviewBorder = this.GetTemplateChild<Border>("P1PreviewBorder", false);
            this.P2PreviewBorder = this.GetTemplateChild<Border>("P2PreviewBorder", false);

            this.CheckeredBackground1Border  = this.GetTemplateChild<Border>("CheckeredBackground1Border", false);
            this.CheckeredBackground2Border  = this.GetTemplateChild<Border>("CheckeredBackground2Border", false);
            this.CheckeredBackground3Border  = this.GetTemplateChild<Border>("CheckeredBackground3Border", false);
            this.CheckeredBackground4Border  = this.GetTemplateChild<Border>("CheckeredBackground4Border", false);
            this.CheckeredBackground5Border  = this.GetTemplateChild<Border>("CheckeredBackground5Border", false);
            this.CheckeredBackground6Border  = this.GetTemplateChild<Border>("CheckeredBackground6Border", false);
            this.CheckeredBackground7Border  = this.GetTemplateChild<Border>("CheckeredBackground7Border", false);
            this.CheckeredBackground8Border  = this.GetTemplateChild<Border>("CheckeredBackground8Border", false);
            this.CheckeredBackground9Border  = this.GetTemplateChild<Border>("CheckeredBackground9Border", false);
            this.CheckeredBackground10Border = this.GetTemplateChild<Border>("CheckeredBackground10Border", false);

            // Sync the active color
            if (this.ColorSpectrum != null)
            {
                this.ColorSpectrum.Color = (Color)base.GetValue(ColorProperty);
            }

            // Set initial state
            if (base.IsEnabled == false)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", false);
            }

            // Must connect after controls are resolved
            this.ConnectEvents(true);

            base.OnApplyTemplate();
            this.isInitialized = true;
        }

        /// <summary>
        /// Retrieves the named element in the instantiated ControlTemplate visual tree.
        /// </summary>
        /// <param name="childName">The name of the element to find.</param>
        /// <returns>The template child matching the given name and type.</returns>
        private T GetTemplateChild<T>(string childName, bool isRequired = true) where T : DependencyObject
        {
            T child = base.GetTemplateChild(childName) as T;
            if ((child == null) && isRequired)
            {
                throw new NullReferenceException(childName);
            }

            return (child);
        }

        /// <summary>
        /// Connects or disconnects all dependency property callbacks.
        /// </summary>
        /// <param name="connected">True to connect callbacks, otherwise false.</param>
        private void ConnectCallbacks(bool connected)
        {
            if ((connected == true) &&
                (this.callbacksConnected == false))
            {
                // Add callbacks for dependency properties
                this.tokenColor         = this.RegisterPropertyChangedCallback(ColorProperty,         OnColorChanged);
                this.tokenCustomPalette = this.RegisterPropertyChangedCallback(CustomPaletteProperty, OnCustomPaletteChanged);

                this.callbacksConnected = true;
            }
            else if ((connected == false) &&
                     (this.callbacksConnected == true))
            {
                // Remove callbacks for dependency properties
                this.UnregisterPropertyChangedCallback(ColorProperty,         this.tokenColor);
                this.UnregisterPropertyChangedCallback(CustomPaletteProperty, this.tokenCustomPalette);

                this.callbacksConnected = false;
            }

            return;
        }

        /// <summary>
        /// Connects or disconnects all control event handlers.
        /// </summary>
        /// <param name="connected">True to connect event handlers, otherwise false.</param>
        private void ConnectEvents(bool connected)
        {
            if ((connected == true) &&
                (this.eventsConnected == false))
            {
                // Add all events
                if (this.ColorSpectrum               != null) { this.ColorSpectrum.ColorChanged                   += ColorSpectrum_ColorChanged; }
                if (this.ColorSpectrum               != null) { this.ColorSpectrum.GotFocus                       += ColorSpectrum_GotFocus; }
                if (this.ColorRepresentationComboBox != null) { this.ColorRepresentationComboBox.SelectionChanged += ColorRepresentationComboBox_SelectionChanged; }
                if (this.HexInputTextBox             != null) { this.HexInputTextBox.KeyDown                      += HexInputTextBox_KeyDown; }
                if (this.HexInputTextBox             != null) { this.HexInputTextBox.LostFocus                    += HexInputTextBox_LostFocus; }
                if (this.HexInputTextBox             != null) { this.HexInputTextBox.TextChanged                  += HexInputTextBox_TextChanged; }
                if (this.PaletteGridView             != null) { this.PaletteGridView.Loaded                       += PaletteGridView_Loaded; }

                //if (this.Channel1NumberBox     != null) { this.Channel1NumberBox.ValueChanged     += ChannelNumberBox_ValueChanged; }
                //if (this.Channel2NumberBox     != null) { this.Channel2NumberBox.ValueChanged     += ChannelNumberBox_ValueChanged; }
                //if (this.Channel3NumberBox     != null) { this.Channel3NumberBox.ValueChanged     += ChannelNumberBox_ValueChanged; }
                //if (this.AlphaChannelNumberBox != null) { this.AlphaChannelNumberBox.ValueChanged += ChannelNumberBox_ValueChanged; }

                if (this.Channel1Slider                    != null) { this.Channel1Slider.ValueChanged                    += ChannelSlider_ValueChanged; }
                if (this.Channel2Slider                    != null) { this.Channel2Slider.ValueChanged                    += ChannelSlider_ValueChanged; }
                if (this.Channel3Slider                    != null) { this.Channel3Slider.ValueChanged                    += ChannelSlider_ValueChanged; }
                if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.ValueChanged                += ChannelSlider_ValueChanged; }
                if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.ValueChanged          += ChannelSlider_ValueChanged; }
                if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.ValueChanged += ChannelSlider_ValueChanged; }

                if (this.Channel1Slider                    != null) { this.Channel1Slider.Loaded                    += ChannelSlider_Loaded; }
                if (this.Channel2Slider                    != null) { this.Channel2Slider.Loaded                    += ChannelSlider_Loaded; }
                if (this.Channel3Slider                    != null) { this.Channel3Slider.Loaded                    += ChannelSlider_Loaded; }
                if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.Loaded                += ChannelSlider_Loaded; }
                if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.Loaded          += ChannelSlider_Loaded; }
                if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.Loaded += ChannelSlider_Loaded; }

                if (this.N1PreviewBorder != null) { this.N1PreviewBorder.PointerPressed += PreviewBorder_PointerPressed; }
                if (this.N2PreviewBorder != null) { this.N2PreviewBorder.PointerPressed += PreviewBorder_PointerPressed; }
                if (this.P1PreviewBorder != null) { this.P1PreviewBorder.PointerPressed += PreviewBorder_PointerPressed; }
                if (this.P2PreviewBorder != null) { this.P2PreviewBorder.PointerPressed += PreviewBorder_PointerPressed; }

                if (this.CheckeredBackground1Border  != null) { this.CheckeredBackground1Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground2Border  != null) { this.CheckeredBackground2Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground3Border  != null) { this.CheckeredBackground3Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground4Border  != null) { this.CheckeredBackground4Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground5Border  != null) { this.CheckeredBackground5Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground6Border  != null) { this.CheckeredBackground6Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground7Border  != null) { this.CheckeredBackground7Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground8Border  != null) { this.CheckeredBackground8Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground9Border  != null) { this.CheckeredBackground9Border.Loaded  += CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground10Border != null) { this.CheckeredBackground10Border.Loaded += CheckeredBackgroundBorder_Loaded; }

                this.eventsConnected = true;
            }
            else if ((connected == false) &&
                     (this.eventsConnected == true))
            {
                // Remove all events
                if (this.ColorSpectrum               != null) { this.ColorSpectrum.ColorChanged                   -= ColorSpectrum_ColorChanged; }
                if (this.ColorSpectrum               != null) { this.ColorSpectrum.GotFocus                       -= ColorSpectrum_GotFocus; }
                if (this.ColorRepresentationComboBox != null) { this.ColorRepresentationComboBox.SelectionChanged -= ColorRepresentationComboBox_SelectionChanged; }
                if (this.HexInputTextBox             != null) { this.HexInputTextBox.KeyDown                      -= HexInputTextBox_KeyDown; }
                if (this.HexInputTextBox             != null) { this.HexInputTextBox.LostFocus                    -= HexInputTextBox_LostFocus; }
                if (this.HexInputTextBox             != null) { this.HexInputTextBox.TextChanged                  -= HexInputTextBox_TextChanged; }
                if (this.PaletteGridView             != null) { this.PaletteGridView.Loaded                       -= PaletteGridView_Loaded; }

                //if (this.Channel1TextBox     != null) { this.Channel1TextBox.ValueChanged     -= ChannelTextBox_ValueChanged; }
                //if (this.Channel2TextBox     != null) { this.Channel2TextBox.ValueChanged     -= ChannelTextBox_ValueChanged; }
                //if (this.Channel3TextBox     != null) { this.Channel3TextBox.ValueChanged     -= ChannelTextBox_ValueChanged; }
                //if (this.AlphaChannelTextBox != null) { this.AlphaChannelTextBox.ValueChanged -= ChannelTextBox_ValueChanged; }

                if (this.Channel1Slider                    != null) { this.Channel1Slider.ValueChanged                    -= ChannelSlider_ValueChanged; }
                if (this.Channel2Slider                    != null) { this.Channel2Slider.ValueChanged                    -= ChannelSlider_ValueChanged; }
                if (this.Channel3Slider                    != null) { this.Channel3Slider.ValueChanged                    -= ChannelSlider_ValueChanged; }
                if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.ValueChanged                -= ChannelSlider_ValueChanged; }
                if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.ValueChanged          -= ChannelSlider_ValueChanged; }
                if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.ValueChanged -= ChannelSlider_ValueChanged; }

                if (this.Channel1Slider                    != null) { this.Channel1Slider.Loaded                    -= ChannelSlider_Loaded; }
                if (this.Channel2Slider                    != null) { this.Channel2Slider.Loaded                    -= ChannelSlider_Loaded; }
                if (this.Channel3Slider                    != null) { this.Channel3Slider.Loaded                    -= ChannelSlider_Loaded; }
                if (this.AlphaChannelSlider                != null) { this.AlphaChannelSlider.Loaded                -= ChannelSlider_Loaded; }
                if (this.ColorSpectrumAlphaSlider          != null) { this.ColorSpectrumAlphaSlider.Loaded          -= ChannelSlider_Loaded; }
                if (this.ColorSpectrumThirdDimensionSlider != null) { this.ColorSpectrumThirdDimensionSlider.Loaded -= ChannelSlider_Loaded; }

                if (this.N1PreviewBorder != null) { this.N1PreviewBorder.PointerPressed -= PreviewBorder_PointerPressed; }
                if (this.N2PreviewBorder != null) { this.N2PreviewBorder.PointerPressed -= PreviewBorder_PointerPressed; }
                if (this.P1PreviewBorder != null) { this.P1PreviewBorder.PointerPressed -= PreviewBorder_PointerPressed; }
                if (this.P2PreviewBorder != null) { this.P2PreviewBorder.PointerPressed -= PreviewBorder_PointerPressed; }

                if (this.CheckeredBackground1Border  != null) { this.CheckeredBackground1Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground2Border  != null) { this.CheckeredBackground2Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground3Border  != null) { this.CheckeredBackground3Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground4Border  != null) { this.CheckeredBackground4Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground5Border  != null) { this.CheckeredBackground5Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground6Border  != null) { this.CheckeredBackground6Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground7Border  != null) { this.CheckeredBackground7Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground8Border  != null) { this.CheckeredBackground8Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground9Border  != null) { this.CheckeredBackground9Border.Loaded  -= CheckeredBackgroundBorder_Loaded; }
                if (this.CheckeredBackground10Border != null) { this.CheckeredBackground10Border.Loaded -= CheckeredBackgroundBorder_Loaded; }

                this.eventsConnected = false;
            }

            return;
        }

        /// <summary>
        /// Gets the active representation of the color: HSV or RGB.
        /// </summary>
        private ColorRepresentation GetActiveColorRepresentation()
        {
            // This is kind-of an ugly way to see if the HSV color channel representation is active
            // However, it is the same technique used in the ColorPicker
            // The order and number of items in the template is fixed and very important
            if ((this.ColorRepresentationComboBox != null) &&
                (this.ColorRepresentationComboBox.SelectedIndex == 1))
            {
                return ColorRepresentation.Hsva;
            }

            return ColorRepresentation.Rgba;
        }

        /// <summary>
        /// Gets the active third dimension in the color spectrum: Hue, Saturation or Value.
        /// </summary>
        private ColorChannel GetActiveColorSpectrumThirdDimension()
        {
            switch (this.ColorSpectrumComponents)
            {
                case Windows.UI.Xaml.Controls.ColorSpectrumComponents.SaturationValue:
                case Windows.UI.Xaml.Controls.ColorSpectrumComponents.ValueSaturation:
                    {
                        // Hue
                        return ColorChannel.Channel1;
                    }
                case Windows.UI.Xaml.Controls.ColorSpectrumComponents.HueValue:
                case Windows.UI.Xaml.Controls.ColorSpectrumComponents.ValueHue:
                    {
                        // Saturation
                        return ColorChannel.Channel2;
                    }
                case Windows.UI.Xaml.Controls.ColorSpectrumComponents.HueSaturation:
                case Windows.UI.Xaml.Controls.ColorSpectrumComponents.SaturationHue:
                    {
                        // Value
                        return ColorChannel.Channel3;
                    }
            }

            return ColorChannel.Alpha; // Error, should never get here
        }

        /// <summary>
        /// Gets whether or not the color is considered empty (all fields zero).
        /// In the future Color.IsEmpty will hopefully be added to UWP.
        /// </summary>
        /// <param name="color">The Windows.UI.Color to calculate with.</param>
        /// <returns>Whether the color is considered empty.</returns>
        private static bool IsColorEmpty(Color color)
        {
            return color.A == 0x00 &&
                   color.R == 0x00 &&
                   color.G == 0x00 &&
                   color.B == 0x00;
        }

        /// <summary>
        /// Declares a new color to set to the control.
        /// Application of this color will be scheduled to avoid overly rapid updates.
        /// </summary>
        /// <param name="newColor">The new color to set to the control. </param>
        private void ScheduleColorUpdate(Color newColor)
        {
            // Coerce the value as needed
            if (this.IsAlphaEnabled == false)
            {
                newColor = new Color()
                {
                    R = newColor.R,
                    G = newColor.G,
                    B = newColor.B,
                    A = 255
                };
            }

            this.updatedRgbColor = newColor;

            return;
        }

        /// <summary>
        /// Updates the color channel values in all editing controls to match the current color.
        /// </summary>
        private void UpdateChannelControlValues()
        {
            bool eventsDisconnectedByMethod = false;
            Color rgbColor = this.Color;
            HsvColor hsvColor;

            if (this.isInitialized)
            {
                // Disable events during the update
                if (this.eventsConnected)
                {
                    this.ConnectEvents(false);
                    eventsDisconnectedByMethod = true;
                }

                this.HexInputTextBox.Text = rgbColor.ToHex().Replace("#", "");

                // Regardless of the active color representation, the spectrum is always HSV
                // Therefore, always calculate HSV color here
                // Warning: Always maintain/use HSV information in the saved HSV color
                // This avoids loss of precision and drift caused by continuously converting to/from RGB
                if (this.savedHsvColor == null)
                {
                    hsvColor = rgbColor.ToHsv();

                    // Round the channels, be sure rounding matches with the scaling next
                    // Rounding of SVA requires at MINIMUM 2 decimal places
                    int decimals = 0;
                    hsvColor = new HsvColor()
                    {
                        H = Math.Round(hsvColor.H, decimals),
                        S = Math.Round(hsvColor.S, 2 + decimals),
                        V = Math.Round(hsvColor.V, 2 + decimals),
                        A = Math.Round(hsvColor.A, 2 + decimals)
                    };

                    // Must update HSV color
                    this.savedHsvColor              = hsvColor;
                    this.savedHsvColorRgbEquivalent = rgbColor;
                }
                else
                {
                    hsvColor = this.savedHsvColor.Value;
                }

                // Update the color spectrum third dimension channel
                if (this.ColorSpectrumThirdDimensionSlider != null)
                {
                    // Convert the channels into a usable range for the user
                    double hue         = hsvColor.H;
                    double staturation = hsvColor.S * 100;
                    double value       = hsvColor.V * 100;

                    switch (this.GetActiveColorSpectrumThirdDimension())
                    {
                        case ColorChannel.Channel1:
                            {
                                // Hue
                                this.ColorSpectrumThirdDimensionSlider.Minimum = 0;
                                this.ColorSpectrumThirdDimensionSlider.Maximum = 360;
                                this.ColorSpectrumThirdDimensionSlider.Value   = hue;
                                break;
                            }
                        case ColorChannel.Channel2:
                            {
                                // Saturation
                                this.ColorSpectrumThirdDimensionSlider.Minimum = 0;
                                this.ColorSpectrumThirdDimensionSlider.Maximum = 100;
                                this.ColorSpectrumThirdDimensionSlider.Value   = staturation;
                                break;
                            }
                        case ColorChannel.Channel3:
                            {
                                // Value
                                this.ColorSpectrumThirdDimensionSlider.Minimum = 0;
                                this.ColorSpectrumThirdDimensionSlider.Maximum = 100;
                                this.ColorSpectrumThirdDimensionSlider.Value   = value;
                                break;
                            }
                    }
                }

                // Update all other color channels
                if (this.GetActiveColorRepresentation() == ColorRepresentation.Hsva)
                {
                    // Convert the channels into a usable range for the user
                    double hue         = hsvColor.H;
                    double staturation = hsvColor.S * 100;
                    double value       = hsvColor.V * 100;
                    double alpha       = hsvColor.A * 100;

                    // Hue
                    if (this.Channel1TextBox != null)
                    {
                        this.Channel1TextBox.Text = hue.ToString();
                    }

                    if (this.Channel1Slider != null)
                    {
                        this.Channel1Slider.Minimum = 0;
                        this.Channel1Slider.Maximum = 360;
                        this.Channel1Slider.Value   = hue;
                    }

                    // Saturation
                    if (this.Channel2TextBox != null)
                    {
                        this.Channel2TextBox.Text = staturation.ToString();
                    }

                    if (this.Channel2Slider != null)
                    {
                        this.Channel2Slider.Minimum = 0;
                        this.Channel2Slider.Maximum = 100;
                        this.Channel2Slider.Value   = staturation;
                    }

                    // Value
                    if (this.Channel3TextBox != null)
                    {
                        this.Channel3TextBox.Text = value.ToString();
                    }

                    if (this.Channel3Slider != null)
                    {
                        this.Channel3Slider.Minimum = 0;
                        this.Channel3Slider.Maximum = 100;
                        this.Channel3Slider.Value   = value;
                    }

                    // Alpha
                    if (this.AlphaChannelTextBox != null)
                    {
                        this.AlphaChannelTextBox.Text = alpha.ToString();
                    }

                    if (this.AlphaChannelSlider != null)
                    {
                        this.AlphaChannelSlider.Minimum = 0;
                        this.AlphaChannelSlider.Maximum = 100;
                        this.AlphaChannelSlider.Value   = alpha;
                    }

                    // Color spectrum alpha
                    if (this.ColorSpectrumAlphaSlider != null)
                    {
                        this.ColorSpectrumAlphaSlider.Minimum = 0;
                        this.ColorSpectrumAlphaSlider.Maximum = 100;
                        this.ColorSpectrumAlphaSlider.Value   = alpha;
                    }
                }
                else
                {
                    // Red
                    if (this.Channel1TextBox != null)
                    {
                        this.Channel1TextBox.Text = rgbColor.R.ToString();;
                    }

                    if (this.Channel1Slider != null)
                    {
                        this.Channel1Slider.Minimum = 0;
                        this.Channel1Slider.Maximum = 255;
                        this.Channel1Slider.Value   = Convert.ToDouble(rgbColor.R);
                    }

                    // Green
                    if (this.Channel2TextBox != null)
                    {
                        this.Channel2TextBox.Text = rgbColor.G.ToString();;
                    }

                    if (this.Channel2Slider != null)
                    {
                        this.Channel2Slider.Minimum = 0;
                        this.Channel2Slider.Maximum = 255;
                        this.Channel2Slider.Value   = Convert.ToDouble(rgbColor.G);
                    }

                    // Blue
                    if (this.Channel3TextBox != null)
                    {
                        this.Channel3TextBox.Text = rgbColor.B.ToString();;
                    }

                    if (this.Channel3Slider != null)
                    {
                        this.Channel3Slider.Minimum = 0;
                        this.Channel3Slider.Maximum = 255;
                        this.Channel3Slider.Value   = Convert.ToDouble(rgbColor.B);
                    }

                    // Alpha
                    if (this.AlphaChannelTextBox != null)
                    {
                        this.AlphaChannelTextBox.Text = rgbColor.A.ToString();;
                    }

                    if (this.AlphaChannelSlider != null)
                    {
                        this.AlphaChannelSlider.Minimum = 0;
                        this.AlphaChannelSlider.Maximum = 255;
                        this.AlphaChannelSlider.Value   = Convert.ToDouble(rgbColor.A);
                    }

                    // Color spectrum alpha
                    if (this.ColorSpectrumAlphaSlider != null)
                    {
                        this.ColorSpectrumAlphaSlider.Minimum = 0;
                        this.ColorSpectrumAlphaSlider.Maximum = 255;
                        this.ColorSpectrumAlphaSlider.Value   = Convert.ToDouble(rgbColor.A);
                    }
                }

                if (eventsDisconnectedByMethod)
                {
                    this.ConnectEvents(true);
                }
            }

            return;
        }

        /// <summary>
        /// Sets a new color channel value to the current color.
        /// Only the specified color channel will be modified.
        /// </summary>
        /// <param name="colorRepresentation">The color representation of the given channel.</param>
        /// <param name="channel">The specified color channel to modify.</param>
        /// <param name="newValue">The new color channel value.</param>
        private void SetColorChannel(ColorRepresentation colorRepresentation,
                                     ColorChannel channel,
                                     double newValue)
        {
            Color oldRgbColor = this.Color;
            Color newRgbColor;
            HsvColor oldHsvColor;

            if (colorRepresentation == ColorRepresentation.Hsva)
            {
                // Warning: Always maintain/use HSV information in the saved HSV color
                // This avoids loss of precision and drift caused by continuously converting to/from RGB
                if (this.savedHsvColor == null)
                {
                    oldHsvColor = oldRgbColor.ToHsv();
                }
                else
                {
                    oldHsvColor = this.savedHsvColor.Value;
                }

                double hue        = oldHsvColor.H;
                double saturation = oldHsvColor.S;
                double value      = oldHsvColor.V;
                double alpha      = oldHsvColor.A;

                switch (channel)
                {
                    case ColorChannel.Channel1:
                        {
                            hue = Math.Clamp((double.IsNaN(newValue) ? 0 : newValue), 0, 360);
                            break;
                        }
                    case ColorChannel.Channel2:
                        {
                            saturation = Math.Clamp((double.IsNaN(newValue) ? 0 : newValue) / 100, 0, 1);
                            break;
                        }
                    case ColorChannel.Channel3:
                        {
                            value = Math.Clamp((double.IsNaN(newValue) ? 0 : newValue) / 100, 0, 1);
                            break;
                        }
                    case ColorChannel.Alpha:
                        {
                            // Unlike color channels, default to no transparency
                            alpha = Math.Clamp((double.IsNaN(newValue) ? 100 : newValue) / 100, 0, 1);
                            break;
                        }
                }

                newRgbColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv(hue,
                                                                                saturation,
                                                                                value,
                                                                                alpha);

                // Must update HSV color
                this.savedHsvColor = new HsvColor()
                {
                    H = hue,
                    S = saturation,
                    V = value,
                    A = alpha
                };
                this.savedHsvColorRgbEquivalent = newRgbColor;
            }
            else
            {
                byte red   = oldRgbColor.R;
                byte green = oldRgbColor.G;
                byte blue  = oldRgbColor.B;
                byte alpha = oldRgbColor.A; 

                switch (channel)
                {
                    case ColorChannel.Channel1:
                        {
                            red = Convert.ToByte(Math.Clamp((double.IsNaN(newValue) ? 0 : newValue), 0, 255));
                            break;
                        }
                    case ColorChannel.Channel2:
                        {
                            green = Convert.ToByte(Math.Clamp((double.IsNaN(newValue) ? 0 : newValue), 0, 255));
                            break;
                        }
                    case ColorChannel.Channel3:
                        {
                            blue = Convert.ToByte(Math.Clamp((double.IsNaN(newValue) ? 0 : newValue), 0, 255));
                            break;
                        }
                    case ColorChannel.Alpha:
                        {
                            // Unlike color channels, default to no transparency
                            alpha = Convert.ToByte(Math.Clamp((double.IsNaN(newValue) ? 255 : newValue), 0, 255));
                            break;
                        }
                }

                newRgbColor = new Color()
                {
                    R = red,
                    G = green,
                    B = blue,
                    A = alpha
                };

                // Must clear saved HSV color
                this.savedHsvColor              = null;
                this.savedHsvColorRgbEquivalent = null;
            }

            this.ScheduleColorUpdate(newRgbColor);
            return;
        }

        /// <summary>
        /// Updates all channel slider control backgrounds.
        /// </summary>
        private void UpdateChannelSliderBackgrounds()
        {
            this.UpdateChannelSliderBackground(this.Channel1Slider);
            this.UpdateChannelSliderBackground(this.Channel2Slider);
            this.UpdateChannelSliderBackground(this.Channel3Slider);
            this.UpdateChannelSliderBackground(this.AlphaChannelSlider);
            this.UpdateChannelSliderBackground(this.ColorSpectrumAlphaSlider);
            this.UpdateChannelSliderBackground(this.ColorSpectrumThirdDimensionSlider);
            return;
        }

        /// <summary>
        /// Generates a new background image for the specified color channel slider and applies it.
        /// A new image will only be generated if it differs from the last color used to generate a background.
        /// This provides some performance improvement.
        /// </summary>
        /// <param name="slider">The color channel slider to apply the generated background to.</param>
        private async void UpdateChannelSliderBackground(Slider slider)
        {
            byte[] bitmap = null;
            int width = 0;
            int height = 0;
            Color baseColor = this.Color;

            // Updates may be requested when sliders are not in the visual tree.
            // For first-time load this is handled by the Loaded event.
            // However, after that problems may arise, consider the following case:
            // 
            //   (1) Backgrounds are drawn normally the first time on Loaded.
            //       Actual height/width are available.
            //   (2) The palette tab is selected which has no sliders
            //   (3) The picker flyout is closed
            //   (4) Externally the color is changed
            //       The color change will trigger slider background updates but
            //       with the flyout closed, actual height/width are zero. 
            //       No zero size bitmap can be generated.
            //   (5) The picker flyout is re-opened by the user and the default
            //       last-opened tab will be viewed: palette.
            //       No loaded events will be fired for sliders. The color change
            //       event was already handled in (4). The sliders will never
            //       be updated.
            // 
            // In this case the sliders become out of sync with the Color because there is no way 
            // to tell when they actually come into view. To work around this, force a re-render of 
            // the background with the last size of the slider. This last size will be when it was 
            // last loaded or updated.
            // 
            // In the future additional consideration may be required for SizeChanged of the control.
            // This work-around will also cause issues if display scaling changes in the special
            // case where cached sizes are required.
            if (slider != null)
            {
                width  = Convert.ToInt32(slider.ActualWidth);
                height = Convert.ToInt32(slider.ActualHeight);

                if (width == 0 || height == 0)
                {
                    // Attempt to use the last size if it was available
                    if (this.cachedSliderSizes.ContainsKey(slider))
                    {
                        Size cachedSize = this.cachedSliderSizes[slider];
                        width  = Convert.ToInt32(cachedSize.Width);
                        height = Convert.ToInt32(cachedSize.Height);
                    }
                }
                else
                {
                    // Update the cached slider size
                    if (this.cachedSliderSizes.ContainsKey(slider))
                    {
                        this.cachedSliderSizes[slider] = new Size(width, height);
                    }
                    else
                    {
                        this.cachedSliderSizes.Add(slider, new Size(width, height));
                    }
                }
            }

            if (object.ReferenceEquals(slider, this.Channel1Slider))
            {
                bitmap = await this.CreateChannelBitmapAsync(width,
                                                             height,
                                                             Orientation.Horizontal,
                                                             this.GetActiveColorRepresentation(),
                                                             ColorChannel.Channel1,
                                                             baseColor,
                                                             this.checkerBackgroundColor);
            }
            else if (object.ReferenceEquals(slider, this.Channel2Slider))
            {
                bitmap = await this.CreateChannelBitmapAsync(width,
                                                             height,
                                                             Orientation.Horizontal,
                                                             this.GetActiveColorRepresentation(),
                                                             ColorChannel.Channel2,
                                                             baseColor,
                                                             this.checkerBackgroundColor);
            }
            else if (object.ReferenceEquals(slider, this.Channel3Slider))
            {
                bitmap = await this.CreateChannelBitmapAsync(width,
                                                             height,
                                                             Orientation.Horizontal,
                                                             this.GetActiveColorRepresentation(),
                                                             ColorChannel.Channel3,
                                                             baseColor,
                                                             this.checkerBackgroundColor);
            }
            else if (object.ReferenceEquals(slider, this.AlphaChannelSlider))
            {
                bitmap = await this.CreateChannelBitmapAsync(width,
                                                             height,
                                                             Orientation.Horizontal,
                                                             this.GetActiveColorRepresentation(),
                                                             ColorChannel.Alpha,
                                                             baseColor,
                                                             this.checkerBackgroundColor);
            }
            else if (object.ReferenceEquals(slider, this.ColorSpectrumAlphaSlider))
            {
                bitmap = await this.CreateChannelBitmapAsync(width,
                                                             height,
                                                             Orientation.Vertical,
                                                             this.GetActiveColorRepresentation(),
                                                             ColorChannel.Alpha,
                                                             baseColor,
                                                             this.checkerBackgroundColor);
            }
            else if (object.ReferenceEquals(slider, this.ColorSpectrumThirdDimensionSlider))
            {
                bitmap = await this.CreateChannelBitmapAsync(width,
                                                             height,
                                                             Orientation.Vertical,
                                                             ColorRepresentation.Hsva, // Always HSV
                                                             this.GetActiveColorSpectrumThirdDimension(),
                                                             baseColor,
                                                             this.checkerBackgroundColor);
            }

            if (bitmap != null)
            {
                slider.Background = await this.BitmapToBrushAsync(bitmap, width, height);
            }

            return;
        }

        /// <summary>
        /// Sets the default color palette to the control.
        /// </summary>
        private void SetDefaultPalette()
        {
            this.CustomPalette = null;

            this.CustomPaletteColors.Add(Colors.Black);
            this.CustomPaletteColors.Add(Colors.Gray);
            this.CustomPaletteColors.Add(Colors.Silver);
            this.CustomPaletteColors.Add(Colors.White);
            this.CustomPaletteColors.Add(Colors.Maroon);
            this.CustomPaletteColors.Add(Colors.Red);
            this.CustomPaletteColors.Add(Colors.Olive);
            this.CustomPaletteColors.Add(Colors.Yellow);
            this.CustomPaletteColors.Add(Colors.Green);
            this.CustomPaletteColors.Add(Colors.Lime);
            this.CustomPaletteColors.Add(Colors.Teal);
            this.CustomPaletteColors.Add(Colors.Aqua);
            this.CustomPaletteColors.Add(Colors.Navy);
            this.CustomPaletteColors.Add(Colors.Blue);
            this.CustomPaletteColors.Add(Colors.Purple);
            this.CustomPaletteColors.Add(Colors.Fuchsia);

            this.CustomPaletteSectionCount = 4;

            return;
        }

        

        /***************************************************************************************
         *
         * Color Update Timer
         *
         ***************************************************************************************/

        private void StartDispatcherTimer()
        {
            this.dispatcherTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, ColorUpdateInterval)
            };
            this.dispatcherTimer.Tick += DispatcherTimer_Tick;
            this.dispatcherTimer.Start();

            return;
        }

        private void StopDispatcherTimer()
        {
            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
            }
            return;
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            if (this.updatedRgbColor != null)
            {
                var newColor = this.updatedRgbColor.Value;

                // Clear first to avoid timing issues if it takes longer than the timer interval to set the new color
                this.updatedRgbColor = null;

                // An equality check here is important
                // Without it, OnColorChanged would continuously be invoked and preserveHsvColor overwritten when not wanted
                if (object.Equals(newColor, base.GetValue(ColorProperty)) == false)
                {
                    // Disable events here so the color update isn't repeated as other controls in the UI are updated through binding.
                    // For example, the Spectrum should be bound to Color, as soon as Color is changed here the Spectrum is updated.
                    // Then however, the ColorSpectrum.ColorChanged event would fire which would schedule a new color update -- 
                    // with the same color. This causes several problems:
                    //   1. Layout cycle that may crash the app
                    //   2. A performance hit recalculating for no reason
                    //   3. preserveHsvColor gets overwritten unexpectedly by the ColorChanged handler
                    this.ConnectEvents(false);
                    base.SetValue(ColorProperty, newColor);
                    this.ConnectEvents(true);
                }
            }

            return;
        }

        /***************************************************************************************
         *
         * Callbacks
         *
         ***************************************************************************************/

        /// <summary>
        /// Callback for when the <see cref="Color"/> dependency property value changes.
        /// </summary>
        private void OnColorChanged(DependencyObject d, DependencyProperty e)
        {
            // TODO: Coerce the value if Alpha is disabled, is this handled in the base ColorPicker?
            if ((this.savedHsvColor != null) &&
                (object.Equals(d.GetValue(e), this.savedHsvColorRgbEquivalent) == false))
            {
                // The color was updated from an unknown source
                // The RGB and HSV colors are no longer in sync so the HSV color must be cleared
                this.savedHsvColor              = null;
                this.savedHsvColorRgbEquivalent = null;
            }

            this.UpdateChannelControlValues();
            this.UpdateChannelSliderBackgrounds();

            return;
        }

        /// <summary>
        /// Callback for when the <see cref="CustomPalette"/> dependency property value changes.
        /// </summary>
        private void OnCustomPaletteChanged(DependencyObject d, DependencyProperty e)
        {
            IColorPalette palette = this.CustomPalette;

            if (palette != null)
            {
                this.CustomPaletteSectionCount = palette.ColorCount;
                this.CustomPaletteColors.Clear();

                for (int shadeIndex = 0; shadeIndex < palette.ShadeCount; shadeIndex++)
                {
                    for (int colorIndex = 0; colorIndex < palette.ColorCount; colorIndex++)
                {
                        this.CustomPaletteColors.Add(palette.GetColor(colorIndex, shadeIndex));
                    }
                }
            }

            return;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/

        /// <summary>
        /// Event handler for when the control has finished loaded.
        /// </summary>
        private void ColorPickerButton_Loaded(object sender, RoutedEventArgs e)
        {
            // Available but not currently used
            return;
        }

        /// <summary>
        /// Event handler for when a color channel slider is loaded.
        /// This will draw an initial background.
        /// </summary>
        private void ChannelSlider_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateChannelSliderBackground(sender as Slider);
            return;
        }

        /// <summary>
        /// Event handler to draw checkered backgrounds on-demand as controls are loaded.
        /// </summary>
        private async void CheckeredBackgroundBorder_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;
            int width;
            int height;

            if (border != null)
            {
                width  = Convert.ToInt32(border.ActualWidth);
                height = Convert.ToInt32(border.ActualHeight);

                var bitmap = await this.CreateCheckeredBitmapAsync(width,
                                                                   height,
                                                                   this.checkerBackgroundColor);

                if (bitmap != null)
                {
                    border.Background = await this.BitmapToBrushAsync(bitmap, width, height);
                }
            }

            return;
        }

        /// <summary>
        /// Event handler for when the grid view showing all colors in the palette is loaded.
        /// This will set the correct column count to any UniformGrid panel.
        /// </summary>
        private void PaletteGridView_Loaded(object sender, RoutedEventArgs e)
        {
            // RelativeSource binding of an ancestor type doesn't work in UWP.
            // Therefore, setting this property must be done here in code-behind.
            var palettePanel = (sender as DependencyObject)?.FindDescendant<UniformGrid>();

            if (palettePanel != null)
            {
                palettePanel.Columns = this.CustomPaletteSectionCount;
            }

            return;
        }

        /// <summary>
        /// Event handler for when the list of custom palette colors is changed.
        /// </summary>
        private void CustomPaletteColors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Available but not currently used
            return;
        }

        /// <summary>
        /// Event handler for when the color spectrum color is changed.
        /// This occurs when the user presses on the spectrum to select a new color.
        /// </summary>
        private void ColorSpectrum_ColorChanged(ColorSpectrum sender, Windows.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            this.ScheduleColorUpdate(this.ColorSpectrum.Color);
            return;
        }

        /// <summary>
        /// Event handler for when the color spectrum is focused.
        /// This is used only to work around some bugs that cause usability problems.
        /// </summary>
        private void ColorSpectrum_GotFocus(object sender, RoutedEventArgs e)
        {
            /* If this control has a color that is currently empty (#00000000),
             * selecting a new color directly in the spectrum will fail. This is
             * a bug in the color spectrum. Selecting a new color in the spectrum will 
             * keep zero for all channels (including alpha and the third dimension).
             * 
             * In practice this means a new color cannot be selected using the spectrum
             * until both the alpha and third dimension slider are raised above zero.
             * This is extremely user unfriendly and must be corrected as best as possible.
             * 
             * In order to work around this, detect when the color spectrum has selected
             * a new color and then automatically set the alpha and third dimension 
             * channel to maximum. However, the color spectrum has a second bug, the
             * ColorChanged event is never raised if the color is empty. This prevents
             * automatically setting the other channels where it normally should be done
             * (in the ColorChanged event).
             * 
             * In order to work around this second bug, the GotFocus event is used
             * to detect when the spectrum is engaged by the user. It's somewhat equivalent
             * to ColorChanged for this purpose. Then when the GotFocus event is fired
             * set the alpha and third channel values to maximum. The problem here is that
             * the GotFocus event does not have access to the new color that was selected
             * in the spectrum. It is not available due to the afore mentioned bug or due to
             * timing. This means the best that can be done is to just set a 'neutral'
             * color such as white.
             * 
             * There is still a small usability issue with this as it requires two 
             * presses to set a color. That's far better than having to slide up both 
             * sliders though.
             * 
             *  1. If the color is empty, the first press on the spectrum will set white
             *     and ignore the pressed color on the spectrum
             *  2. The second press on the spectrum will be correctly handled.
             * 
             */

            if (IsColorEmpty(this.Color)) // In the future Color.IsEmpty will hopefully be added to UWP
            {
                // The following code may be used in the future if ever the selected color is available
                //
                //Color newColor = this.ColorSpectrum.Color;
                //HsvColor newHsvColor = newColor.ToHsv();

                //switch (this.GetActiveColorSpectrumThirdDimension())
                //{
                //    case ColorChannel.Channel1:
                //        {
                //            newColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv
                //            (
                //                360.0,
                //                newHsvColor.S,
                //                newHsvColor.V,
                //                100.0
                //            );
                //            break;
                //        }
                //    case ColorChannel.Channel2:
                //        {
                //            newColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv
                //            (
                //                newHsvColor.H,
                //                100.0,
                //                newHsvColor.V,
                //                100.0
                //            );
                //            break;
                //        }
                //    case ColorChannel.Channel3:
                //        {
                //            newColor = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv
                //            (
                //                newHsvColor.H,
                //                newHsvColor.S,
                //                100.0,
                //                100.0
                //            );
                //            break;
                //        }
                //}

                this.Color = Colors.White;
            }

            return;
        }

        /// <summary>
        /// Event handler for when the selection changes within the color representation ComboBox.
        /// This will convert between RGB and HSV.
        /// </summary>
        private void ColorRepresentationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateChannelControlValues();
            this.UpdateChannelSliderBackgrounds();
            return;
        }

        /// <summary>
        /// Event handler for when a preview color panel is pressed.
        /// This will update the color to the background of the pressed panel.
        /// </summary>
        private void PreviewBorder_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Border border = sender as Border;

            if (border?.Background is SolidColorBrush brush)
            {
                this.ScheduleColorUpdate(brush.Color);
            }

            return;
        }

        /// <summary>
        /// Event handler for when a key is pressed within the Hex RGB value TextBox.
        /// This is used to trigger re-evaluation of the color based on the TextBox value.
        /// </summary>
        private void HexInputTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                try
                {
                    ColorToHexConverter converter = new ColorToHexConverter();
                    this.Color = (Color)converter.ConvertBack(((TextBox)sender).Text, typeof(TextBox), null, null);
                }
                catch
                {
                    // Reset hex value
                    this.UpdateChannelControlValues();
                    this.UpdateChannelSliderBackgrounds();
                }
            }

            return;
        }

        /// <summary>
        /// Event handler for when the Hex RGB value TextBox looses focus.
        /// This is used to trigger re-evaluation of the color based on the TextBox value.
        /// </summary>
        private void HexInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ColorToHexConverter converter = new ColorToHexConverter();
                this.Color = (Color)converter.ConvertBack(((TextBox)sender).Text, typeof(TextBox), null, null);
            }
            catch
            {
                // Reset hex value
                this.UpdateChannelControlValues();
                this.UpdateChannelSliderBackgrounds();
            }

            return;
        }

        private void HexInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Use LostFocus and KeyDown events instead
            return;
        }

        /// <summary>
        /// Event handler for when the value within one of the channel NumberBoxes is changed.
        /// </summary>
        //private void ChannelNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        //{
        //    double senderValue = sender.Value;

        //    if (object.ReferenceEquals(sender, this.Channel1TextBox))
        //    {
        //        this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel1, senderValue);
        //    }
        //    else if (object.ReferenceEquals(sender, this.Channel2TextBox))
        //    {
        //        this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel2, senderValue);
        //    }
        //    else if (object.ReferenceEquals(sender, this.Channel3TextBox))
        //    {
        //        this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel3, senderValue);
        //    }
        //    else if (object.ReferenceEquals(sender, this.AlphaChannelTextBox))
        //    {
        //        this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Alpha, senderValue);
        //    }

        //    return;
        //}

        /// <summary>
        /// Event handler for when the value within one of the channel Sliders is changed.
        /// </summary>
        private void ChannelSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double senderValue = (sender as Slider)?.Value ?? double.NaN;

            if (object.ReferenceEquals(sender, this.Channel1Slider))
            {
                this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel1, senderValue);
            }
            else if (object.ReferenceEquals(sender, this.Channel2Slider))
            {
                this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel2, senderValue);
            }
            else if (object.ReferenceEquals(sender, this.Channel3Slider))
            {
                this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Channel3, senderValue);
            }
            else if (object.ReferenceEquals(sender, this.AlphaChannelSlider))
            {
                this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Alpha, senderValue);
            }
            else if (object.ReferenceEquals(sender, this.ColorSpectrumAlphaSlider))
            {
                this.SetColorChannel(this.GetActiveColorRepresentation(), ColorChannel.Alpha, senderValue);
            }
            else if (object.ReferenceEquals(sender, this.ColorSpectrumThirdDimensionSlider))
            {
                // Regardless of the active color representation, the spectrum is always HSV
                this.SetColorChannel(ColorRepresentation.Hsva, this.GetActiveColorSpectrumThirdDimension(), senderValue);
            }

            return;
        }
    }

    /// <summary>
    /// Adjust the value component of a color in the HSV model.
    /// The value % change must be supplied using the parameter.
    /// 0 = 0% and 100 = 100% with value clipped to this scale.
    /// Both positive and negative adjustments are supported as this applies
    /// the delta to the existing color.
    /// </summary>
    public class HsvValueAdjustmentConverter : IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              string language)
        {
            double valueDelta;
            HsvColor hsvColor;

            // Get the value component delta
            try
            {
                valueDelta = System.Convert.ToDouble(parameter?.ToString());
            }
            catch
            {
                throw new ArgumentException("Invalid parameter provided, unable to convert to double");
            }

            // Get the current color in HSV
            try
            {
                hsvColor = ((Color)value).ToHsv();
            }
            catch
            {
                throw new ArgumentException("Invalid color value provided, unable to convert to HsvColor");
            }

            // Add the value delta to the HSV color and convert it back to RGB
            hsvColor = new HsvColor()
            {
                H = hsvColor.H,
                S = hsvColor.S,
                V = Math.Clamp(hsvColor.V + (valueDelta / 100.0), 0.0, 1.0),
                A = hsvColor.A,
            };

            return Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv(hsvColor.H,
                                                                     hsvColor.S,
                                                                     hsvColor.V,
                                                                     hsvColor.A);
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// Only +/- 3 shades from the given color are supported.
    /// </summary>
    public class AccentColorShadeConverter : IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              string language)
        {
            int shade;
            HsvColor hsvColor;

            // Get the value component delta
            try
            {
                shade = System.Convert.ToInt32(parameter?.ToString());
            }
            catch
            {
                throw new ArgumentException("Invalid parameter provided, unable to convert to double");
            }

            // Get the current color in HSV
            try
            {
                hsvColor = ((Color)value).ToHsv();
            }
            catch
            {
                throw new ArgumentException("Invalid color value provided, unable to convert to HsvColor");
            }

            double colorHue        = hsvColor.H;
            double colorSaturation = hsvColor.S;
            double colorValue      = hsvColor.V;
            double colorAlpha      = hsvColor.A;

            // Use the HSV representation as it's more perceptual
            switch (shade)
            {
                case -3:
                    {
                        colorHue        = colorHue        * 1.0;
                        colorSaturation = colorSaturation * 1.10;
                        colorValue      = colorValue      * 0.40;
                        break;
                    }
                case -2:
                    {
                        colorHue        = colorHue        * 1.0;
                        colorSaturation = colorSaturation * 1.05;
                        colorValue      = colorValue      * 0.50;
                        break;
                    }
                case -1:
                    {
                        colorHue        = colorHue        * 1.0;
                        colorSaturation = colorSaturation * 1.0;
                        colorValue      = colorValue      * 0.75;
                        break;
                    }
                case 0:
                    {
                        // No change
                        break;
                    }
                case 1:
                    {
                        colorHue        = colorHue        * 1.00;
                        colorSaturation = colorSaturation * 1.00;
                        colorValue      = colorValue      * 1.05;
                        break;
                    }
                case 2:
                    {
                        colorHue        = colorHue        * 1.00;
                        colorSaturation = colorSaturation * 0.75;
                        colorValue      = colorValue      * 1.05;
                        break;
                    }
                case 3:
                    {
                        colorHue        = colorHue        * 1.00;
                        colorSaturation = colorSaturation * 0.65;
                        colorValue      = colorValue      * 1.05;
                        break;
                    }
            }

            return Microsoft.Toolkit.Uwp.Helpers.ColorHelper.FromHsv(Math.Clamp(colorHue,        0.0, 360.0),
                                                                     Math.Clamp(colorSaturation, 0.0, 1.0),
                                                                     Math.Clamp(colorValue,      0.0, 1.0),
                                                                     Math.Clamp(colorAlpha,      0.0, 1.0));
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a color to a hex string and vice versa.
    /// </summary>
    public class ColorToHexConverter : IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              string language)
        {
            Color color;

            // Get the changing color to compare against
            try
            {
                color = (Color)value;
            }
            catch
            {
                throw new ArgumentException("Invalid color value provided");
            }

            string hexColor = color.ToHex().Replace("#", "");
            return hexColor;
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  string language)
        {
            string hexValue = value.ToString();

            if (hexValue.StartsWith("#"))
            {
                try
                {
                    return hexValue.ToColor();
                }
                catch
                {
                    throw new ArgumentException("Invalid hex color value provided");
                }
            }
            else
            {
                try
                {
                    return ("#" + hexValue).ToColor();
                }
                catch
                {
                    throw new ArgumentException("Invalid hex color value provided");
                }
            }
        }
    }
}