using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class GameCenterForm : Form
    {
        private Button minesweeperButton;
        private Button sudokuButton;
        private Button bombermanButton;
        private Label titleLabel;
        private Label descriptionLabel;

        public GameCenterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "游戏中心 - Game Center";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255);

            // 创建标题
            titleLabel = new Label
            {
                Text = "🎮 游戏中心",
                Font = new Font("Microsoft YaHei", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 112),
                Size = new Size(400, 50),
                Location = new Point(100, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 创建描述
            descriptionLabel = new Label
            {
                Text = "选择你想要玩的游戏",
                Font = new Font("Microsoft YaHei", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 70, 70),
                Size = new Size(300, 30),
                Location = new Point(150, 90),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 创建扫雷游戏按钮
            minesweeperButton = CreateGameButton(
                "💣 扫雷游戏",
                "经典的扫雷游戏\n三种难度等你挑战",
                new Point(80, 150),
                Color.FromArgb(220, 20, 60)
            );
            minesweeperButton.Click += MinesweeperButton_Click;

            // 创建数独游戏按钮
            sudokuButton = CreateGameButton(
                "🔢 数独游戏",
                "益智数独游戏\n锻炼你的逻辑思维",
                new Point(280, 150),
                Color.FromArgb(30, 144, 255)
            );
            sudokuButton.Click += SudokuButton_Click;

            // 创建炸弹超人游戏按钮
            bombermanButton = CreateGameButton(
                "💥 炸弹超人",
                "经典动作游戏\n单人或双人对战",
                new Point(180, 280),
                Color.FromArgb(255, 140, 0)
            );
            bombermanButton.Click += BombermanButton_Click;

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] {
                titleLabel, descriptionLabel,
                minesweeperButton, sudokuButton, bombermanButton
            });

            // 添加退出按钮
            var exitButton = new Button
            {
                Text = "退出",
                Size = new Size(80, 35),
                Location = new Point(500, 420),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.Black,
                Font = new Font("Microsoft YaHei", 10),
                FlatStyle = FlatStyle.Flat
            };
            exitButton.FlatAppearance.BorderSize = 1;
            exitButton.FlatAppearance.BorderColor = Color.Gray;
            exitButton.Click += (s, e) => this.Close();
            this.Controls.Add(exitButton);
        }

        private Button CreateGameButton(string title, string description, Point location, Color color)
        {
            var button = new Button
            {
                Size = new Size(150, 100),
                Location = location,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, color.R + 30),
                Math.Min(255, color.G + 30),
                Math.Min(255, color.B + 30)
            );

            // 创建自定义绘制
            button.Paint += (sender, e) =>
            {
                var btn = sender as Button;
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 绘制圆角矩形背景
                var rect = new Rectangle(0, 0, btn.Width, btn.Height);
                using (var brush = new SolidBrush(btn.BackColor))
                {
                    g.FillRoundedRectangle(brush, rect, 15);
                }

                // 绘制标题
                var titleLines = title.Split('\n');
                var titleFont = new Font("Microsoft YaHei", 12, FontStyle.Bold);
                var titleBrush = new SolidBrush(Color.White);
                
                var titleSize = g.MeasureString(titleLines[0], titleFont);
                var titleY = 15;
                g.DrawString(titleLines[0], titleFont, titleBrush, 
                    (btn.Width - titleSize.Width) / 2, titleY);

                // 绘制描述
                var descLines = description.Split('\n');
                var descFont = new Font("Microsoft YaHei", 8);
                var descBrush = new SolidBrush(Color.FromArgb(230, 230, 230));
                
                var descY = titleY + 35;
                foreach (var line in descLines)
                {
                    var descSize = g.MeasureString(line, descFont);
                    g.DrawString(line, descFont, descBrush, 
                        (btn.Width - descSize.Width) / 2, descY);
                    descY += 15;
                }

                titleFont.Dispose();
                titleBrush.Dispose();
                descFont.Dispose();
                descBrush.Dispose();
            };

            return button;
        }

        private void MinesweeperButton_Click(object sender, EventArgs e)
        {
            var minesweeperForm = new MainForm();
            minesweeperForm.Show();
        }

        private void SudokuButton_Click(object sender, EventArgs e)
        {
            var sudokuForm = new SudokuForm();
            sudokuForm.Show();
        }

        private void BombermanButton_Click(object sender, EventArgs e)
        {
            var bombermanForm = new BombermanForm();
            bombermanForm.Show();
        }
    }

    // 扩展方法用于绘制圆角矩形
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}