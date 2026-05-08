using System;
using UnityEngine;

#pragma warning disable 0414

namespace beyondi.FSM
{
    public abstract class StateMachineBase<T> : MonoBehaviour where T : struct, IConvertible
    {
        // Properties
        public T CurrentState
        {
            get { return fsm.CurrentState; }
        }
        public bool IsStarted { get { return fsm.IsStarted; } }



        // Fields
        private FSM<T> fsm = null;

        // Functions : build
        protected void addState(T stateID,
            CoroutineHandler enterFunc = null,
            CoroutineHandler exitFunc = null)
        {
            fsm.AddState(stateID, enterFunc, exitFunc);
        }
        protected void startFSM(T stateID)
        {
            fsm.StartFSM(stateID);
        }
        protected void stopFSM()
        {
            fsm.StopFSM();
        }
        protected void performTransition(T stateID)
        {
            fsm.PerformTransition(stateID);
        }



        // Unity Messages
        protected virtual void Awake()
        {
            fsm = new FSM<T>(this);
        }
    }
}