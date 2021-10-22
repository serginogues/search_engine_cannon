using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public static class Evaluation
    {
        /// <summary>
        /// Evaluate given Board position
        /// Must return a score relative to the side to being evaluated
        /// </summary>
        public static int Evaluate(BoardState s, AIUtils.eEval eval_f)
        {
            if (s.terminalFlag != CannonUtils.eNode.leaf)
            {
                // terminal node
                int score = 100000;
                return s.terminalFlag == CannonUtils.eNode.light_wins ? -score : score;
            }
            else
            {
                switch (eval_f)
                {
                    case AIUtils.eEval.color:
                        return (s.boardCounter.darkPieceList.Count - s.boardCounter.lightPieceList.Count) * 10;
                    case AIUtils.eEval.dist2EnemyTown:
                        return Evaluation.dist2Town(s);
                    case AIUtils.eEval.mobility:
                        return Evaluation.mobility(s);
                    case AIUtils.eEval.safeMobility:
                        return Evaluation.safeMobility(s);
                }
                return 0;
            }
        }


        private static int dist2Town(BoardState s)
        {
            int dark_score = 0;
            int light_score = 0;
            foreach (int soldier in s.boardCounter.darkPieceList)
            {
                dark_score += (9 - AIUtils.ChebyshevDistance(soldier, s.lightTown)) * 10;
            }
            foreach (int soldier in s.boardCounter.lightPieceList)
            {
                light_score += (9 - AIUtils.ChebyshevDistance(soldier, s.darkTown)) * 10;
            }
            return dark_score - light_score;
        }

        private static int mobility(BoardState s)
        {
            BoardState sNext = s.DeepCopy();
            sNext.turnCounter++;
            sNext.generateLegalMoves();

            int n_dark;
            int n_light;
            if (s.friendSoldier == CannonUtils.eSoldiers.dark_soldier)
            {
                n_dark = s.legalMoves.Count;
                n_light = sNext.legalMoves.Count;
            }
            else
            {
                n_dark = sNext.legalMoves.Count;
                n_light = s.legalMoves.Count;
            }

            return (n_dark - n_light) * 10;
        }

        private static int safeMobility(BoardState s) 
        {
            BoardState s2 = s.DeepCopy();
            s2.turnCounter++;
            s2.generateLegalMoves();
            BoardState darkState;
            BoardState lightState;
            if (s.friendSoldier == CannonUtils.eSoldiers.dark_soldier)
            {
                darkState = s;
                lightState = s2;
            }
            else
            {
                darkState = s2;
                lightState = s;
            }

            // dark
            BoardCounter s_dark = darkState.boardCounter;
            int dark_score = 10 * s_dark.moveTypeCounter[(int)CannonUtils.eMoves.step];
            dark_score += 50 * s_dark.moveTypeCounter[(int)CannonUtils.eMoves.shootCannon];
            dark_score += 40 * s_dark.moveTypeCounter[(int)CannonUtils.eMoves.capture];
            dark_score += darkState.legalMoves.Any(x => x.targetIndex == darkState.lightTown) ? 1000 : 0;

            // light
            BoardCounter s_light = lightState.boardCounter;
            int light_score = 10 * s_light.moveTypeCounter[(int)CannonUtils.eMoves.step];
            light_score += 50 * s_light.moveTypeCounter[(int)CannonUtils.eMoves.shootCannon];
            light_score += 40 * s_light.moveTypeCounter[(int)CannonUtils.eMoves.capture];
            light_score += lightState.legalMoves.Any(x => x.targetIndex == lightState.darkTown) ? 1000 : 0;

            return dark_score - light_score;
        }

        //private int evalSafeMobility(BoardState s)
        //{
        //    BoardState new_s = s.DeepCopy();
        //    new_s.TurnCounter++;
        //    new_s.MoveOrderingLegalMoves();
        //    int s_safe_moves = s.LegalMoves.Count(m => m.Type != CannonUtils.IMoves.capture || m.Type != CannonUtils.IMoves.retreat);
        //    int new_s_safe_moves = new_s.LegalMoves.Count(m => m.Type != CannonUtils.IMoves.capture || m.Type != CannonUtils.IMoves.retreat);
        //    int n_friend_moves = 0;
        //    int n_enemy_moves = 0;

        //    if (Color)
        //    {
        //        // AI is dark pieces
        //        n_friend_moves = s.Friend == CannonUtils.ISoldiers.dark_soldier ? s_safe_moves : new_s_safe_moves;
        //        n_enemy_moves = s.Friend == CannonUtils.ISoldiers.dark_soldier ? new_s_safe_moves : s_safe_moves;
        //    }
        //    else
        //    {
        //        // AI is light pieces
        //        n_friend_moves = s.Friend == CannonUtils.ISoldiers.dark_soldier ? new_s_safe_moves : s_safe_moves;
        //        n_enemy_moves = s.Friend == CannonUtils.ISoldiers.dark_soldier ? s_safe_moves : new_s_safe_moves;
        //    }

        //    return (n_friend_moves - n_enemy_moves) * 10;
        //}



        ///// <summary>
        ///// evaluate soldier based on its current row distance from enemy town
        ///// </summary>
        //private int evalByTypeAndRow(Cell soldier)
        //{
        //    int w_2 = 100; // pieces that are 2 rows away from enemy town
        //    int w_4 = 70; // pieces that are 4 rows away from enemy town
        //    int w_6 = 50; // pieces that are 6 rows away from enemy town
        //    int w_9 = 10; // pieces that are 8 to 10 rows away from enemy town

        //    int score = 0;
        //    if (soldier.Piece == CannonUtils.ISoldiers.dark_soldier)
        //    {
        //        if (soldier.Row > 6) { score = w_2; }
        //        else if (soldier.Row > 4) { score = w_4; }
        //        else if (soldier.Row > 2) { score = w_6; }
        //        else { score = w_9; }
        //        return Color ? score : -score;
        //    }
        //    else
        //    {
        //        if (soldier.Row < 3) { score = w_2; }
        //        else if (soldier.Row < 5) { score = w_4; }
        //        else if (soldier.Row < 7) { score = w_6; }
        //        else { score = w_9; }

        //        return Color ? -score : score;
        //    }
        //}
    }
}
