using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class MainForm : Form
    {
        private const int CELL_SIZE = 25;
        private GameBoard gameBoard;
        private Button[,] buttons;
        private Label mineCountLabel;
        private Label timerLabel;
        private Button resetButton;
        private System.Windows.Forms.Timer gameTimer;
        private int elapsedSeconds;
        private bool gameStarted;

        // 难度设置
        private readonly (int width, int height, int mines)[] difficulties = {
            (9, 9, 10),   // 初级
            (16, 16, 40), // 中级
            (30, 16, 99)  // 高级
        };
        
        private int currentDifficulty = 0;

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.Text = "扫雷游戏 - Minesweeper";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建菜单栏
            var menuStrip = new MenuStrip();
            
            // 扫雷难度菜单
            var difficultyMenu = new ToolStripMenuItem("难度");
            
            var easyMenuItem = new ToolStripMenuItem("初级 (9×9, 10雷)");
            easyMenuItem.Click += (s, e) => ChangeDifficulty(0);
            
            var mediumMenuItem = new ToolStripMenuItem("中级 (16×16, 40雷)");
            mediumMenuItem.Click += (s, e) => ChangeDifficulty(1);
            
            var hardMenuItem = new ToolStripMenuItem("高级 (30×16, 99雷)");
            hardMenuItem.Click += (s, e) => ChangeDifficulty(2);

            difficultyMenu.DropDownItems.AddRange(new ToolStripItem[] {
                easyMenuItem, mediumMenuItem, hardMenuItem
            });
            
            // 添加返回游戏中心菜单
            var backMenuItem = new ToolStripMenuItem("返回游戏中心");
            backMenuItem.Click += (s, e) => {
                var gameCenterForm = new GameCenterForm();
                gameCenterForm.Show();
                this.Close();
            };
            
            menuStrip.Items.AddRange(new ToolStripItem[] { difficultyMenu, backMenuItem });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 创建状态面板
            var statusPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.LightGray
            };

            mineCountLabel = new Label
            {
                Text = "雷数: 10",
                Location = new Point(10, 10),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            resetButton = new Button
            {
                Text = "😊",
                Size = new Size(30, 30),
                Font = new Font("Arial", 12),
                Location = new Point(200, 5)
            };
            resetButton.Click += ResetButton_Click;

            timerLabel = new Label
            {
                Text = "时间: 000",
                Size = new Size(80, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(350, 10)
            };

            statusPanel.Controls.AddRange(new Control[] { mineCountLabel, resetButton, timerLabel });
            this.Controls.Add(statusPanel);

            // 初始化计时器
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;
        }

        private void InitializeGame()
        {
            var (width, height, mines) = difficulties[currentDifficulty];
            
            // 清除现有按钮
            if (buttons != null)
            {
                foreach (var btn in buttons)
                {
                    if (btn != null)
                        this.Controls.Remove(btn);
                }
            }

            // 创建游戏板
            gameBoard = new GameBoard(width, height, mines);
            buttons = new Button[width, height];

            // 调整窗口大小
            int formWidth = width * CELL_SIZE + 20;
            int formHeight = height * CELL_SIZE + 120;
            this.Size = new Size(formWidth, formHeight);

            // 创建按钮网格
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var button = new Button
                    {
                        Size = new Size(CELL_SIZE, CELL_SIZE),
                        Location = new Point(10 + x * CELL_SIZE, 80 + y * CELL_SIZE),
                        Font = new Font("Arial", 8, FontStyle.Bold),
                        Tag = new Point(x, y),
                        UseVisualStyleBackColor = true
                    };

                    button.MouseDown += Button_MouseDown;
                    buttons[x, y] = button;
                    this.Controls.Add(button);
                }
            }

            // 重置状态
            mineCountLabel.Text = $"雷数: {mines}";
            timerLabel.Text = "时间: 000";
            resetButton.Text = "😊";
            elapsedSeconds = 0;
            gameStarted = false;
            gameTimer.Stop();
        }

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (gameBoard.IsGameOver) return;

            var button = (Button)sender;
            var pos = (Point)button.Tag;
            int x = pos.X, y = pos.Y;

            if (!gameStarted)
            {
                gameBoard.InitializeMines(x, y);
                gameStarted = true;
                gameTimer.Start();
            }

            if (e.Button == MouseButtons.Left)
            {
                if (button.Text == "🚩") return; // 已标记的不能点击

                var result = gameBoard.RevealCell(x, y);
                UpdateButton(x, y);

                if (result == GameBoard.CellRevealResult.Mine)
                {
                    GameOver(false);
                }
                else if (result == GameBoard.CellRevealResult.EmptyArea)
                {
                    RevealEmptyArea(x, y);
                }

                if (gameBoard.IsWon())
                {
                    GameOver(true);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (gameBoard.GetCell(x, y).IsRevealed) return;

                gameBoard.ToggleFlag(x, y);
                UpdateButton(x, y);
                mineCountLabel.Text = $"雷数: {gameBoard.RemainingMines}";
            }
        }

        private void RevealEmptyArea(int startX, int startY)
        {
            var toReveal = gameBoard.GetEmptyAreaCells(startX, startY);
            foreach (var (x, y) in toReveal)
            {
                UpdateButton(x, y);
            }
        }

        private void UpdateButton(int x, int y)
        {
            var cell = gameBoard.GetCell(x, y);
            var button = buttons[x, y];

            if (cell.IsFlagged)
            {
                button.Text = "🚩";
                button.BackColor = Color.Yellow;
            }
            else if (cell.IsRevealed)
            {
                if (cell.IsMine)
                {
                    button.Text = "💣";
                    button.BackColor = Color.Red;
                }
                else
                {
                    int adjacentMines = cell.AdjacentMines;
                    button.Text = adjacentMines > 0 ? adjacentMines.ToString() : "";
                    button.BackColor = Color.LightGray;
                    button.ForeColor = GetNumberColor(adjacentMines);
                }
                button.Enabled = false;
            }
            else
            {
                button.Text = "";
                button.BackColor = SystemColors.Control;
            }
        }

        private Color GetNumberColor(int number)
        {
            return number switch
            {
                1 => Color.Blue,
                2 => Color.Green,
                3 => Color.Red,
                4 => Color.Purple,
                5 => Color.Maroon,
                6 => Color.Turquoise,
                7 => Color.Black,
                8 => Color.Gray,
                _ => Color.Black
            };
        }

        private void GameOver(bool won)
        {
            gameTimer.Stop();
            gameBoard.IsGameOver = true;
            resetButton.Text = won ? "😎" : "😵";

            if (!won)
            {
                // 显示所有地雷
                for (int x = 0; x < gameBoard.Width; x++)
                {
                    for (int y = 0; y < gameBoard.Height; y++)
                    {
                        var cell = gameBoard.GetCell(x, y);
                        if (cell.IsMine && !cell.IsFlagged)
                        {
                            UpdateButton(x, y);
                        }
                    }
                }
            }

            string message = won ? $"恭喜！你赢了！\n用时: {elapsedSeconds} 秒" : "游戏结束！";
            MessageBox.Show(message, "游戏结果", MessageBoxButtons.OK, 
                won ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            InitializeGame();
        }

        private void ChangeDifficulty(int difficulty)
        {
            currentDifficulty = difficulty;
            InitializeGame();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
            timerLabel.Text = $"时间: {elapsedSeconds:D3}";
        }


    }
}