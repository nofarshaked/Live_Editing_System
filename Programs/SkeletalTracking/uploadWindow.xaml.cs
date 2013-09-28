using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Google.GData.Extensions;
using Google.GData.YouTube;
using Google.GData.Extensions.MediaRss;
using Google.YouTube;
using Google.GData.Client;

namespace SkeletalTracking
{
    public partial class uploadWindow : Window
    {
        static string _header;
        static string _description;
        static string _keywords;

        public uploadWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UploadVideo();
        }

        private void RichTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var desc = sender as RichTextBox;
            TextRange allTextRange = new TextRange(desc.Document.ContentStart, desc.Document.ContentEnd);
            _description = allTextRange.Text;
        }

        private void RichTextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            var head = sender as RichTextBox;
            TextRange allTextRange = new TextRange(head.Document.ContentStart, head.Document.ContentEnd);
            _header = allTextRange.Text;
        }

        private void RichTextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            var tags = sender as RichTextBox;
            TextRange allTextRange = new TextRange(tags.Document.ContentStart, tags.Document.ContentEnd);
            _keywords = allTextRange.Text;
        }

        public static void UploadVideo()
        {
            try
            {
//                string developerKey = "AI39si72llWnlwlqjLA1XPfT1uz-ryd6n8Yb9KgjoHsjHda3tn5fANjJ1Ys73SFRUp0aV7oxdJESkICc1jveiBb4tD-zvKZnfw";
                String developerKey = "AIzaSyAWcrKx4meD6r3-SixVEMaMf_PCvUwA-rE";
                string username = "moossygoossy@gmail.com";
                string password = "m123456&";
                YouTubeRequestSettings settings = new YouTubeRequestSettings("API Project", developerKey, username, password);
                YouTubeRequest request = new YouTubeRequest(settings);

                Video newVideo = new Video();

                newVideo.Title = _header;
                newVideo.Tags.Add(new MediaCategory("Autos", YouTubeNameTable.CategorySchema));
                newVideo.Keywords = _keywords;
                newVideo.Description = _description;
                newVideo.YouTubeEntry.Private = false;
            
                newVideo.Tags.Add(new MediaCategory("mydevtag, anotherdevtag", YouTubeNameTable.DeveloperTagSchema));

                //newVideo.YouTubeEntry.Location = new GeoRssWhere(37, -122);
                // alternatively, you could just specify a descriptive string
                // newVideo.YouTubeEntry.setYouTubeExtension("location", "Mountain View, CA");

                newVideo.YouTubeEntry.MediaSource = new MediaFileSource(@"C:\Users\nufar\Desktop\Final Project DAMN IT\SkeletalTracking\bin\Debug\EntireScreenCaptured.wmv",
                  "video/x-ms-wmv");

                Video createdVideo = request.Upload(newVideo);
                Status pop = new Status(createdVideo);
                pop.Show();
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //go back to MainWindow;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //exit program or go back to ProjectWindow;
        }
    }
}
