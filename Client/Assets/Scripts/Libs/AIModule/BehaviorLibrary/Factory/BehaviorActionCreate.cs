﻿using System;
using System.Collections.Generic;
using System.Security;

namespace BehaviorLibrary
{
    /**
     * @brief 一类组件的创建
     */
    public class BehaviorActionCreate : ComponentCreate
    {
        public BehaviorActionCreate()
        {
            regHandle();
        }

        protected void regHandle()
        {
            // Actions 组件注册
            m_id2CreateDic["Wander"] = createBehaviorActionWander;
            m_id2BuildDic["Wander"] = buildBehaviorActionWander;

            m_id2CreateDic["Follow"] = createBehaviorActionFollow;
            m_id2BuildDic["Follow"] = buildBehaviorActionFollow;
        }

        public BehaviorComponent createBehaviorActionWander()
        {
            ActionWander behaviorActionWander = new ActionWander();
            return behaviorActionWander;
        }

        public void buildBehaviorActionWander(BehaviorComponent btCmt, SecurityElement btNode)
        {

        }

        public BehaviorComponent createBehaviorActionFollow()
        {
            ActionFollow behaviorActionFollow = new ActionFollow();
            return behaviorActionFollow;
        }

        public void buildBehaviorActionFollow(BehaviorComponent btCmt, SecurityElement btNode)
        {

        }
    }
}