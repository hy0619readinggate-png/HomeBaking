using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C3_G02
{
    public class PlayerInput : MonoBehaviour
    {

        // Properties
        public Direction PressedDirection => inputDirection;



        // Methods
        public void EnableOKButton()
        {
            _OKBTN.interactable = true;
        }
        public void DisableOKButton()
        {
            _OKBTN.interactable = false;
        }
        public void EnableDirectionButton()
        {
            _Top.EnableButton();
            _Bottom.EnableButton();
            _Left.EnableButton();
            _Right.EnableButton();
        }
        public void DisableDirectionButton()
        {
            _Top.DisableButton();
            _Bottom.DisableButton();
            _Left.DisableButton();
            _Right.DisableButton();
        }
        public void SetMovableDirIdx(List<int> dirs)
        {
            var rndDir = UtilArray.Random(0, dirs.Count - 1, 1);

            switch (dirs[rndDir[0]])
            {
                case 0: _AffordanceT.SetActive(true); rndAffDir = _AffordanceT; break;
                case 1: _AffordanceB.SetActive(true); rndAffDir = _AffordanceB; break;
                case 2: _AffordanceL.SetActive(true); rndAffDir = _AffordanceL; break;
                case 3: _AffordanceR.SetActive(true); rndAffDir = _AffordanceR; break;
            }

        }

        public void EndLevel(bool isEnd)
        {
            isLevelEnd = isEnd;
        }
        public void EndOKButton()
        {
            isButtonPressed = false;
        }
        public void StartAffordance()
        {
            StartCoroutine(coShowAffordance());
        }
        public void StopAffordance()
        {
            StopCoroutine(coShowAffordance());
        }


        // Event
        public event Action<bool, Direction> OnDirectionButtonPressed = null;
        public event Action OnOKButton = null;
        public event Action OnAffordanceActive = null;


        // Fields
        private Direction inputDirection = Direction.None;

        private GameObject rndAffDir = null;

        private bool isLevelEnd = false;
        [SerializeField] private bool isButtonPressed = false;
        private bool isAffordanceOn = false;



        // Event Handlers
        private void _Top_OnDown(bool pressed)
        {
            if (inputDirection != Direction.None && inputDirection != Direction.Top)
                return;

            isButtonPressed = pressed;

            if (pressed) AudioMGR.One.PlayEffect(buttonFX);
            inputDirection = pressed ? Direction.Top : Direction.None;

            OnDirectionButtonPressed?.Invoke(pressed, inputDirection);
        }
        private void _Bottom_OnDown(bool pressed)
        {
            if (inputDirection != Direction.None && inputDirection != Direction.Bottom)
                return;

            isButtonPressed = pressed;

            if (pressed) AudioMGR.One.PlayEffect(buttonFX);
            inputDirection = pressed ? Direction.Bottom : Direction.None;

            OnDirectionButtonPressed?.Invoke(pressed, inputDirection);
        }
        private void _Left_OnDown(bool pressed)
        {
            if (inputDirection != Direction.None && inputDirection != Direction.Left)
                return;

            isButtonPressed = pressed;

            if (pressed) AudioMGR.One.PlayEffect(buttonFX);
            inputDirection = pressed ? Direction.Left : Direction.None;

            OnDirectionButtonPressed?.Invoke(pressed, inputDirection);
        }
        private void _Right_OnDown(bool pressed)
        {
            if (inputDirection != Direction.None && inputDirection != Direction.Right)
                return;

            isButtonPressed = pressed;

            if (pressed) AudioMGR.One.PlayEffect(buttonFX);
            inputDirection = pressed ? Direction.Right : Direction.None;

            OnDirectionButtonPressed?.Invoke(pressed, inputDirection);
        }
        private void onOKBTNOnClick()
        {
            isButtonPressed = true;
            OnOKButton?.Invoke();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayerInputButton _Top = null;
        [SerializeField] private PlayerInputButton _Bottom = null;
        [SerializeField] private PlayerInputButton _Left = null;
        [SerializeField] private PlayerInputButton _Right = null;
        [Space()]
        [SerializeField] private Button _OKBTN = null;
        [Space()]
        [SerializeField] private GameObject _AffordanceT = null;
        [SerializeField] private GameObject _AffordanceB = null;
        [SerializeField] private GameObject _AffordanceL = null;
        [SerializeField] private GameObject _AffordanceR = null;
        [Space()]
        [SerializeField] private AudioClip buttonFX = null;

        // Unity Messages
        private void Awake()
        {
            _Top.OnDown += _Top_OnDown;
            _Bottom.OnDown += _Bottom_OnDown;
            _Left.OnDown += _Left_OnDown;
            _Right.OnDown += _Right_OnDown;

            _OKBTN.onClick.AddListener(onOKBTNOnClick);
        }
        private void Start()
        {
            _OKBTN.interactable = false;
        }

        // Unity Coroutines
        IEnumerator coShowAffordance()
        {
            var time = 0f;

            while (true)
            {
                if (!isLevelEnd)
                {
                    if (isButtonPressed)
                    {
                        time = 0f;
                        rndAffDir?.SetActive(false);
                        isAffordanceOn = false;
                    }
                    else
                    {
                        time += Time.deltaTime;

                        if (time >= 10)
                        {
                            if(!isAffordanceOn)
                            {
                                isAffordanceOn = true;
                                OnAffordanceActive?.Invoke();
                            }
                        }
                        //else
                        //{
                        //    rndAffDir?.SetActive(false);
                        //    isAffordanceOn = false;
                        //}

                    }
                    yield return null;

                }
                else break;
            }
        }
    }

}