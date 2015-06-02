﻿using SDK.Common;
using SDK.Lib;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class SceneDZData
    {
        public SceneDZArea[] m_sceneDZAreaArr;
        public HistoryArea m_historyArea;
        public TimerItemBase m_timer;   // 回合开始的时候开始回合倒计时，进入对战，每一回合倒计时
        public bool m_bStartRound = false;                  // 起始牌都落下，才算开始回合

        public RoundBtn m_roundBtn;          // 翻转按钮，结束当前一局
        public LuckCoinCard m_luckCoin; // 对战场景中的幸运币
        public SelfTurnTip m_selfTurnTip;               // 自己回合提示
        public SelfCardFullTip m_selfCardFullTip;   // 自己卡牌满

        public AuxLabel[] m_textArr;
        public UIGrid[] m_mpGridArr;

        public GameObject m_centerGO;                   // 中心 GO ，所有场景中的牌都放在这个上面
        public GameObject m_startGO;                    // 开始按钮

        public SceneCardBase m_curDragItem;             // 当前正在拖放的 item

        public GameObject[,] m_cardCenterGOArr;    // 保存所有占位的位置信息

        public GameObject m_attackArrowGO;                      // 攻击箭头开始位置
        public GameObject m_arrowListGO;                        // 攻击箭头列表

        public AttackArrow m_attackArrow;
        public GameOpState m_gameOpState;
        public GameRunState m_gameRunState;             // 游戏运行状态

        protected int m_preWhiteIdx = -1;      // 之前白色卡牌位置
        protected int m_curWhiteIdx = -1;      // 当前卡牌位置

        protected bool m_bHeroAniEnd = false;   // hero 动画是否结束
        protected bool m_bAddselfCard = false;  // 是否有自己的初始卡牌

        public GameObject m_timerGo;            // 定时器节点
        public DJSTimer m_DJSTimer;             // 定时器
        public List<uint> m_changeCardList;     // 在初始阶段，选中的需要交换卡牌
        public DZDaoJiShiXmlLimit m_DZDaoJiShiXmlLimit;

        public SceneDZData()
        {
            // 加载xml配置文件
            m_DZDaoJiShiXmlLimit = Ctx.m_instance.m_mapCfg.m_mapXml.m_list[0] as DZDaoJiShiXmlLimit;
            startInitCardTimer();           // 启动定时器

            m_gameRunState = new GameRunState(this);
            m_sceneDZAreaArr = new SceneDZArea[(int)EnDZPlayer.ePlayerTotal];
            m_roundBtn = new RoundBtn();
            m_luckCoin = new LuckCoinCard();
            m_selfTurnTip = new SelfTurnTip();
            m_selfCardFullTip = new SelfCardFullTip();
            m_textArr = new AuxLabel[(int)EnSceneDZText.eTotal];
            m_mpGridArr = new UIGrid[(int)EnSceneDZText.eTotal];
            m_cardCenterGOArr = new GameObject[2, 6];
            m_changeCardList = new List<uint>();
        }

        public void findWidget()
        {
            m_roundBtn.setGameObject(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.TurnBtn));
            m_roundBtn.m_sceneDZData = this;
            m_luckCoin.setGameObject(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.LuckyCoin));
            m_selfTurnTip.setGameObject(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfTurnTip));
            m_selfCardFullTip.setGameObject(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfCardFullTip));
            m_selfCardFullTip.m_desc = new AuxLabel(m_selfCardFullTip.getGameObject(), CVSceneDZPath.SelfCardFullTipText);
            m_selfCardFullTip.getGameObject().SetActive(false);

            m_centerGO = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.CenterGO);
            m_startGO = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.StartGO);
            UtilApi.SetActive(m_startGO, false);      // 默认是隐藏的

            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerSelf, (int)CardArea.CARDCELLTYPE_NONE] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfStartCardCenterGO);
            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerEnemy, (int)CardArea.CARDCELLTYPE_NONE] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemyStartCardCenterGO);

            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerSelf, (int)CardArea.CARDCELLTYPE_COMMON] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfOutCardCenterGO);
            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerEnemy, (int)CardArea.CARDCELLTYPE_COMMON] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemyOutCardCenterGO);

            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerSelf, (int)CardArea.CARDCELLTYPE_HAND] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfCardCenterGO);
            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerEnemy, (int)CardArea.CARDCELLTYPE_HAND] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemyCardCenterGO);

            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerSelf, (int)CardArea.CARDCELLTYPE_EQUIP] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfEquipGO);
            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerEnemy, (int)CardArea.CARDCELLTYPE_EQUIP] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemyEquipGO);

            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerSelf, (int)CardArea.CARDCELLTYPE_SKILL] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfSkillGO);
            m_cardCenterGOArr[(int)EnDZPlayer.ePlayerEnemy, (int)CardArea.CARDCELLTYPE_SKILL] = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemySkillGO);

            m_attackArrowGO = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.ArrowStartPosGO);
            m_arrowListGO = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.ArrowListGO);
            m_timerGo = UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.TimerGo);

            m_textArr[(int)EnSceneDZText.eSelfMp] = new AuxLabel(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfMpText));
            m_textArr[(int)EnSceneDZText.eEnemyMp] = new AuxLabel(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemyMpText));

            m_mpGridArr[(int)EnDZPlayer.ePlayerSelf] = new UIGrid();
            m_mpGridArr[(int)EnDZPlayer.ePlayerSelf].setGameObject(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.SelfMpList));
            m_mpGridArr[(int)EnDZPlayer.ePlayerSelf].maxPerLine = 10;
            m_mpGridArr[(int)EnDZPlayer.ePlayerSelf].cellWidth = 0.284f;
            m_mpGridArr[(int)EnDZPlayer.ePlayerSelf].cellHeight = 0.284f;
            m_mpGridArr[(int)EnDZPlayer.ePlayerEnemy] = new UIGrid();
            m_mpGridArr[(int)EnDZPlayer.ePlayerEnemy].setGameObject(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.EnemyMpList));
            m_mpGridArr[(int)EnDZPlayer.ePlayerEnemy].maxPerLine = 10;
            m_mpGridArr[(int)EnDZPlayer.ePlayerEnemy].cellWidth = 0.284f;
            m_mpGridArr[(int)EnDZPlayer.ePlayerEnemy].cellHeight = 0.284f;
        }

        public void addEventHandle()
        {

        }

        public void init()
        {
            m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerSelf] = new SelfDZArea(this, EnDZPlayer.ePlayerSelf);
            m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerEnemy] = new EnemyDZArea(this, EnDZPlayer.ePlayerEnemy);

            m_historyArea = new HistoryArea(UtilApi.GoFindChildByPObjAndName(CVSceneDZPath.HistoryGo));
            m_historyArea.m_sceneDZData = this;
            m_attackArrow = new AttackArrow(this);
            m_gameOpState = new GameOpState(this);

            // 设置 hero 动画结束后的处理
            m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerSelf].centerHero.heroAniEndDisp = heroAniEndDisp;
        }

        public void dispose()
        {
            // 释放自己的资源
            // 移除定时器
            stopTimer();
            m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerSelf].dispose();
            m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerEnemy].dispose();
        }

        public bool bHeroAniEnd
        {
            get
            {
                return m_bHeroAniEnd;
            }
            set
            {
                m_bHeroAniEnd = value;
            }
        }

        public bool bAddselfCard
        {
            get
            {
                return m_bAddselfCard;
            }
            set
            {
                m_bAddselfCard = value;
            }
        }

        public int preWhiteIdx
        {
            get
            {
                return m_preWhiteIdx;
            }
            set
            {
                m_preWhiteIdx = value;
            }
        }

        public int curWhiteIdx
        {
            get
            {
                return m_curWhiteIdx;
            }
            set
            {
                m_preWhiteIdx = m_curWhiteIdx;
                m_curWhiteIdx = value;
            }
        }

        public SceneCardBase createOneCard(uint objid, EnDZPlayer m_playerFlag, CardArea area, CardType cardType)
        {
            SceneCardBase cardItem = Ctx.m_instance.m_sceneCardMgr.createCard(objid, cardType, this, m_centerGO);
            if (objid != SceneCardBase.BLACK_CARD_ID || CardType.CARDTYPE_HERO != cardType)   // 如果不是 hero 和 BlackCard
            {
                cardItem.dragControl.m_centerPos = m_cardCenterGOArr[(int)m_playerFlag, (int)area].transform.localPosition;
                // 设置初始位置为发牌位置
                cardItem.aniControl.startPos = m_cardCenterGOArr[(int)m_playerFlag, (int)CardArea.CARDCELLTYPE_NONE].transform.localPosition;
                cardItem.aniControl.destPos = m_cardCenterGOArr[(int)m_playerFlag, (int)area].transform.localPosition;

                // 设置是否可以动画
                if (m_playerFlag == EnDZPlayer.ePlayerEnemy)        // 如果是 enemy 的卡牌
                {
                    cardItem.dragControl.disableDrag();
                    if (area == CardArea.CARDCELLTYPE_SKILL || area == CardArea.CARDCELLTYPE_EQUIP)
                    {
                        cardItem.aniControl.destScale = SceneCardBase.SMALLFACT;
                    }
                }
                // 如果是放在技能或者装备的位置，是不允许拖放的
                else if (area == CardArea.CARDCELLTYPE_SKILL || area == CardArea.CARDCELLTYPE_EQUIP)
                {
                    cardItem.aniControl.destScale = SceneCardBase.SMALLFACT;
                    cardItem.dragControl.disableDrag();
                }

                // 更新边框
                if (EnDZPlayer.ePlayerSelf == m_playerFlag)
                {
                    if (CardArea.CARDCELLTYPE_HAND == area)
                    {
                        if (Ctx.m_instance.m_dataPlayer.m_dzData.bSelfSide())
                        {
                            cardItem.updateCardOutState(true);
                        }
                        else
                        {
                            cardItem.updateCardOutState(false);
                        }
                    }
                }
            }
            cardItem.init();        // 非主角初始化，主角需要等到设置 GameObject 后再初始化

            return cardItem;
        }

        public void createMovePath(SceneCardBase card, Transform startPos, Transform destPos)
        {
            card.aniControl.startPos = startPos.localPosition;
            card.aniControl.destPos = destPos.localPosition;

            card.aniControl.moveToDestRST();
        }

        public SceneCardBase getUnderSceneCard()
        {
            SceneCardBase cardBase;
            GameObject underGo = Ctx.m_instance.m_coordConv.getUnderGameObject();
            if(underGo != null)
            {
                cardBase = m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerSelf].getUnderSceneCard(underGo);
                if(cardBase != null)
                {
                    return cardBase;
                }

                cardBase = m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerEnemy].getUnderSceneCard(underGo);
                if (cardBase != null)
                {
                    return cardBase;
                }
            }

            return null;
        }

        public SceneCardBase getSceneCardByThisID(uint thisID, ref EnDZPlayer side, ref CardArea slot)
        {
            SceneCardBase cardBase;
            cardBase = m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerSelf].getSceneCardByThisID(thisID, ref slot);
            if(cardBase != null)
            {
                side = EnDZPlayer.ePlayerSelf;
                return cardBase;
            }

            cardBase = m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerEnemy].getSceneCardByThisID(thisID, ref slot);
            if (cardBase != null)
            {
                side = EnDZPlayer.ePlayerEnemy;
                return cardBase;
            }

            return null;
        }

        public void heroAniEndDisp()
        {
            bHeroAniEnd = true;
            UtilApi.SetActive(m_startGO, true);      // 主角动画完成，需要显示开始按钮
            if (bAddselfCard)
            {
                addSelfFirstCard();
            }
        }

        public void addSelfFirstCard()
        {
            m_sceneDZAreaArr[(int)EnDZPlayer.ePlayerSelf].inSceneCardList.addInitCard();
        }

        // 启动定时器
        public void startInitCardTimer()
        {
            Ctx.m_instance.m_logSys.log(Ctx.m_instance.m_langMgr.getText(LangTypeId.eDZ4, LangItemID.eItem4));

            if (m_timer == null)
            {
                m_timer = new TimerItemBase();
                m_timer.m_internal = m_DZDaoJiShiXmlLimit.m_preparetime - m_DZDaoJiShiXmlLimit.m_lastpreparetime;
                m_timer.m_totalCount = m_timer.m_internal;
                m_timer.m_timerDisp = onTimerInitCardHandle;

                Ctx.m_instance.m_timerMgr.addObject(m_timer);
            }
            else
            {
                m_timer.reset();
            }
        }

        // 开始对战定时器
        public void startDZTimer()
        {
            Ctx.m_instance.m_logSys.log(Ctx.m_instance.m_langMgr.getText(LangTypeId.eDZ4, LangItemID.eItem5));

            if (m_timer == null)
            {
                m_timer = new TimerItemBase();
                m_timer.m_internal = m_DZDaoJiShiXmlLimit.m_roundtime - m_DZDaoJiShiXmlLimit.m_lastroundtime;
                m_timer.m_totalCount = m_timer.m_internal;
                m_timer.m_timerDisp = onTimerDZHandle;

                Ctx.m_instance.m_timerMgr.addObject(m_timer);
            }
            else
            {
                m_timer.reset();    // 重置参数
            }
        }

        // 改变定时器参数为回合倒计时定时器参数
        public void changeTimer()
        {
            Ctx.m_instance.m_logSys.log(Ctx.m_instance.m_langMgr.getText(LangTypeId.eDZ4, LangItemID.eItem6));

            if (m_timer != null)
            {
                m_timer.reset();
                m_timer.m_internal = m_DZDaoJiShiXmlLimit.m_roundtime - m_DZDaoJiShiXmlLimit.m_lastroundtime;
                m_timer.m_totalCount = m_timer.m_internal;
                m_timer.m_timerDisp = onTimerDZHandle;

                Ctx.m_instance.m_timerMgr.addObject(m_timer);
            }
        }

        // 重置定时器
        protected void resetTimer()
        {
            m_timer.reset();
        }

        // 停止定时器
        public void stopTimer()
        {
            Ctx.m_instance.m_logSys.log(Ctx.m_instance.m_langMgr.getText(LangTypeId.eDZ4, LangItemID.eItem7));

            if (m_timer != null)
            {
                Ctx.m_instance.m_timerMgr.delObject(m_timer);
            }
        }

        // 开始卡牌倒计时
        public void onTimerInitCardHandle(TimerItemBase timer)
        {
            Ctx.m_instance.m_logSys.log(Ctx.m_instance.m_langMgr.getText(LangTypeId.eDZ4, LangItemID.eItem8));

            // 开始显示倒计时数据
            if (m_DJSTimer == null)
            {
                m_DJSTimer = new DJSTimer(m_timerGo);
            }

            m_DJSTimer.startTimer();
        }

        // 每一回合倒计时
        public void onTimerDZHandle(TimerItemBase timer)
        {
            Ctx.m_instance.m_logSys.log(Ctx.m_instance.m_langMgr.getText(LangTypeId.eDZ4, LangItemID.eItem9));

            // 开始显示倒计时数据
            if (m_DJSTimer == null)
            {
                m_DJSTimer = new DJSTimer(m_timerGo);
            }

            m_DJSTimer.startTimer();
        }
    }
}