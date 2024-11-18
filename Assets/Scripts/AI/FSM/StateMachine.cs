using UnityEngine;

namespace AI.FSM
{
    public class StateMachine<T> : MonoBehaviour
    {
        
        public State<T> currentState {get; protected set;}
        public string current_state_name = "None";

        /// <summary>
        /// Event callback
        /// </summary>
        /// <param name="prev">The previous state</param>
        /// <param name="next">The next state</param>
        public delegate void OnStateChanged(State<T> prev, State<T> next);

        /// <summary>
        /// This event is invoked when the state is changed.
        /// </summary>
        public event OnStateChanged StateChanged;

        /// <summary>
        /// Method to initialize the state machine and enter the starting state. 
        /// </summary>
        /// <param name="state">State to start in</param>
        public void Initialize(State<T> state)
        {
            currentState = state;
            UpdateStateName();
            currentState?.Enter();
        }

        /// <summary>
        /// Method to switch states. 
        /// </summary>
        /// <param name="nextState">State to transition into</param>
        public virtual void SwitchState(State<T> nextState)
        {   
            // set previous state
            State<T> prev = currentState;
            // change state
            currentState?.Exit();
            currentState = nextState;
            UpdateStateName();
            currentState?.Enter();
            // Invoke event for side-effects.
            StateChanged?.Invoke(prev, nextState);
        }


        public void UpdateStateName()
        {
            current_state_name =  currentState?.ToString() ?? "None";
        }
        #region Monobehaviour Callbacks

        protected void Update() 
        {
            currentState?.HandleInputs();
            currentState?.LogicUpdate();
        }

        protected void FixedUpdate() 
        {
            currentState?.PhysicsUpdate();
        }

        #endregion
    }
}
