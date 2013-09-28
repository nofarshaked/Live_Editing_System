using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

using BytescoutScreenCapturingLib; // import bytescout screen capturing activex object 

class Record
{
    private Capturer capturer = new Capturer(); // create new screen capturer object
    private int recordFlag = 0;

    public Record()
    {
        capturer.CapturingType = CaptureAreaType.catScreen; // set capturing area type to catScreen to capture whole screen
        capturer.CaptureMouseCursor = false;
        capturer.AnimateMouseClicks = false;
        capturer.AudioEnabled = true;

     /*   capturer.CaptureAreaBorderColor = 5;
        capturer.CaptureAreaBorderType = CaptureAreaBorderType.cabtSolid;

        capturer.CropEnabled = true;
        capturer.CropRectHeight = 548;
        capturer.CropRectWidth = 1280;
        capturer.CropRectTop = 71;
        capturer.CropRectLeft = 1;
        */
        capturer.OutputFileName = "EntireScreenCaptured.wmv"; // set output video filename to .WMV or .AVI file

        // set output video width and height
        capturer.OutputWidth = 640;
        capturer.OutputHeight = 480;

        // uncomment to set Bytescout Lossless Video format output video compression method
        // do not forget to set file to .avi format if you use Video Codec Name
        capturer.CurrentVideoCodecName = "Bytescout Lossless";

        
        //Process.Start("EntireScreenCaptured.wmv");
    }
    public void startRecord()
    {
        capturer.Run(); // run screen video capturing 
    }
    public void stopRecord()
    {
        capturer.Stop(); // run screen video capturing 
    }
    public void setRecordFlag()
    {
        recordFlag++;
        if (recordFlag == 1)//start recording
            startRecord();
        if (recordFlag == 2)//end recording
        {
            stopRecord();
            //go to uploadWindow;
            var upWindow = new SkeletalTracking.uploadWindow();
            upWindow.Show();
        }
    }
}
