using System;
using System.Collections.Generic;

namespace Minesweeper
{
    public class GameBoard
    {
        public enum CellRevealResult
        {
            Number,
            EmptyArea,
            Mine
        }

        public class Cell
        {
            public bool IsMine { get; set; }
            public bool IsRevealed { get; set; }
            public bool IsFlagged { get; set; }
            public int AdjacentMines { get; set; }
        }

        private Cell[,] board;
        private Random random;
        
        public int Width { get; }
        public int Height { get; }
        public int TotalMines { get; }
        public int RemainingMines { get; private set; }
        public bool IsGameOver { get; set; }

        public GameBoard(int width, int height, int mines)
        {
            Width = width;
            Height = height;
            TotalMines = mines;
            RemainingMines = mines;
            random = new Random();
            
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            board = new Cell[Width, Height];
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    board[x, y] = new Cell();
                }
            }
        }

        public void InitializeMines(int firstClickX, int firstClickY)
        {
            var minePositions = new HashSet<(int, int)>();
            
            // 生成地雷位置，避开第一次点击的位置
            while (minePositions.Count < TotalMines)
            {
                int x = random.Next(Width);
                int y = random.Next(Height);
                
                if (x != firstClickX || y != firstClickY)
                {
                    minePositions.Add((x, y));
                }
            }

            // 放置地雷
            foreach (var (x, y) in minePositions)
            {
                board[x, y].IsMine = true;
            }

            // 计算相邻地雷数
            CalculateAdjacentMines();
        }

        private void CalculateAdjacentMines()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!board[x, y].IsMine)
                    {
                        board[x, y].AdjacentMines = CountAdjacentMines(x, y);
                    }
                }
            }
        }

        private int CountAdjacentMines(int x, int y)
        {
            int count = 0;
            
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    if (IsValidPosition(nx, ny) && board[nx, ny].IsMine)
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }

        public CellRevealResult RevealCell(int x, int y)
        {
            if (!IsValidPosition(x, y) || board[x, y].IsRevealed || board[x, y].IsFlagged)
                return CellRevealResult.Number;

            board[x, y].IsRevealed = true;

            if (board[x, y].IsMine)
            {
                return CellRevealResult.Mine;
            }

            if (board[x, y].AdjacentMines == 0)
            {
                return CellRevealResult.EmptyArea;
            }

            return CellRevealResult.Number;
        }

        public List<(int x, int y)> GetEmptyAreaCells(int startX, int startY)
        {
            var revealed = new List<(int, int)>();
            var toCheck = new Queue<(int, int)>();
            var visited = new HashSet<(int, int)>();
            
            toCheck.Enqueue((startX, startY));
            visited.Add((startX, startY));

            while (toCheck.Count > 0)
            {
                var (x, y) = toCheck.Dequeue();
                
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        
                        if (!IsValidPosition(nx, ny) || visited.Contains((nx, ny)))
                            continue;
                            
                        visited.Add((nx, ny));
                        
                        if (!board[nx, ny].IsMine && !board[nx, ny].IsFlagged)
                        {
                            board[nx, ny].IsRevealed = true;
                            revealed.Add((nx, ny));
                            
                            if (board[nx, ny].AdjacentMines == 0)
                            {
                                toCheck.Enqueue((nx, ny));
                            }
                        }
                    }
                }
            }
            
            return revealed;
        }

        public void ToggleFlag(int x, int y)
        {
            if (!IsValidPosition(x, y) || board[x, y].IsRevealed)
                return;

            board[x, y].IsFlagged = !board[x, y].IsFlagged;
            RemainingMines += board[x, y].IsFlagged ? -1 : 1;
        }

        public bool IsWon()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = board[x, y];
                    if (!cell.IsMine && !cell.IsRevealed)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Cell GetCell(int x, int y)
        {
            return IsValidPosition(x, y) ? board[x, y] : null;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}