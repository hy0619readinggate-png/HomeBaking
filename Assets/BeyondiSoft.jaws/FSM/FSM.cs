using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0414

namespace beyondi.FSM
{
    public delegate IEnumerator CoroutineHandler();

    public class FSM<T> where T : struct, IConvertible
    {
        // Properties
        public T CurrentState
        {
            get
            {
                if (currentState == null)
                    return default(T);
                else return currentState.ID;
            }
        }
        public bool IsStarted { get { return isStarted; } }
        public FSMEvent OnChanged { get { return onChanged; } }

        // Methods : ctor.
        public FSM(MonoBehaviour owner)
        {
            this.owner = owner;
        }

        // Methods : build
        public void AddState(T stateID,
            CoroutineHandler enterFunc = null,
            CoroutineHandler exitFunc = null)
        {
            State state = new State();
            state.ID = stateID;
            state.StartFunc = enterFunc;
            state.ExitFunc = exitFunc;
            states[stateID] = state;
        }
        public void StartFSM(T stateID, string skipStateTo)
        {
            if (!string.IsNullOrEmpty(skipStateTo))
            {
                if (Enum.TryParse<T>(skipStateTo, true, out var state))
                {
                    StartFSM(state);
                }
                else Debug.LogError($"FSM [{owner.name}] No state exists for {skipStateTo}");
            }
            else StartFSM(stateID);
        }
        public void StartFSM(T stateID)
        {
            owner.StartCoroutine(runState(states[stateID]));
            isStarted = true;
        }
        public void StopFSM()
        {
            owner.StopAllCoroutines();
            currentCoroutineEnter = null;
            isStarted = false;
            currentState = null;
        }
        public void PerformTransition(T stateID)
        {
            if(!isStarted)
            {
                Debug.LogWarning($"FSM [{owner.name}] It is not started.");
                return;
            }

            if (!states.ContainsKey(stateID))
            {
                Debug.LogWarning($"FSM [{owner.name}] State '{stateID}' was not found.");
                return;
            }

            if (isTransiting)
            {
                Debug.LogWarning($"FSM [{owner.name}] It cannot transit for isTransiting.");
                return;
            }

            owner.StartCoroutine(runState(states[stateID]));
        }



        // Fields
        private MonoBehaviour owner;
        private FSMEvent onChanged = new FSMEvent();
        private Dictionary<T, State> states = new Dictionary<T, State>();
        private State currentState;
        private IEnumerator currentCoroutineEnter;
        private bool isStarted = false;
        private bool isTransiting = false;

        // Functions
        private IEnumerator runState(State state)
        {
            isTransiting = true;

            // Clear Previous State
            if (currentCoroutineEnter != null)
                owner.StopCoroutine(currentCoroutineEnter);
            currentCoroutineEnter = null;

            if (currentState != null && currentState.ExitFunc != null)
                yield return owner.StartCoroutine(currentState.ExitFunc());

            Debug.Log($"<color=yellow>FSM [{owner.name}] <b>{state.ID}</b></color>");

            // Assign New State
            currentState = state;
            onChanged.Invoke(currentState.ID);

            isTransiting = false;

            // Start New State
            if (currentState.StartFunc != null)
            {
                currentCoroutineEnter = currentState.StartFunc();
                owner.StartCoroutine(currentCoroutineEnter);
            }
        }


        // Inner class
        private class State
        {
            public T ID;
            public CoroutineHandler StartFunc;
            public CoroutineHandler ExitFunc;
        }
        public class FSMEvent : UnityEvent<T> { }
    }
}