using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosJerry
{
    class CosmosJerryAnim:CreatureAnimScript
    {
        private CosmosJerry script;
        private new SkeletonAnimation animator;
        public void SetScript(CosmosJerry sc)
        {
            this.script = sc;
            this.animator = base.gameObject.GetComponent<SkeletonAnimation>();
            //アブノーマリティのサイズを変更
            animator.transform.localScale *= 0.5f;
            Default();
        }

        public void Default()
        {
            animator.AnimationState.SetAnimation(0, "default", true);
        }

        //StartWorkで使われてる
        public void Default(TrackEntry entry)
        {
            animator.AnimationState.SetAnimation(0, "default", true);
        }

        public void StartWork()
        {
            animator.AnimationState.SetAnimation(0, "start_work", false).Complete += Default;
        }

        public void Escape()
        {
            animator.AnimationState.SetAnimation(0, "move", true);
        }
    }
}
