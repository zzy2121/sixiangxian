using System;

namespace Minesweeper
{
    public class SudokuGame
    {
        private const int SIZE = 9;
        private int[,] board;
        private bool[,] fixedCells;
        private Random random;

        public SudokuGame()
        {
            board = new int[SIZE, SIZE];
            fixedCells = new bool[SIZE, SIZE];
            random = new Random();
        }

        public void GenerateNewGame(int difficulty)
        {
            // 清空棋盘
            ClearBoard();
            
            // 生成完整的数独解
            GenerateCompleteSudoku();
            
            // 根据难度移除数字
            RemoveNumbers(difficulty);
            
            // 标记固定的单元格
            MarkFixedCells();
        }

        private void ClearBoard()
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    board[row, col] = 0;
                    fixedCells[row, col] = false;
                }
            }
        }

        private void GenerateCompleteSudoku()
        {
            // 使用回溯算法生成完整的数独
            SolveSudoku();
        }

        private void RemoveNumbers(int difficulty)
        {
            int numbersToRemove;
            switch (difficulty)
            {
                case 0: // 简单
                    numbersToRemove = 40;
                    break;
                case 1: // 中等
                    numbersToRemove = 50;
                    break;
                case 2: // 困难
                    numbersToRemove = 60;
                    break;
                default:
                    numbersToRemove = 40;
                    break;
            }

            int removed = 0;
            while (removed < numbersToRemove)
            {
                int row = random.Next(SIZE);
                int col = random.Next(SIZE);
                
                if (board[row, col] != 0)
                {
                    board[row, col] = 0;
                    removed++;
                }
            }
        }

        private void MarkFixedCells()
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    fixedCells[row, col] = board[row, col] != 0;
                }
            }
        }

        public bool Solve()
        {
            return SolveSudoku();
        }

        private bool SolveSudoku()
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (board[row, col] == 0)
                    {
                        var numbers = GetShuffledNumbers();
                        foreach (int num in numbers)
                        {
                            if (IsValidMove(row, col, num))
                            {
                                board[row, col] = num;
                                
                                if (SolveSudoku())
                                    return true;
                                
                                board[row, col] = 0;
                            }
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private int[] GetShuffledNumbers()
        {
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            
            // Fisher-Yates 洗牌算法
            for (int i = numbers.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = numbers[i];
                numbers[i] = numbers[j];
                numbers[j] = temp;
            }
            
            return numbers;
        }

        public bool IsValidMove(int row, int col, int num)
        {
            // 检查行
            for (int c = 0; c < SIZE; c++)
            {
                if (c != col && board[row, c] == num)
                    return false;
            }

            // 检查列
            for (int r = 0; r < SIZE; r++)
            {
                if (r != row && board[r, col] == num)
                    return false;
            }

            // 检查3x3方块
            int boxRow = (row / 3) * 3;
            int boxCol = (col / 3) * 3;
            
            for (int r = boxRow; r < boxRow + 3; r++)
            {
                for (int c = boxCol; c < boxCol + 3; c++)
                {
                    if ((r != row || c != col) && board[r, c] == num)
                        return false;
                }
            }

            return true;
        }

        public bool IsValid()
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (board[row, col] != 0)
                    {
                        int temp = board[row, col];
                        board[row, col] = 0;
                        
                        if (!IsValidMove(row, col, temp))
                        {
                            board[row, col] = temp;
                            return false;
                        }
                        
                        board[row, col] = temp;
                    }
                }
            }
            return true;
        }

        public bool IsComplete()
        {
            // 检查是否所有格子都填满了
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (board[row, col] == 0)
                        return false;
                }
            }
            
            // 检查是否有效
            return IsValid();
        }

        public int GetCell(int row, int col)
        {
            if (row >= 0 && row < SIZE && col >= 0 && col < SIZE)
                return board[row, col];
            return 0;
        }

        public void SetCell(int row, int col, int value)
        {
            if (row >= 0 && row < SIZE && col >= 0 && col < SIZE && !fixedCells[row, col])
            {
                board[row, col] = value;
            }
        }

        public bool IsFixedCell(int row, int col)
        {
            if (row >= 0 && row < SIZE && col >= 0 && col < SIZE)
                return fixedCells[row, col];
            return false;
        }
    }
}