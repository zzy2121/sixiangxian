using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class SudokuForm : Form
    {
        private const int CELL_SIZE = 40;
        private const int GRID_SIZE = 9;
        private TextBox[,] cells;
        private SudokuGame sudokuGame;
        private Button newGameButton;
        private Button solveButton;
        private Button checkButton;
        private ComboBox difficultyCombo;
        private Label statusLabel;

        public SudokuForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.Text = "数独游戏 - Sudoku";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(420, 520);

            // 创建菜单栏
            var menuStrip = new MenuStrip();
            var backMenuItem = new ToolStripMenuItem("返回游戏中心");
            backMenuItem.Click += (s, e) => {
                var gameCenterForm = new GameCenterForm();
                gameCenterForm.Show();
                this.Close();
            };
            menuStrip.Items.Add(backMenuItem);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 创建控制面板
            var controlPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.LightGray
            };

            difficultyCombo = new ComboBox
            {
                Location = new Point(10, 15),
                Size = new Size(80, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            difficultyCombo.Items.AddRange(new[] { "简单", "中等", "困难" });
            difficultyCombo.SelectedIndex = 0;

            newGameButton = new Button
            {
                Text = "新游戏",
                Location = new Point(100, 15),
                Size = new Size(70, 25)
            };
            newGameButton.Click += NewGameButton_Click;

            solveButton = new Button
            {
                Text = "求解",
                Location = new Point(180, 15),
                Size = new Size(60, 25)
            };
            solveButton.Click += SolveButton_Click;

            checkButton = new Button
            {
                Text = "检查",
                Location = new Point(250, 15),
                Size = new Size(60, 25)
            };
            checkButton.Click += CheckButton_Click;

            statusLabel = new Label
            {
                Text = "开始新游戏",
                Location = new Point(10, 35),
                Size = new Size(300, 20),
                ForeColor = Color.Blue
            };

            controlPanel.Controls.AddRange(new Control[] { 
                difficultyCombo, newGameButton, solveButton, checkButton, statusLabel 
            });
            this.Controls.Add(controlPanel);

            // 创建数独网格
            var gridPanel = new Panel
            {
                Location = new Point(10, 70),
                Size = new Size(GRID_SIZE * CELL_SIZE + 20, GRID_SIZE * CELL_SIZE + 20),
                BackColor = Color.Black
            };

            cells = new TextBox[GRID_SIZE, GRID_SIZE];

            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    var cell = new TextBox
                    {
                        Size = new Size(CELL_SIZE - 2, CELL_SIZE - 2),
                        Location = new Point(
                            col * CELL_SIZE + (col / 3) * 2 + 2,
                            row * CELL_SIZE + (row / 3) * 2 + 2
                        ),
                        Font = new Font("Arial", 14, FontStyle.Bold),
                        TextAlign = HorizontalAlignment.Center,
                        MaxLength = 1,
                        Tag = new Point(row, col)
                    };

                    cell.KeyPress += Cell_KeyPress;
                    cell.TextChanged += Cell_TextChanged;
                    cells[row, col] = cell;
                    gridPanel.Controls.Add(cell);

                    // 设置3x3方块的背景色
                    if ((row / 3 + col / 3) % 2 == 0)
                        cell.BackColor = Color.LightBlue;
                    else
                        cell.BackColor = Color.White;
                }
            }

            this.Controls.Add(gridPanel);
        }

        private void InitializeGame()
        {
            sudokuGame = new SudokuGame();
            ClearGrid();
        }

        private void NewGameButton_Click(object sender, EventArgs e)
        {
            int difficulty = difficultyCombo.SelectedIndex;
            sudokuGame.GenerateNewGame(difficulty);
            UpdateGrid();
            statusLabel.Text = "新游戏开始！";
            statusLabel.ForeColor = Color.Blue;
        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            if (sudokuGame.Solve())
            {
                UpdateGrid();
                statusLabel.Text = "已求解完成！";
                statusLabel.ForeColor = Color.Green;
            }
            else
            {
                statusLabel.Text = "无法求解此数独！";
                statusLabel.ForeColor = Color.Red;
            }
        }

        private void CheckButton_Click(object sender, EventArgs e)
        {
            GetCurrentInput();
            if (sudokuGame.IsComplete())
            {
                statusLabel.Text = "恭喜！数独完成正确！";
                statusLabel.ForeColor = Color.Green;
                MessageBox.Show("恭喜！你成功完成了数独！", "游戏完成", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (sudokuGame.IsValid())
            {
                statusLabel.Text = "目前填写正确，继续加油！";
                statusLabel.ForeColor = Color.Blue;
            }
            else
            {
                statusLabel.Text = "发现错误，请检查填写！";
                statusLabel.ForeColor = Color.Red;
            }
        }

        private void Cell_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入数字1-9和退格键
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
                return;
            }

            if (char.IsDigit(e.KeyChar) && (e.KeyChar < '1' || e.KeyChar > '9'))
            {
                e.Handled = true;
            }
        }

        private void Cell_TextChanged(object sender, EventArgs e)
        {
            var cell = (TextBox)sender;
            var pos = (Point)cell.Tag;
            
            if (cell.Text.Length > 0)
            {
                int value = int.Parse(cell.Text);
                sudokuGame.SetCell(pos.X, pos.Y, value);
            }
            else
            {
                sudokuGame.SetCell(pos.X, pos.Y, 0);
            }

            // 实时检查冲突
            CheckConflicts();
        }

        private void CheckConflicts()
        {
            GetCurrentInput();
            
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    var cell = cells[row, col];
                    if (!sudokuGame.IsFixedCell(row, col))
                    {
                        if (cell.Text.Length > 0 && !sudokuGame.IsValidMove(row, col, int.Parse(cell.Text)))
                        {
                            cell.ForeColor = Color.Red;
                        }
                        else
                        {
                            cell.ForeColor = Color.Black;
                        }
                    }
                }
            }
        }

        private void GetCurrentInput()
        {
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    var cell = cells[row, col];
                    if (cell.Text.Length > 0)
                    {
                        sudokuGame.SetCell(row, col, int.Parse(cell.Text));
                    }
                    else
                    {
                        sudokuGame.SetCell(row, col, 0);
                    }
                }
            }
        }

        private void UpdateGrid()
        {
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    var cell = cells[row, col];
                    int value = sudokuGame.GetCell(row, col);
                    
                    if (value != 0)
                    {
                        cell.Text = value.ToString();
                        if (sudokuGame.IsFixedCell(row, col))
                        {
                            cell.ReadOnly = true;
                            cell.ForeColor = Color.Black;
                            cell.Font = new Font("Arial", 14, FontStyle.Bold);
                        }
                        else
                        {
                            cell.ReadOnly = false;
                            cell.ForeColor = Color.Blue;
                            cell.Font = new Font("Arial", 14, FontStyle.Regular);
                        }
                    }
                    else
                    {
                        cell.Text = "";
                        cell.ReadOnly = false;
                        cell.ForeColor = Color.Black;
                        cell.Font = new Font("Arial", 14, FontStyle.Regular);
                    }
                }
            }
        }

        private void ClearGrid()
        {
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    cells[row, col].Text = "";
                    cells[row, col].ReadOnly = false;
                    cells[row, col].ForeColor = Color.Black;
                }
            }
        }
    }
}