﻿using System.Collections;
using UnityEngine;

namespace SDK.Lib
{
    /**
     * @brief 数字动画，移动、旋转、缩放
     */
    public class RSTAni : ITweenAniBase
    {
        // 目标信息
        protected Vector3 m_destPos;       // 最终位置
        protected Vector3 m_destRot;       // 最终旋转
        protected Vector3 m_destScale;     // 最终缩放

        public RSTAni()
        {

        }

        public Vector3 destPos
        {
            get
            {
                return m_destPos;
            }
            set
            {
                m_destPos = value;
            }
        }

        public Vector3 destRot
        {
            set
            {
                m_destRot = value;
            }
        }

        public Vector3 destScale
        {
            set
            {
                m_destScale = value;
            }
        }

        public override void play()
        {
            base.play();
            buildAniParam();
        }

        protected void buildAniParam()
        {
            Hashtable args;
            args = new Hashtable();
            base.buildAniBasicParam(args);

            args["position"] = m_destPos;
            args["time"] = m_time;
            args["islocal"] = true;

            args["easetype"] = m_easeType;
            args["looptype"] = m_loopType;
            //args["method"] = "to";
            //args["type"] = "color";
            incItweenCount();
            iTween.MoveTo(m_go, args);

            args = new Hashtable();
            base.buildAniBasicParam(args);
            args["rotation"] = m_destRot;
            args["time"] = m_time;
            args["islocal"] = true;
            args["easetype"] = m_easeType;
            args["looptype"] = m_loopType;
            incItweenCount();
            iTween.RotateTo(m_go, args);

            args = new Hashtable();
            base.buildAniBasicParam(args);
            args["scale"] = m_destScale;
            args["time"] = m_time;
            args["easetype"] = m_easeType;
            args["looptype"] = m_loopType;
            incItweenCount();
            iTween.ScaleTo(m_go, args);
        }
    }
}