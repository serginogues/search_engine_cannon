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
        public List<Child> MoveOrderingList { get; set; }

        public Node(BoardState s)
        {
            State = s;
            MoveOrderingList = new List<Child>();
            for (int i = 0; i < State.LegalMoves.Count; i++)
            {
                MoveOrderingList.Add(new Child(i, -1000));
            }
        }

        /// <summary>
        /// Evaluate State
        /// </summary>
        public int Evaluate()
        {
            Random rnd = new Random();
            return rnd.Next(-200, 200);
        }

        public bool TerminalState()
        {
            if (MoveOrderingList.Count == 0) { return true; }
            else { return false; }
        }

        public Node Successor(int move_id)
        {
            State.ExecuteMove(State.LegalMoves[move_id]);
            Node child = new Node(State);
            return child;
        }
    }

    public class Child
    {
        public int Value { get; set; }
        public int Id { get; set; }

        public Child (int id, int value)
        {
            Value = value;
            Id = id;
        }
    }
}
