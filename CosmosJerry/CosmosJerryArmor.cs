using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosJerry
{
    class CosmosJerryArmor:EquipmentScriptBase
    {
        //ここのUnitModel actorは攻撃者。装備者の情報が必要な場合はthis.model.ownerを使う。
        public override bool OnTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            //攻撃者が職員であればRed以外のダメージをWhiteダメージに変換する
            if(actor is WorkerModel)
            {
                if (dmg.type != RwbpType.R)
                {
                    dmg.type = RwbpType.W;
                }
            }
            return base.OnTakeDamage(actor, ref dmg);
        }
    }
}
