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
    /// <summary>
    /// Interaction logic for Status.xaml
    /// </summary>
    public partial class Status : Window
    {
        public Status(Video vid)
        {
            InitializeComponent();
            statusBox.Text = "Processing...";

            while (true)
            {
                if (vid.IsDraft)
                {
                    Console.WriteLine("Video is not live.");
                    string stateName = vid.Status.Name;
                    if (stateName == "processing")
                    {
                        statusBox.Text = "Processing...";
                    }
                    else if (stateName == "rejected")
                    {
                        statusBox.Text = "Video has been rejected because: \n";
                        statusBox.Text += vid.Status.Value;
                        statusBox.Text += "\nFor help visit: \n";
                        statusBox.Text += vid.Status.Help;
                    }
                    else if (stateName == "failed")
                    {
                        statusBox.Text = "Video failed uploading because: \n";
                        statusBox.Text += vid.Status.Value;
                        statusBox.Text += "\nFor help visit: \n";
                        statusBox.Text += vid.Status.Help;
                    }
                    else if (stateName == "accepted")
                    {
                        statusBox.Text = "Success! Video is up online.";
                        continue;
                    }
                }
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
