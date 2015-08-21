﻿using SDK.Common;
using System.IO;
using UnityEngine;

namespace SDK.Lib
{
    public class MazePlayerTrackAniControl
    {
        protected const float sTime = 1;

        protected MazePlayer m_mazePlayer;

        protected NumAniParallel m_numAniParal;
        protected MList<MazePtBase> m_ptList;
        protected bool m_bDiePt;
        protected bool m_bBombPt;

        public MazePlayerTrackAniControl(MazePlayer mazePlayer_)
        {
            m_mazePlayer = mazePlayer_;
            m_numAniParal = new NumAniParallel();
            m_numAniParal.setAniSeqEndDisp(onMoveEndHandle);
            m_ptList = new MList<MazePtBase>();
            m_bDiePt = false;
            m_bBombPt = false;
        }

        public MList<MazePtBase> ptList
        {
            get
            {
                return m_ptList;
            }
            set
            {
                m_ptList = value;
            }
        }

        public void setStartPos()
        {
            m_mazePlayer.selfGo.transform.localPosition = m_ptList[0].pos;
            // 删除第一个点
            m_ptList.RemoveAt(0);
        }

        public void startMove()
        {
            m_ptList[0].moveToDestPos(m_mazePlayer);
            // 删除第一个点
            m_ptList.RemoveAt(0);
        }

        // 移动到下一个开始点，需要跳跃
        public void moveToDestPos(MazeStartPt pt_)
        {
            string path = Path.Combine(Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathAudio], "Jump.mp3");
            Ctx.m_instance.m_soundMgr.play(path, false);

            Vector3 srcPos = m_mazePlayer.selfGo.transform.localPosition;
            Vector3 destPos = pt_.pos;
            //float time = 1;

            Vector3 midPt;      // 中间点
            midPt = (srcPos + destPos) / 2;
            midPt.y = 5;

            SimpleCurveAni curveAni = new SimpleCurveAni();
            m_numAniParal.addOneNumAni(curveAni);
            curveAni.setGO(m_mazePlayer.selfGo);
            curveAni.setTime(sTime);
            curveAni.setPlotCount(3);
            curveAni.addPlotPt(0, srcPos);
            curveAni.addPlotPt(1, midPt);
            curveAni.addPlotPt(2, destPos);
            curveAni.setAniEndDisp(onMove2DestEnd);

            curveAni.setEaseType(iTween.EaseType.easeInExpo);

            m_numAniParal.play();
        }

        // 简单直接移动移动动画
        public void moveToDestPos(MazeComPt pt_)
        {
            PosAni posAni;
            posAni = new PosAni();
            m_numAniParal.addOneNumAni(posAni);
            posAni.setGO(m_mazePlayer.selfGo);
            posAni.destPos = pt_.pos;
            posAni.setTime(sTime);
            posAni.setEaseType(iTween.EaseType.linear);

            m_numAniParal.play();
        }

        // 移动到结束点
        public void moveToDestPos(MazeEndPt pt_)
        {
            PosAni posAni;
            posAni = new PosAni();
            m_numAniParal.addOneNumAni(posAni);
            posAni.setGO(m_mazePlayer.selfGo);
            posAni.destPos = pt_.pos;
            posAni.setTime(sTime);
            posAni.setEaseType(iTween.EaseType.linear);

            m_numAniParal.play();
        }

        // 爆炸点
        public void moveToDestPos(MazeBombPt pt_)
        {
            // 爆炸点
            m_bBombPt = true;

            PosAni posAni;
            posAni = new PosAni();
            m_numAniParal.addOneNumAni(posAni);
            posAni.setGO(m_mazePlayer.selfGo);
            posAni.destPos = pt_.pos;
            posAni.setTime(sTime);
            posAni.setEaseType(iTween.EaseType.linear);

            m_numAniParal.play();
        }

        // 死亡点
        public void moveToDestPos(MazeDiePt pt_)
        {
            // 死亡点
            m_bDiePt = true;
            m_mazePlayer.sceneEffect.stop();
            m_mazePlayer.sceneEffect.setTableID(32);
            m_mazePlayer.sceneEffect.play();

            PosAni posAni;
            posAni = new PosAni();
            m_numAniParal.addOneNumAni(posAni);
            posAni.setGO(m_mazePlayer.selfGo);
            posAni.destPos = pt_.pos;
            posAni.setTime(3);
            posAni.setEaseType(iTween.EaseType.easeInOutBounce);

            m_numAniParal.play();

            string path = Path.Combine(Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathAudio], "BossDie.mp3");
            Ctx.m_instance.m_soundMgr.play(path, false);
        }

        // 移动结束回调
        protected void onMoveEndHandle(NumAniSeqBase dispObj)
        {
            if (m_ptList.Count() > 0 && !m_bDiePt)  // 说明还有 WayPoint 可以走
            {
                m_ptList[0].moveToDestPos(m_mazePlayer);
                m_ptList.RemoveAt(0);
            }
            else    // 如果运行到终点位置
            {
                m_ptList.Clear();
                bool bChangeScene = false;
                if(!m_bBombPt && !m_bDiePt)     // 如果胜利
                {
                    Ctx.m_instance.m_maze.mazeData.mazeScene.show();
                    bChangeScene = true;
                }
                m_bBombPt = false;
                m_bDiePt = false;
                Ctx.m_instance.m_uiMgr.loadAndShow(UIFormID.eUIMaze);

                string path = "";
                path = Path.Combine(Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathAudio], "Ground.mp3");
                Ctx.m_instance.m_soundMgr.stop(path);
                path = Path.Combine(Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathAudio], "GameOver.mp3");
                Ctx.m_instance.m_soundMgr.play(path, false);

                if(bChangeScene)
                {
                    Ctx.m_instance.m_maze.mazeData.mazeScene.loadSecondScene();
                }
            }
        }

        public void onMove2DestEnd(NumAniBase ani)
        {
            onMoveEndHandle(null);
        }

        // 开始
        public void moveToDestPos(MazeStartJumpPt pt_)
        {
            string path = Path.Combine(Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathAudio], "Jump.mp3");
            Ctx.m_instance.m_soundMgr.play(path, false);

            Vector3 srcPos = m_mazePlayer.selfGo.transform.localPosition;
            Vector3 destPos = pt_.pos;
            //float time = 1;

            Vector3 midPt;      // 中间点
            midPt = (srcPos + destPos) / 2;
            midPt.y = 5;

            SimpleCurveAni curveAni = new SimpleCurveAni();
            m_numAniParal.addOneNumAni(curveAni);
            curveAni.setGO(m_mazePlayer.selfGo);
            curveAni.setTime(sTime);
            curveAni.setPlotCount(3);
            curveAni.addPlotPt(0, srcPos);
            curveAni.addPlotPt(1, midPt);
            curveAni.addPlotPt(2, destPos);
            curveAni.setAniEndDisp(onMove2DestEnd);

            curveAni.setEaseType(iTween.EaseType.easeInExpo);

            m_numAniParal.play();
        }

        public void moveToDestPos(MazeStartShowPt pt_)
        {
            m_mazePlayer.selfGo.transform.localPosition = pt_.pos;
        }

        public void moveToDestPos(MazeStartDoorPt pt_)
        {
            m_mazePlayer.selfGo.transform.localPosition = pt_.pos;
        }

        public void moveToDestPos(MazeEndDiePt pt_)
        {
            m_bDiePt = true;
            m_mazePlayer.sceneEffect.stop();
            m_mazePlayer.sceneEffect.setTableID(32);
            m_mazePlayer.sceneEffect.play();

            string path = Path.Combine(Ctx.m_instance.m_cfg.m_pathLst[(int)ResPathType.ePathAudio], "BossDie.mp3");
            Ctx.m_instance.m_soundMgr.play(path, false);
        }
    }
}