﻿namespace SDK.Lib
{
    public class MazeEndPt : MazePtBase
    {
        public MazeEndPt()
            : base(eMazePtType.eEnd)
        {

        }

        override public void moveToDestPos(MazePlayer mazePlayer_)
        {
            base.moveToDestPos(mazePlayer_);
            mazePlayer_.mazePlayerTrackAniControl.moveToDestPos(this);
        }
    }
}