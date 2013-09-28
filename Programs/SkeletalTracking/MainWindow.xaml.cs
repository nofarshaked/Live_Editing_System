
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Coding4Fun.Kinect.Wpf.Controls;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.ComponentModel;

namespace SkeletalTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static double _topBoundary;
        private static double _bottomBoundary;
        private static double _leftBoundary;
        private static double _rightBoundary;
        private static double _itemLeft;
        private static double _itemTop;
        private List<GItem> _itemsList = new List<GItem>();
        private String[] itemMenuPaths = { "","","","" };
        private List<GItem> voiceCommands = new List<GItem>();

        Record recordManager = new Record();

        //zoom variables
        double zLeftX;
        double zRightX;
        bool zoomFlag = false;

        bool closing = false;
        const int skeletonCount = 6; 
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        bool dragFlag = false;               // An item is active and the user drags it // Follow a hand
        bool rightActivatedItem = false;     //TRUE:right hand activated item; FALSE:left hand activated item

        private KinectSensor sensor;
        private SpeechRecognitionEngine speechEngine;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);
                var commands = new Choices();
                commands.Add(new SemanticResultValue("start", "START"));
                commands.Add(new SemanticResultValue("action", "START"));
                commands.Add(new SemanticResultValue("record", "START"));
                commands.Add(new SemanticResultValue("drag", "DRAG"));
                commands.Add(new SemanticResultValue("catch", "DRAG"));
                commands.Add(new SemanticResultValue("let go", "STOP"));
                commands.Add(new SemanticResultValue("stop", "STOP"));
                commands.Add(new SemanticResultValue("one", "ONE"));

                var gb = new GrammarBuilder { Culture = ri.Culture };
                gb.Append(commands);

                var g = new Grammar(gb);
                speechEngine.LoadGrammar(g);

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;

                //speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);
                try
                {
                    speechEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                    speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                }
                catch (Exception)
                { }
            }


            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
            kinectRecordButton.Click += new RoutedEventHandler(kinectRecordButton_Clicked);
            kinectButton1.Click += new RoutedEventHandler(kinectButton1_Clicked);
            kinectButton2.Click += new RoutedEventHandler(kinectButton2_Clicked);
            kinectButton3.Click += new RoutedEventHandler(kinectButton3_Clicked);
            kinectButton4.Click += new RoutedEventHandler(kinectButton4_Clicked);
            touchUp.Click += new RoutedEventHandler(touchUp_Clicked);
            
            /*
             * load items from text file
             * and add to list of project items
             * which will be used in recording session
             */
            try
            {
                //need to handle path of file. right now is a file I created//
                using (StreamReader sr = new StreamReader("C:\\Users\\nufar\\Desktop\\Final Project DAMN IT\\folderForSaveFiles\\file.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string str;
                        string[] strArray;
                        str = sr.ReadLine();

                        strArray = str.Split(',');
                        GItem gi = new GItem();
                        gi.setPath(strArray[0]);
                        gi.setType(strArray[1]);
                        gi.setScreenPositionX(strArray[2]);
                        gi.setScreenPositionY(strArray[3]);
                        gi.setMenuPosition(strArray[4]);
                        /*new item in item menu*/
                        if (gi.getMenuPosition() > -1 && gi.getMenuPosition() < 4)
                        {
                            itemMenuPaths[gi.getMenuPosition()] = String.Copy(gi.getPath());
                        }
                        gi.setVoiceCommand(strArray[5]);
                        /*new voice command*/
                        if (gi.getVoiceCommand() != "")
                        {
                            voiceCommands.Add(gi);
                        }

                        _itemsList.Add(gi);

                        //change controlled image:       dynamicImage.Source = new BitmapImage(new Uri(@gi.getPath()));
                        //change button image source:   kinectButton1.ImageSource = gi.getPath();
                    }

                    // Done reading, close the reader
                    sr.Close();

                    /*set item menu icons*/
                    if (!itemMenuPaths[0].Equals("BlackButton-Active.png"))    //button1
                    {
                        kinectButton1.ImageSource = itemMenuPaths[0];
                        kinectButton1.ActiveImageSource = itemMenuPaths[0];
                    }
                    else
                        kinectButton1.ImageSource = "";

                    if (!itemMenuPaths[1].Equals("BlackButton-Active.png"))    //button2
                    {
                        kinectButton2.ImageSource = itemMenuPaths[1];
                        kinectButton2.ActiveImageSource = itemMenuPaths[1];
                    }
                    else
                        kinectButton1.ImageSource = "";

                    if (!itemMenuPaths[2].Equals("BlackButton-Active.png"))    //button3
                    {
                        kinectButton3.ImageSource = itemMenuPaths[2];
                        kinectButton3.ActiveImageSource = itemMenuPaths[2];
                    }
                    else
                        kinectButton1.ImageSource = "";

                    if (!itemMenuPaths[3].Equals("BlackButton-Active.png"))    //button4
                    {
                        kinectButton4.ImageSource = itemMenuPaths[3];
                        kinectButton4.ActiveImageSource = itemMenuPaths[3];
                    }
                    else
                        kinectButton1.ImageSource = "";
                }
            }

            // File error
            catch (Exception ee)
            {
                Console.WriteLine("{0}\n", ee.Message);
            }
        }

        /*Kinect changed*/
        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;

            StopKinect(old);

            KinectSensor sensor = (KinectSensor)e.NewValue;

            if (sensor == null)
            {
                return;
            }

            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();
            sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated; // Use Seated Mode

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); 
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            
            try
            {
                sensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }

        /*Kinect senses a single skeleton*/
        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton first =  GetFirstSkeleton(e);

            if (first == null)
            {
                return; 
            }
            //set scaled position
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);

            GetCameraPoint(first, e); 

        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 

            //convert & scale (.3 = means 1/3 of joint distance)
            Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);

        }
        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }
                

                //Map a joint location to a point on the depth map
                //left hand
                DepthImagePoint leftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //right hand
                DepthImagePoint rightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);


                //Map a depth point to a point on the color image
                //left hand
                ColorImagePoint leftColorPoint =
                    depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y,
                    ColorImageFormat.RawBayerResolution1280x960Fps12);
                //right hand
                ColorImagePoint rightColorPoint =
                    depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y,
                    ColorImageFormat.RawBayerResolution1280x960Fps12);


                //Set location
                CameraPosition(leftEllipse, leftColorPoint);
                CameraPosition(rightEllipse, rightColorPoint);

                CheckButton(kinectRecordButton);
                CheckButton(kinectButton1);
                CheckButton(kinectButton2);
                CheckButton(kinectButton3);
                CheckButton(kinectButton4);
                CheckButton(touchUp);

                /*draging dynamic item*/
                if (dragFlag)
                {
                    dynamicItemPosition(dynamicImage);
                }
                if (Canvas.GetTop(rightEllipse)< 100 && Canvas.GetTop(leftEllipse)< 100)//(zoomFlag)
                {
                    double zoom = 0;
                    var st = (ScaleTransform)dynamicImage.RenderTransform;
                    if((zRightX - zLeftX) / (Canvas.GetLeft(rightEllipse) - Canvas.GetLeft(leftEllipse)) != 1)
                        zoom = (zRightX - zLeftX) / (Canvas.GetLeft(rightEllipse) - Canvas.GetLeft(leftEllipse)) > 1 ? -.03 : .03;
                    st.ScaleX += zoom;
                    st.ScaleY += zoom;

                    zLeftX = Canvas.GetLeft(leftEllipse);
                    zRightX = Canvas.GetLeft(rightEllipse);

                }

            }        
        }

        /*drag item with hand*/
        private void dynamicItemPosition(FrameworkElement element)
        {
            //Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

           //Canvas.SetLeft(element, scaledJoint.Position.X);
           // Canvas.SetTop(element, scaledJoint.Position.Y);
            if(rightActivatedItem)
            {
                Canvas.SetLeft(element, Canvas.GetLeft(rightEllipse) - element.ActualWidth/2);
                Canvas.SetTop(element, Canvas.GetTop(rightEllipse) - element.ActualHeight/2);
            }
            else
            {
                Canvas.SetLeft(element, Canvas.GetLeft(leftEllipse) - element.ActualWidth/2);
                Canvas.SetTop(element, Canvas.GetTop(leftEllipse) - element.ActualHeight/2);
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

        }
        
        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null; 
                }

                
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                         where s.TrackingState == SkeletonTrackingState.Tracked
                                         select s).FirstOrDefault();

                return first;

            }
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true; 
            StopKinect(kinectSensorChooser1.Kinect);
        }

        #region importedfrommb

        private void CheckButton(HoverButton button) //at least one hand touches
        {
            if (button.ImageSource != "") //can't select items that aren't there
            {
                if (IsItemMidpointInContainer(button, rightEllipse) || IsItemMidpointInContainer(button, leftEllipse))
                {
                    button.Hovering();
                }
                else
                {
                    button.Release();
                }
            }
        }

        public bool IsItemMidpointInContainer(FrameworkElement container, FrameworkElement target)
        {
            FindValues(container, target);

            if (_itemTop < _topBoundary || _bottomBoundary < _itemTop)
            {
                //Midpoint of target is outside of top or bottom
                return false;
            }

            if (_itemLeft < _leftBoundary || _rightBoundary < _itemLeft)
            {
                //Midpoint of target is outside of left or right
                return false;
            }

            return true;
        }

        private void FindValues(FrameworkElement container, FrameworkElement target)
        {

            if (closing)
            {
                return;
            }

            var containerTopLeft = container.PointToScreen(new Point());
            var itemTopLeft = target.PointToScreen(new Point());

            _topBoundary = containerTopLeft.Y;
            _bottomBoundary = _topBoundary + container.ActualHeight;
            _leftBoundary = containerTopLeft.X;
            _rightBoundary = _leftBoundary + container.ActualWidth;

            //use midpoint of item (width or height divided by 2)
            _itemLeft = itemTopLeft.X + (target.ActualWidth / 2);
            _itemTop = itemTopLeft.Y + (target.ActualHeight / 2);
        }
/**************************************************************************************************************/

        /*Buttons Clicked*/
        void kinectRecordButton_Clicked(object sender, RoutedEventArgs e)
        {
            //change recorder state
            recordManager.setRecordFlag();
        }
        void kinectButton1_Clicked(object sender, RoutedEventArgs e)
        {
            /*image/video loaded to the controller in the position of selecting hand*/
            /*change controlled image:*/
            foreach (GItem gi in _itemsList)
            {
                if (IsItemMidpointInContainer(kinectButton1, rightEllipse))
                {
                    rightActivatedItem = true;
                }
                else
                {
                    rightActivatedItem = false;
                }

                if (gi.getPath().Equals(itemMenuPaths[0]))
                {
                    /*check if image or video*/

                    dynamicImage.Source = new BitmapImage(new Uri(@gi.getPath()));
                    dynamicImage.Width = 230;
                    dynamicImage.Height = 310;
                    //Canvas.SetLeft(dynamicImage, gi.getScrnPosX());
                    //Canvas.SetTop(dynamicImage, gi.getScrnPosY());
                    Canvas.SetLeft(dynamicImage, -500);
                    Canvas.SetTop(dynamicImage, -500);

                    dynamicVideo.Source = new Uri(@gi.getPath());
                    Canvas.SetLeft(dynamicVideo, -500);
                    Canvas.SetTop(dynamicVideo, -500);
                }
            }
            dragFlag = true;            
        }

        void kinectButton2_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsItemMidpointInContainer(kinectButton2, rightEllipse))
            {
                rightActivatedItem = true;
            }
            else
            {
                rightActivatedItem = false;
            }

            foreach (GItem gi in _itemsList)
            {
                if (gi.getPath().Equals(itemMenuPaths[1]))
                {
                    /*check if image or video*/

                    dynamicImage.Source = new BitmapImage(new Uri(@gi.getPath()));
                    dynamicImage.Width = 350;
                    dynamicImage.Height = 200;
                    //Canvas.SetLeft(dynamicImage, gi.getScrnPosX());
                    //Canvas.SetTop(dynamicImage, gi.getScrnPosY());
                    Canvas.SetLeft(dynamicImage,-500);
                    Canvas.SetTop(dynamicImage, -500);

                    dynamicVideo.Source = new Uri(@gi.getPath());
                    Canvas.SetLeft(dynamicVideo, -500);
                    Canvas.SetTop(dynamicVideo, -500);
                }
            }
            dragFlag = true;
        }

        void kinectButton3_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsItemMidpointInContainer(kinectButton3, rightEllipse))
            {
                rightActivatedItem = true;
            }
            else
            {
                rightActivatedItem = false;
            }

            foreach (GItem gi in _itemsList)
            {
                if (gi.getPath().Equals(itemMenuPaths[2]))
                {
                    /*check if image or video*/

                    dynamicImage.Source = new BitmapImage(new Uri(@gi.getPath()));
                    dynamicImage.Width = 200;
                    dynamicImage.Height = 160;
                    //Canvas.SetLeft(dynamicImage, gi.getScrnPosX());
                    //Canvas.SetTop(dynamicImage, gi.getScrnPosY());
                    Canvas.SetLeft(dynamicImage, -500);
                    Canvas.SetTop(dynamicImage, -500);

                    dynamicVideo.Source = new Uri(@gi.getPath());
                    Canvas.SetLeft(dynamicVideo, -500);
                    Canvas.SetTop(dynamicVideo, -500);
                }
            }
            dragFlag = true;
        }

        void kinectButton4_Clicked(object sender, RoutedEventArgs e)
        {
            if (IsItemMidpointInContainer(kinectButton4, rightEllipse))
            {
                rightActivatedItem = true;
            }
            else
            {
                rightActivatedItem = false;
            }

            foreach (GItem gi in _itemsList)
            {
                if (gi.getPath().Equals(itemMenuPaths[3]))
                {
                    /*check if image or video*/

                    dynamicImage.Source = new BitmapImage(new Uri(@gi.getPath()));
                    dynamicImage.Width = 380;
                    dynamicImage.Height = 215;
                    //Canvas.SetLeft(dynamicImage, gi.getScrnPosX());
                    //Canvas.SetTop(dynamicImage, gi.getScrnPosY());
                    Canvas.SetLeft(dynamicImage, -500);
                    Canvas.SetTop(dynamicImage, -500);

                    dynamicVideo.Source = new Uri(@gi.getPath());
                    Canvas.SetLeft(dynamicVideo, -500);
                    Canvas.SetTop(dynamicVideo, -500);
                }
            }
            dragFlag = true;
        }

        void touchUp_Clicked(object sender, RoutedEventArgs e)
        {
            /* Both hands touch-up: ZOOM-ITEM-IN/OUT */
           /* if (IsItemMidpointInContainer(touchUp, rightEllipse) && IsItemMidpointInContainer(touchUp, leftEllipse))
            {
                //first hands' X-positionings
                zLeftX = Canvas.GetLeft(leftEllipse);
                zRightX = Canvas.GetLeft(rightEllipse);

                zoomFlag = true;
            }*/
            /* One hand touch-up */
            /*else*/ if (/*!zoomFlag
                 && */((IsItemMidpointInContainer(touchUp, rightEllipse) && Canvas.GetTop(leftEllipse) > 99)
                 || (Canvas.GetTop(rightEllipse) > 99 && IsItemMidpointInContainer(touchUp, leftEllipse))))
            {
                /* One hand drags item, the other touch-up: LET-GO-OF-ITEM */
                if (dragFlag)
                {
                    dragFlag = false;
                }
                /* Item still on screen, one hand touch-up: CLOSE-ITEM */
                else
                {
                    dragFlag = false;

                    Canvas.SetLeft(dynamicImage, -500);
                    Canvas.SetTop(dynamicImage, -500);

                   // dynamicVideo.Source = new Uri("");
                    Canvas.SetLeft(dynamicVideo, -500);
                    Canvas.SetTop(dynamicVideo, -500);      
                }
            }
            if (!IsItemMidpointInContainer(touchUp, rightEllipse) || !IsItemMidpointInContainer(touchUp, leftEllipse))
            {
                zoomFlag = false;
            }
        }

        #endregion

        #region speechRec

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;
           
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "START":
                        recordManager.setRecordFlag();
                        kinectRecordButton.IsChecked = !kinectRecordButton.IsChecked;
                        break;

                    case "DRAG":
                        dragFlag = true;
                        break;

                    case "STOP":
                        dragFlag = false;
                        break;

                    case "ONE": 
                        foreach (GItem gi in _itemsList)
                        {
                            if (gi.getVoiceCommand().Equals("one"))
                            {
                                /*check if image or video*/

                                dynamicImage.Source = new BitmapImage(new Uri(@gi.getPath()));
                                //Canvas.SetLeft(dynamicImage, gi.getScrnPosX());
                                //Canvas.SetTop(dynamicImage, gi.getScrnPosY());
                                Canvas.SetLeft(dynamicImage, -500);
                                Canvas.SetTop(dynamicImage, -500);

                                dynamicVideo.Source = new Uri(@gi.getPath());
                                Canvas.SetLeft(dynamicVideo, -500);
                                Canvas.SetTop(dynamicVideo, -500);
                            }
                        }
                        dragFlag = true;
                        break;
                }
            }
        }

        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
           
        }

        #endregion
    }

}
