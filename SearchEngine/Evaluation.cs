using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CannonModel;

namespace SearchEngine
{
    public static class Evaluation
    {
        public static int byColor(BoardState s)
        {
            int n_dark = s.SoldierList.Count(x => x.Piece == CannonUtils.ISoldiers.dark_soldier);
            int n_light = s.SoldierList.Count(x => x.Piece == CannonUtils.ISoldiers.light_soldier);
            return (n_dark - n_light) * 10;
        }

        public static int evalByTypeAndDistanceToTown(BoardState s)
        {
            int score = 0;
            foreach (Cell soldier in s.SoldierList)
            {
                if(soldier.Piece == CannonUtils.ISoldiers.dark_soldier) 
                { 
                    score = score + CannonUtils.ChebyshevDistance(soldier, s.LightTown) * -10; 
                }
                else
                {
                    score = score - CannonUtils.ChebyshevDistance(soldier, s.DarkTown) * -10;
                }
            }
            return score;
        }

        public static int mobility(BoardState s)
        {
            BoardState s2 = s.DeepCopy();
            s2.TurnCounter++;
            s2.initLegalMoves();

            int n_dark = s.Friend == CannonUtils.ISoldiers.dark_soldier ? s.FriendLegalMoves.Count : s2.FriendLegalMoves.Count;
            int n_light = s.Friend == CannonUtils.ISoldiers.dark_soldier ? s2.FriendLegalMoves.Count : s.FriendLegalMoves.Count;

            return (n_dark - n_light) * 10;
        }

        public static int safe_mobility(BoardState s) 
        {
            BoardState s2 = s.DeepCopy();
            s2.TurnCounter++;
            s2.initLegalMoves();
            BoardState s_dark;
            BoardState s_light;
            if (s.Friend == CannonUtils.ISoldiers.dark_soldier)
            {
                s_dark = s;
                s_light = s2;
            }
            else
            {
                s_dark = s2;
                s_light = s;
            }

            // dark
            int dark_score = 10 * s_dark.FriendLegalMoves.Count(x => x.Type == CannonUtils.IMoves.step);
            dark_score = dark_score + 50 * s_dark.FriendLegalMoves.Count(x => x.Type == CannonUtils.IMoves.shootCannon);
            dark_score = dark_score + 40 * s_dark.FriendLegalMoves.Count(x => x.Type == CannonUtils.IMoves.capture);
            dark_score = dark_score + 1000 * s_dark.FriendLegalMoves.Count(x => x.NewCell.Piece == CannonUtils.ISoldiers.light_town);

            // light
            int light_score = 10 * s_light.FriendLegalMoves.Count(x => x.Type == CannonUtils.IMoves.step);
            light_score = light_score + 50 * s_light.FriendLegalMoves.Count(x => x.Type == CannonUtils.IMoves.shootCannon);
            light_score = light_score + 40 * s_light.FriendLegalMoves.Count(x => x.Type == CannonUtils.IMoves.capture);
            light_score = light_score + 1000 * s_light.FriendLegalMoves.Count(x => x.NewCell.Piece == CannonUtils.ISoldiers.dark_town);

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
