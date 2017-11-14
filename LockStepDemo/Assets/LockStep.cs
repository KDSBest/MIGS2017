using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Messages;
using UnityEngine;

namespace Assets
{
    public class LockStep
    {
        private const int PlayerCount = 2;
        public int SkipFrames = 4;

        public Dictionary<int, LockstepState> States = new Dictionary<int, LockstepState>();

        public LockStep()
        {
            for (int i = 0; i < 4; i++)
            {
                States.Add(i, new LockstepState()
                {
                    Frame = i,
                    ClickPosition = new Dictionary<byte, Vector2>()
                    {
                        {0, Vector2.negativeInfinity },
                        {1, Vector2.negativeInfinity }
                    }
                });
            }
        }

        public void Add(LockstepState state)
        {
            if (!States.ContainsKey(state.Frame))
            {
                States.Add(state.Frame, state);
            }
            else
            {
                foreach (var cmd in state.ClickPosition)
                {
                    if (!States[state.Frame].ClickPosition.ContainsKey(cmd.Key))
                    {
                        States[state.Frame].ClickPosition.Add(cmd.Key, cmd.Value);
                    }
                }
            }
        }

        public LockstepState GetState(int frame)
        {
            if (States.ContainsKey(frame))
                return States[frame];

            return new LockstepState();
        }

        public bool IsComplete(int frame)
        {
            if (!States.ContainsKey(frame))
                return false;

            return States[frame].ClickPosition.Count == PlayerCount;
        }
    }
}
