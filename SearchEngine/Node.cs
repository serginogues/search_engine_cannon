using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public class Node
    {
        public BoardState State { get; set; }
        public static int NodesEvaluated = 0;

        public Node(BoardState s)
        {
            State = s;
            NodesEvaluated++;
        }

        /// <summary>
        /// Evaluate State
        /// </summary>
        public int Evaluate()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(-200,200);
        }

        public bool TerminalState()
        {
            if (State.LegalMoves.Count == 0) { return true; }
            else { return false; }
        }

        public Node Successor(int move_id)
        {
            try
            {
                BoardState child = State.DeepCopy();
                //if (child.LegalMoves[0] == State.LegalMoves[0])
                //{
                //    Console.Write("bad");
                //}
                child.ExecuteMove(State.LegalMoves[move_id]);
                return new Node(child);
            }
            catch (Exception)
            {
                return new Node(State);
            }
            
        }
    }
}
