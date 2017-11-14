using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Messages;
using UnityEngine;

namespace Assets
{
    public class Simulation
    {
        private float Speed = 0.1f;
        public Vector3 P1NewPosition = Vector3.zero;
        public Vector3 P2NewPosition = Vector3.zero;
        public Vector3 P1GoalPosition = Vector3.zero;
        public Vector3 P2GoalPosition = Vector3.zero;
        public Vector3 P1OldPosition = Vector3.zero;
        public Vector3 P2OldPosition = Vector3.zero;
        public int CurrentFrame = 0;
        public LockStep LockStep = new LockStep();

        public void Step()
        {
            if (!LockStep.IsComplete(CurrentFrame))
                return;

            var state = LockStep.GetState(CurrentFrame);

            P1OldPosition = P1NewPosition;
            P2OldPosition = P2NewPosition;

            if (state.ClickPosition[0].x != Single.NegativeInfinity)
                P1GoalPosition = new Vector3(state.ClickPosition[0].x, 0, state.ClickPosition[0].y);

            if (state.ClickPosition[1].x != Single.NegativeInfinity)
                P2GoalPosition = new Vector3(state.ClickPosition[1].x, 0, state.ClickPosition[1].y);

            if (Math.Abs(Vector3.Distance(P1OldPosition, P1GoalPosition)) <= Speed)
            {
                P1NewPosition = P1GoalPosition;
            }
            else
            {
                P1NewPosition = P1OldPosition + (P1GoalPosition - P1OldPosition).normalized * Speed;
            }

            if (Math.Abs(Vector3.Distance(P2OldPosition, P2GoalPosition)) <= Speed)
            {
                P2NewPosition = P2GoalPosition;
            }
            else
            {
                P2NewPosition = P2OldPosition + (P2GoalPosition - P2OldPosition).normalized * Speed;
            }

            CurrentFrame++;
        }
    }
}
