using System;
using System.Collections.Generic;
using UnityEngine;

namespace CosmosJerry
{
    public class CosmosJerry : CreatureBase
    {
		CosmosJerryAnim _anim;
        Timer _attackTimer = new Timer();
        bool _attacking = false;
		Timer _healTimer = new Timer();
		bool _healing = false;
		private static DamageInfo damageInfo = new DamageInfo(RwbpType.P, 1, 2);
		private global::MapNode _currentDestNode;

		public override void OnViewInit(CreatureUnit unit)
        {
			try
			{
				base.OnViewInit(unit);
				_anim = (CosmosJerryAnim)unit.animTarget;
				_anim.SetScript(this);
				ParamInit();
			}catch(Exception ex)
            {
				//staticメソッドなら他のクラスから呼び出してきても問題ない
				Harmony_Patch.WriteLog(ex.Message);
            }
        }
        public override void ActivateQliphothCounter()
        {
            base.ActivateQliphothCounter();
            this.Escape();
        }
        public override void OnStageStart()
        {
            base.OnStageStart();
			//ParamInit();
        }

        public override void OnWorkCoolTimeEnd(CreatureFeelingState oldState)
        {
			int probability = 0;
            if (oldState == CreatureFeelingState.BAD)
            {
				probability = 100;
            }else if (oldState == CreatureFeelingState.NORM)
            {
				probability = 15;
            }
            if (Prob(probability))
            {
				model.SubQliphothCounter();
            }

        }

        public override void OnEnterRoom(UseSkill skill)
        {
            base.OnEnterRoom(skill);
            if (skill.targetCreature.GetRiskLevel() - skill.agent.level < -1)
            {
				if (skill.agent.HasEquipment(42210))
				{
					skill.agent.RecoverMental(skill.agent.maxMental * 0.2f);
				}
				else
				{
					skill.agent.RecoverMental(skill.agent.maxMental * 0.1f);
				}
            }
			_anim.StartWork();
        }
        public override void ParamInit()
        {
            base.ParamInit();
			_attacking = false;
			_healing = false;
			_attackTimer.StopTimer();
			_healTimer.StopTimer();
			model.ResetQliphothCounter();
			_currentDestNode = null;
        }

        public override void Escape()
        {
            base.Escape();
            _attacking = false;
			_healing = false;
			_anim.Escape();
			//this.model.Escape()を書かないと脱走しない。thisは省略可能
			model.Escape();
			MakeMovement();
			_attackTimer.StartTimer(0.5f);
			_healTimer.StartTimer(5f);
		}

        public override void UniqueEscape()
        {
            base.UniqueEscape();
            if (this.model.hp <= 0f)
            {
				return;
            }

			//ここはあまり参考にしない方が良い
            if (_attackTimer.RunTimer())
            {
                _attacking = true;
            }
            if (_attacking)
            {
				InvokeDamage(GetNearUnits());
				_attacking = false;
				_attackTimer.StartTimer(0.5f);
            }
            if (_healTimer.RunTimer())
            {
				_healing = true;
            }
            if (_healing)
            {
				Heal();
				_healTimer.StartTimer(5f);
				_healing = false;
            }
            if (_currentDestNode != null && _currentDestNode == movable.currentNode)
            {
				_currentDestNode = null;
            }
			MakeMovement();
        }

        public override void OnSuppressed()
        {
            base.OnSuppressed();
			this.movable.StopMoving();
        }

        public override void OnReturn()
        {
            base.OnReturn();
			this.model.ResetQliphothCounter();
			ParamInit();
			_anim.Default();
        }

        private List<global::UnitModel> GetNearUnits()
		{
			List<global::UnitModel> list = new List<global::UnitModel>();
			if (this.currentPassage == null)
			{
				return list;
			}
			try
			{
				global::MovableObjectNode[] enteredTargets = this.currentPassage.GetEnteredTargets(this.movable);
				foreach (global::MovableObjectNode movableObjectNode in enteredTargets)
				{
					global::UnitModel unit = movableObjectNode.GetUnit();
					if (!list.Contains(unit))
					{
						if (unit.IsAttackTargetable())
						{
							if (this.movable != movableObjectNode)
							{
								if (this.movable.GetDistanceDouble(movableObjectNode) <= 1f)
								{
									list.Add(unit);
								}
							}
						}
					}
				}
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			return list;
		}
		private void InvokeDamage(List<global::UnitModel> near)
		{
			foreach (global::UnitModel unitModel in near)
			{
				bool flag = unitModel is global::WorkerModel;
				DamageInfo dmg = damageInfo * 0.5f;
				unitModel.TakeDamage(this.model, dmg);
				global::DamageParticleEffect damageParticleEffect = global::DamageParticleEffect.Invoker(unitModel, damageInfo.type, this.model);
				
			}
		}
		public void MakeMovement()
		{
			if (this._currentDestNode == null)
			{
				this._currentDestNode = this.GetRandomNode(false);
			}
			if (!this.movable.IsMoving())
			{
				this.model.MoveToNode(this._currentDestNode);
			}
		}
		private global::MapNode GetRandomNode(bool removeSefira = false)
		{
			global::MapNode roamingNodeByRandom;
			if (removeSefira)
			{
				int num = 10;
				do
				{
					num--;
					roamingNodeByRandom = global::MapGraph.instance.GetRoamingNodeByRandom(this.model.sefira.indexString);
					if (roamingNodeByRandom.GetAttachedPassage().type != global::PassageType.SEFIRA)
					{
						break;
					}
				}
				while (num != 0);
			}
			else
			{
				roamingNodeByRandom = global::MapGraph.instance.GetRoamingNodeByRandom();
			}
			return roamingNodeByRandom;
		}

		private void Heal()
        {
			PassageObjectModel currentPassage = this.model.GetMovableNode().currentPassage;
            if (currentPassage != null)
            {
				foreach (global::MovableObjectNode movableObjectNode in currentPassage.GetEnteredTargets())
				{
					global::WorkerModel workerModel = movableObjectNode.GetUnit() as global::WorkerModel;
					if (workerModel != null)
					{
						if (!workerModel.IsDead())
						{
							if (workerModel.HasEquipment(42210))
							{
								workerModel.RecoverMental(10f);
							}
							else
							{
								workerModel.RecoverMental(6f);
							}
						}
					}
				}
			}

		}
	}
}
