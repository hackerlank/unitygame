--[[
    @brief 技能基本表
    // 添加一个表的步骤一
]]

MLoader("MyLua.Libs.Core.GlobalNS");
MLoader("MyLua.Libs.Core.Class");

local M = GlobalNS.Class(GlobalNS.TableItemBodyBase);
M.clsName = "TableSkillItemBody";
GlobalNS[M.clsName] = M;

function M:ctor()
    self.m_name = "";               -- 名称
    self.m_effect = "";             -- 效果
    self.m_skillAttackEffect = 0;       -- 技能攻击特效
    self.m_effectMoveTime = 0;      -- 移动
    self.m_bNeedMove = 0;             -- 是否弹道特效, 0 不需要 1 需要
end

function M:parseBodyByteBuffer(bytes, offset)
    bytes:setPos(offset);
    self.m_name = GlobalNS.UtilTable.readString(bytes, self.m_name);
    self.m_effect = GlobalNS.UtilTable.readString(bytes, self.m_effect);
    _, self.m_skillAttackEffect = bytes:readUnsignedInt32(self.m_skillAttackEffect);
    _, self.m_bNeedMove = bytes:readInt32(self.m_bNeedMove);

    self:initDefaultValue();
end

function M:initDefaultValue()
    if (self.m_skillAttackEffect == 0) then
        self.m_skillAttackEffect = 8;
    end

    self.m_effectMoveTime = 1;
end

return M;