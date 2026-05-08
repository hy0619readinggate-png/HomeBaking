using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.C4_G02
{
    public class Monster : MonoBehaviour
    {

        // Methods
        public void Eat(Action callback)
        {
            eating = true;
            ateAction = callback;

            AudioMGR.One.PlayEffect(_TurtleHitClip);
        }
        public void MoveFast()
        {
            //if (ate)
            //    return;

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            if (moveFastCoroutine == null)
                moveFastCoroutine = StartCoroutine(coMoveFast());
        }



        // Fields
        private bool eating = false;
        private bool ate = false;
        private Coroutine moveCoroutine = null;
        private Coroutine moveFastCoroutine = null; 
        private Action ateAction = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayableDirector _Landing = null;
        [SerializeField] private PlayableDirector _Walk = null;
        [SerializeField] private PlayableDirector _Eat = null;
        [SerializeField] private PlayableDirector _WalkWithAcorn = null;
        //[SerializeField] private PlayableDirector _Death = null;
        [Header("★ AudioClip")]
        [SerializeField] private AudioClip _TurtleAcornClip = null;
        [SerializeField] private AudioClip _TurtleHitClip = null;
        [Header("★ Config")]
        [SerializeField] private float _Speed = 1f;
        [SerializeField] private float _FastSpeed = 1f;

        // Unity Messages
        private void Start()
        {
            if (moveCoroutine == null)
                moveCoroutine = StartCoroutine(coMove());
        }
        private void OnDestroy()
        {
            StopAllCoroutines();

            moveCoroutine = null;
            moveFastCoroutine = null;
        }


        // Unity Coroutine
        IEnumerator coMove()
        {
            _Landing.Play();
            yield return null;
            yield return new WaitUntil(() => _Landing.state != PlayState.Playing);

            _Walk.Play();
            yield return null;


            bool moving = true;
            while (moving)
            {
                if (eating)
                {
                    eating = false;

                    _Eat.Play();
                    yield return null;
                    yield return new WaitUntil(() => _Eat.state != PlayState.Playing);

                    AudioMGR.One.PlayEffect(_TurtleAcornClip);
                    _WalkWithAcorn.Play();

                    ate = true;
                    ateAction?.Invoke();
                    yield return null;
                }

                transform.position += Vector3.left * _Speed * Time.deltaTime;
                yield return null;
            }

            moveCoroutine = null;
            yield return null;
        }
        IEnumerator coMoveFast()
        {
            if (ate)
                _WalkWithAcorn.Play();
            else _Walk.Play();
            yield return null;

            bool moving = true;
            while (moving)
            {
                transform.position += Vector3.left * _FastSpeed * Time.deltaTime;
                yield return null;
            }

            yield return null;
        }
    }
}