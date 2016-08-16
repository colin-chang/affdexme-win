using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Shapes;
using System.IO;


namespace AffdexMe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Affdex.ImageListener, Affdex.ProcessStatusListener
    {

        public MainWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
        }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Detector = null;

            // Show the logo
            logoBackground.Visibility = Visibility.Visible;
            cornerLogo.Visibility = Visibility.Hidden;

            EnabledClassifiers = AffdexMe.Settings.Default.Classifiers;
            canvas.MetricNames = EnabledClassifiers;

            // Enable/Disable buttons on start
            btnStopCamera.IsEnabled =
            btnExit.IsEnabled = true;

            btnStartCamera.IsEnabled =
            btnResetCamera.IsEnabled =
            Points.IsEnabled =
            Metrics.IsEnabled =
            Appearance.IsEnabled =
            Emojis.IsEnabled =
            btnResetCamera.IsEnabled =
            btnAppShot.IsEnabled =
            btnStopCamera.IsEnabled = false;

            // Initialize Button Click Handlers
            btnStartCamera.Click += btnStartCamera_Click;
            btnStopCamera.Click += btnStopCamera_Click;
            Points.Click += Points_Click;
            Emojis.Click += Emojis_Click;
            Appearance.Click += Appearance_Click;
            Metrics.Click += Metrics_Click;
            btnResetCamera.Click += btnResetCamera_Click;
            btnExit.Click += btnExit_Click;
            btnAppShot.Click += btnAppShot_Click;

            ShowEmojis = canvas.DrawEmojis = AffdexMe.Settings.Default.ShowEmojis;
            ShowAppearance = canvas.DrawAppearance = AffdexMe.Settings.Default.ShowAppearance;
            ShowFacePoints = canvas.DrawPoints = AffdexMe.Settings.Default.ShowPoints;
            ShowMetrics = canvas.DrawMetrics = AffdexMe.Settings.Default.ShowMetrics;
            changeButtonStyle(Emojis, AffdexMe.Settings.Default.ShowEmojis);
            changeButtonStyle(Appearance, AffdexMe.Settings.Default.ShowAppearance);
            changeButtonStyle(Points, AffdexMe.Settings.Default.ShowPoints);
            changeButtonStyle(Metrics, AffdexMe.Settings.Default.ShowMetrics);

            this.ContentRendered += MainWindow_ContentRendered;
        }

        /// <summary>
        /// Once the window las been loaded and the content rendered, the camera
        /// can be initialized and started. This sequence allows for the underlying controls
        /// and watermark logo to be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            StartCameraProcessing();
        }

        /// <summary>
        /// Center the main window on the screen
        /// </summary>
        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }


        /// <summary>
        /// Handles the Image results event produced by Affdex.Detector
        /// </summary>
        /// <param name="faces">The detected faces.</param>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image analyzed.</param>
        public void onImageResults(Dictionary<int, Affdex.Face> faces, Affdex.Frame image)
        {
            DrawData(image, faces);
        }

        /// <summary>
        /// Handles the Image capture from source produced by Affdex.Detector
        /// </summary>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image captured from camera.</param>
        public void onImageCapture(Affdex.Frame image)
        {
            DrawCapturedImage(image);
        }

        /// <summary>
        /// Handles occurence of exception produced by Affdex.Detector
        /// </summary>
        /// <param name="ex">The <see cref="Affdex.AffdexException"/> instance containing the exception details.</param>
        public void onProcessingException(Affdex.AffdexException ex)
        {
            String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
            ShowExceptionAndShutDown(message);
        }


        /// <summary>
        /// Handles the end of processing event; not used
        /// </summary>
        public void onProcessingFinished(){}


        /// <summary>
        /// Displays a alert with exception details
        /// </summary>
        /// <param name="exceptionMessage"> contains the exception details.</param>
        private void ShowExceptionAndShutDown(String exceptionMessage)
        {
            MessageBoxResult result = MessageBox.Show(exceptionMessage,
                                                        "AffdexMe Error",
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Error);
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                StopCameraProcessing();
                ResetDisplayArea();
            }));
        }

        /// <summary>
        /// Constructs the bitmap image from byte array.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        private BitmapSource ConstructImage(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch(Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }

            return null;
        }

        /// <summary>
        /// Draws the facial analysis captured by Affdex.Detector.
        /// </summary>
        /// <param name="image">The image analyzed.</param>
        /// <param name="faces">The faces found in the image analyzed.</param>
        private void DrawData(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
        {
            try
            {
                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((Detector != null) && (Detector.isRunning()))
                        {
                            canvas.Faces = faces;
                            canvas.Width = cameraDisplay.ActualWidth;
                            canvas.Height = cameraDisplay.ActualHeight;
                            canvas.XScale = canvas.Width / image.getWidth();
                            canvas.YScale = canvas.Height / image.getHeight();
                            canvas.InvalidateVisual();                            
                            DrawSkipCount = 0;
                        }
                    }));
                }
            }
            catch(Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Draws the image captured from the camera.
        /// </summary>
        /// <param name="image">The image captured.</param>
        private void DrawCapturedImage(Affdex.Frame image)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    // Update the Image control from the UI thread
                    //cameraDisplay.Source = rtb;
                    cameraDisplay.Source = ConstructImage(image.getBGRByteArray(), image.getWidth(), image.getHeight());

                    // Allow N successive OnCapture callbacks before the FacePoint drawing canvas gets cleared.
                    if (++DrawSkipCount > 4)
                    {
                        canvas.Faces = new Dictionary<int, Affdex.Face>();
                        canvas.InvalidateVisual();
                        DrawSkipCount = 0;
                    }

                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                    ShowExceptionAndShutDown(message);
                }
            }));
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        void SaveSettings()
        {
            AffdexMe.Settings.Default.ShowPoints = ShowFacePoints;
            AffdexMe.Settings.Default.ShowAppearance = ShowAppearance;
            AffdexMe.Settings.Default.ShowEmojis = ShowEmojis;
            AffdexMe.Settings.Default.ShowMetrics = ShowMetrics;
            AffdexMe.Settings.Default.Classifiers = EnabledClassifiers;
            AffdexMe.Settings.Default.Save();
        }

        /// <summary>
        /// Resets the display area.
        /// </summary>
        private void ResetDisplayArea()
        {
            try
            {
                // Hide Camera feed;
                cameraDisplay.Visibility = cornerLogo.Visibility = Visibility.Hidden;

                // Clear any drawn data
                canvas.Faces = new Dictionary<int, Affdex.Face>();
                canvas.InvalidateVisual();

                // Show the logo
                logoBackground.Visibility = Visibility.Visible;

                Points.IsEnabled =
                Metrics.IsEnabled =
                Appearance.IsEnabled =
                Emojis.IsEnabled = false;

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Turns the on classifiers.
        /// </summary>
        private void TurnOnClassifiers()
        {
            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectAllEmojis(true);
            Detector.setDetectGender(true);
            Detector.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = Detector.GetType().GetMethod(String.Format("setDetect{0}", canvas.NameMappings(metric)));
                setMethodInfo.Invoke(Detector, new object[] { true });
            }
        }

        /// <summary>
        /// Starts the camera processing.
        /// </summary>
        private void StartCameraProcessing()
        {
            try
            {
                btnStartCamera.IsEnabled = false;
                btnResetCamera.IsEnabled =
                Points.IsEnabled =
                Metrics.IsEnabled =
                Appearance.IsEnabled =
                Emojis.IsEnabled =
                btnStopCamera.IsEnabled =
                btnAppShot.IsEnabled = 
                btnExit.IsEnabled = true;

                // Instantiate CameraDetector using default camera ID
                const int cameraId = 0;
                const int numberOfFaces = 10;
                const int cameraFPS = 15;
                const int processFPS = 15;
                Detector = new Affdex.CameraDetector(cameraId, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);

                //Set location of the classifier data files, needed by the SDK
                Detector.setClassifierPath(FilePath.GetClassifierDataFolder());

                // Set the Classifiers that we are interested in tracking
                TurnOnClassifiers();

                Detector.setImageListener(this);
                Detector.setProcessStatusListener(this);

                Detector.start();

                // Hide the logo, show the camera feed and the data canvas
                logoBackground.Visibility = Visibility.Hidden;
                cornerLogo.Visibility = Visibility.Visible;
                canvas.Visibility = Visibility.Visible;
                cameraDisplay.Visibility = Visibility.Visible;
            }
            catch (Affdex.AffdexException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    // If this is a camera failure, then reset the application to allow the user to turn on/enable camera
                    if (ex.Message.Equals("Unable to open webcam."))
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message,
                                                                "AffdexMe Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                        StopCameraProcessing();
                        return;
                    }
                }

                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Resets the camera processing.
        /// </summary>
        private void ResetCameraProcessing()
        {
            try
            {
                Detector.reset();
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Stops the camera processing.
        /// </summary>
        private void StopCameraProcessing()
        {
            try
            {
                if ((Detector != null) && (Detector.isRunning()))
                {
                    Detector.stop();
                    Detector.Dispose();
                    Detector = null;
                }

                // Enable/Disable buttons on start
                btnStartCamera.IsEnabled = true;
                btnResetCamera.IsEnabled =
                btnAppShot.IsEnabled = 
                btnStopCamera.IsEnabled = false;

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Changes the button style based on the specified flag.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="isOn">if set to <c>true</c> [is on].</param>
        private void changeButtonStyle(Button button, bool isOn)
        {
            Style style;
            String buttonText = String.Empty;

            if (isOn)
            {
                style = this.FindResource("PointsOnButtonStyle") as Style;
                buttonText = "Hide " + button.Name;
            }
            else
            {
                style = this.FindResource("CustomButtonStyle") as Style;
                buttonText = "Show " + button.Name;
            }
            button.Style = style;
            button.Content = buttonText;
        }

        /// <summary>
        /// Take a shot of the current canvas and save it to a png file on disk
        /// </summary>
        /// <param name="fileName">The file name of the png file to save it in</param>
        private void TakeScreenShot(String fileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            double dpi = 96d;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                                                                       dpi, dpi, PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(this);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            renderBitmap.Render(dv);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(fileName))
            {
                encoder.Save(file);
            }

            appShotLocLabel.Content = String.Format("Screenshot saved to: {0}", fileName);
            ((System.Windows.Media.Animation.Storyboard)FindResource("autoFade")).Begin(appShotLocLabel);
        }


        /// <summary>
        /// Handles the Click eents of the Take Screenshot control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAppShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                String fileName = String.Format("AffdexMe ScreenShot {0:MMMM dd yyyy h mm ss}.png", DateTime.Now);
                fileName = System.IO.Path.Combine(picturesFolder, fileName);
                this.TakeScreenShot(fileName);
            }
            catch (Exception ex)
            {
                String message = String.Format("AffdexMe error encountered while trying to take a screenshot, details={0}", ex.Message);
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Handles the Click event of the Metrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Metrics_Click(object sender, RoutedEventArgs e)
        {
            ShowMetrics = !ShowMetrics;
            canvas.DrawMetrics = ShowMetrics;
            changeButtonStyle((Button) sender, ShowMetrics);
        }

        /// <summary>
        /// Handles the Click event of the Points control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Points_Click(object sender, RoutedEventArgs e)
        {
            ShowFacePoints = !ShowFacePoints;
            canvas.DrawPoints = ShowFacePoints;
            changeButtonStyle((Button)sender, ShowFacePoints);
        }

        /// <summary>
        /// Handles the Click event of the Appearance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Appearance_Click(object sender, RoutedEventArgs e)
        {
            ShowAppearance = !ShowAppearance;
            canvas.DrawAppearance = ShowAppearance;
            changeButtonStyle((Button)sender, ShowAppearance);
        }

        /// <summary>
        /// Handles the Click event of the Emojis control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Emojis_Click(object sender, RoutedEventArgs e)
        {
            ShowEmojis = !ShowEmojis;
            canvas.DrawEmojis = ShowEmojis;
            changeButtonStyle((Button)sender, ShowEmojis);
        }

        /// <summary>
        /// Handles the Click event of the btnResetCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnResetCamera_Click(object sender, RoutedEventArgs e)
        {
            ResetCameraProcessing();
        }

        /// <summary>
        /// Handles the Click event of the btnStartCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartCameraProcessing();
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "AffdexMe error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStopCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopCamera_Click(object sender, RoutedEventArgs e)
        {
            StopCameraProcessing();
            ResetDisplayArea();
        }

        /// <summary>
        /// Handles the Click event of the btnExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCameraProcessing();
            SaveSettings();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the btnChooseWin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnChooseWin_Click(object sender, RoutedEventArgs e)
        {
            Boolean wasRunning = false;
            if ((Detector != null) && (Detector.isRunning()))
            {
                StopCameraProcessing();
                ResetDisplayArea();
                wasRunning = true;
            }
            
            MetricSelectionUI w = new MetricSelectionUI(EnabledClassifiers);
            w.ShowDialog();
            EnabledClassifiers = new StringCollection();
            foreach (String classifier in w.Classifiers)
            {
                EnabledClassifiers.Add(classifier);
            }
            canvas.MetricNames = EnabledClassifiers;
            if (wasRunning)
            {
                StartCameraProcessing();
            }
        }

        /// <summary>
        /// Once a face's feature points get displayed, the number of successive captures that occur without
        /// the points getting redrawn in the OnResults callback.
        /// </summary>
        private int DrawSkipCount { get; set; }

        /// <summary>
        /// Affdex Detector
        /// </summary>
        private Affdex.Detector Detector { get; set; }

        /// <summary>
        /// Collection of strings represent the name of the active selected metrics;
        /// </summary>
        private StringCollection EnabledClassifiers { get; set; }

        private bool ShowFacePoints { get; set; }
        private bool ShowAppearance { get; set; }
        private bool ShowEmojis { get; set; }
        private bool ShowMetrics { get; set; }
    }
}
