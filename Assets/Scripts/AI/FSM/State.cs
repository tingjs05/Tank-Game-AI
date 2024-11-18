using System;
using System.Collections;
using UnityEngine;

namespace AI.FSM
{
    public class State<T>
    {
        protected StateMachine<T> fsm;
        protected T character;

        /// <summary>
        /// Constructor to create a state
        /// </summary>
        /// <param name="fsm">Reference to state machine of type StateMachine</param>
        /// <param name="character">Reference to character of generic type</param>
        public State(StateMachine<T> fsm, T character)
        {
            this.fsm = fsm;
            this.character = character;
        }

        public virtual void Enter() {}
        public virtual void HandleInputs() {}
        public virtual void LogicUpdate() {}
        public virtual void PhysicsUpdate() {}
        public virtual void Exit() {}

        /// <summary>
        /// Coroutine to wait for a set duration in a state
        /// </summary>
        /// <param name="duration">Duration to wait</param>
        /// <param name="callback">Method to call after the wait duration</param>
        /// <returns></returns>
        protected IEnumerator WaitForSeconds(float duration, Action callback = null)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }
    }
}
