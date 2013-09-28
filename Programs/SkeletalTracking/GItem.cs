using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 

namespace SkeletalTracking
{
    public class GItem
    {
        public GItem()
        { }

        public GItem(String path, string t, int scrnPosX, int scrnPosY, int menuPos, string voiceCmnd)
        {
            mPath = String.Copy(path);
            mType = t;
            screenPositionX = scrnPosX;
            screenPositionY = scrnPosY;
            menuPosition = menuPos;
            voiceCommand = String.Copy(voiceCmnd);
        }

        private string mPath;
        private string mType;
        private bool isActive = false;
        private double scale = 1.0;
        private int screenPositionX;
        private int screenPositionY;
        private int menuPosition;
        private String voiceCommand;

        /*Setters*/
        public void setPath(string p)
        { mPath = p; }

        public void setType(string t)
        { mType = t; }

        public void setScreenPositionX(string x)
        { screenPositionX = Int32.Parse(x); }

        public void setScreenPositionY(string y)
        { screenPositionY = Int32.Parse(y); }

        public void setMenuPosition(string m)
        { menuPosition =Int32.Parse(m); }

        public void setVoiceCommand(string v)
        { voiceCommand = v; }

        /*Getters*/
        public String getPath()
        { return mPath; }
        public int getMenuPosition()
        { return menuPosition; }
        public String getVoiceCommand()
        { return voiceCommand; }
        public int getScrnPosX()
        { return screenPositionX; }
        public int getScrnPosY()
        { return screenPositionY; }
    }
}
