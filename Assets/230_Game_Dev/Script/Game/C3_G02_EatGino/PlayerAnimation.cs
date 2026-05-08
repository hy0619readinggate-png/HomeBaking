using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public enum CharacterState
    {
        Stand,
        MoveL,
        MoveR
    }

    public class PlayerAnimation : MonoBehaviour
    {

        // Methods
        public bool IsSame(int index)
        {
            index = index + (int)JinoAnimation.Top_Eat;
            return runAnimation == (JinoAnimation)index;
        }
        public bool IsPlaying()
        {
            if (runAnimation == JinoAnimation.Idle)
                return false;
            else return _Animation.IsAnimationPlaying(runAnimation);
        }
        public void Flip(CharacterState state)
        {
            characterState = state;

            flip();
        }
        public void Idle()
        {
            characterState = CharacterState.Stand;

            play(JinoAnimation.Idle);
        }
        public void WalkEat(int index, CharacterState state)
        {
            characterState = state;

            index = index + (int)JinoAnimation.Top_WalkEat;

            play((JinoAnimation)index, true);
        }
        public void Walk(int index, CharacterState state)
        {
            characterState = state;

            index = index + (int)JinoAnimation.Top_Walk;

            play((JinoAnimation)index);
        }
        public void Eat(int index, CharacterState state)
        {
            characterState = state;

            index = index + (int)JinoAnimation.Top_Eat;

            play((JinoAnimation)index, true);
        }
        //public void Jelly(int index)
        //{
        //    // Timeline으로 대체

        //    characterState = CharacterState.Stand;

        //    index = index + (int)JinoAnimation.Top_Jelly;

        //    play((JinoAnimation)index);
        //}



        // Fields
        private JinoAnimation runAnimation = JinoAnimation.Idle;
        private CharacterState characterState = CharacterState.Stand;



        // Functions
        private void play(JinoAnimation value, bool force = false)
        {

            if (runAnimation != value || force)
            {
                runAnimation = value;

                flip();


                var runANimationIndex = (int)runAnimation;
                if (runANimationIndex >= (int)JinoAnimation.Top_WalkEat)
                {

                    JinoAnimation next = JinoAnimation.Idle;
                    if (runANimationIndex >= (int)JinoAnimation.Top_Eat)
                    {
                        next = (JinoAnimation)(runANimationIndex - (int)JinoAnimation.Top_Eat + (int)JinoAnimation.Top_Walk);
                    }
                    else
                    {
                        next = (JinoAnimation)(runANimationIndex - (int)JinoAnimation.Top_WalkEat + (int)JinoAnimation.Top_Walk);

                    }

                    _Animation.PlayAnimation(runAnimation, next);
                }
                else
                {
                    _Animation.PlayAnimationLoop(runAnimation);
                }
            }
        }
        private void flip()
        {
            switch (characterState)
            {
                case CharacterState.MoveR:
                    _Animation.FlipX(false);
                    break;

                case CharacterState.MoveL:
                    _Animation.FlipX(true);
                    break;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GinoAnimation _Animation = null;

    }
}