using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosJerry
{
    class CosmosJerryWeapon:EquipmentScriptBase
    {
        int count = 1;
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            float average = (actor.attackSpeed + actor.movement + actor.maxHp) / 3f;
            float damageMulti = average / 16f;
            if (damageMulti > 1f)
            {
                dmg *= damageMulti;
            }

            float countMulti = 0.333f*count;

            dmg *= countMulti;

            AgentModel owner = model.owner as AgentModel;
            if (!owner.IsPanic())
            {
                if (target is AgentModel)
                {
                    AgentModel agent = target as AgentModel;
                    if (agent.GetState() != AgentAIState.CANNOT_CONTROLL)
                    {
                        agent.RecoverMental(dmg.max * 0.5f);
                        return false;
                    }
                }
            }
            count--;
            return base.OnGiveDamage(actor, target, ref dmg);
        }

        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            List<DamageInfo> list= new List<DamageInfo>();
            string animationName = base.model.metaInfo.animationNames[0];
            for (int i = 0; i < 2; i++)
            {
                list.Add(base.model.metaInfo.damageInfos[0].Copy());
            }
            count = 2;
            return new EquipmentScriptBase.WeaponDamageInfo(animationName, list.ToArray());
        }
    }
}
