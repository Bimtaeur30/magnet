using System;

namespace GGMLib.FSM
{
    public sealed class StateMachine<TContext>
    {
        private readonly TContext _context;

        public IState<TContext> CurrentState { get; private set; }

        public StateMachine(TContext context)
        {
            _context = context;
        }

        public void ChangeState(IState<TContext> nextState)
        {
            if (nextState == null)
                throw new ArgumentNullException(nameof(nextState));

            if (ReferenceEquals(CurrentState, nextState))
                return;

            CurrentState?.Exit(_context);
            CurrentState = nextState;
            CurrentState.Enter(_context);
        }

        public void Update()
        {
            CurrentState?.Update(_context);
        }

        public void Clear()
        {
            CurrentState?.Exit(_context);
            CurrentState = null;
        }
    }
}
