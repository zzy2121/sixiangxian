using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class BombermanForm : Form
    {
        private const int CELL_SIZE = 32;
        private const int MAP_WIDTH = 15;
        private const int MAP_HEIGHT = 13;
        
        private BombermanGame game;
        private System.Windows.Forms.Timer gameTimer;
        private Panel gamePanel;
        private Label player1StatusLabel;
        private Label player2StatusLabel;
        private Label timeLabel;
        private Label scoreLabel;
        
        private Dictionary<Keys, bool> pressedKeys;

        public BombermanForm()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.Text = "炸弹超人 - Bomberman";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(MAP_WIDTH * CELL_SIZE + 40, MAP_HEIGHT * CELL_SIZE + 150);
            this.KeyPreview = true;
            
            // 启用双缓冲以减少闪烁
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer, true);

            // 创建菜单栏
            var menuStrip = new MenuStrip();
            var gameMenu = new ToolStripMenuItem("游戏");
            
            var singlePlayerItem = new ToolStripMenuItem("单人模式");
            singlePlayerItem.Click += (s, e) => StartSinglePlayer();
            
            var twoPlayerItem = new ToolStripMenuItem("双人对战");
            twoPlayerItem.Click += (s, e) => StartTwoPlayer();
            
            var resetItem = new ToolStripMenuItem("重新开始");
            resetItem.Click += (s, e) => ResetGame();

            gameMenu.DropDownItems.AddRange(new ToolStripItem[] {
                singlePlayerItem, twoPlayerItem, new ToolStripSeparator(), resetItem
            });
            
            // 添加返回游戏中心菜单
            var backMenuItem = new ToolStripMenuItem("返回游戏中心");
            backMenuItem.Click += (s, e) => {
                var gameCenterForm = new GameCenterForm();
                gameCenterForm.Show();
                this.Close();
            };
            
            menuStrip.Items.AddRange(new ToolStripItem[] { gameMenu, backMenuItem });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 创建状态面板
            var statusPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.LightGray
            };

            player1StatusLabel = new Label
            {
                Text = "玩家1: ❤️❤️❤️ 💣×1 🔥×2",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            player2StatusLabel = new Label
            {
                Text = "玩家2: ❤️❤️❤️ 💣×1 🔥×2",
                Location = new Point(220, 10),
                Size = new Size(200, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            timeLabel = new Label
            {
                Text = "时间: 03:00",
                Location = new Point(10, 35),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            scoreLabel = new Label
            {
                Text = "得分: P1=0 P2=0",
                Location = new Point(220, 35),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9, FontStyle.Bold)
            };

            statusPanel.Controls.AddRange(new Control[] { 
                player1StatusLabel, player2StatusLabel, timeLabel, scoreLabel 
            });
            this.Controls.Add(statusPanel);

            // 创建游戏面板
            gamePanel = new Panel
            {
                Location = new Point(10, 90),
                Size = new Size(MAP_WIDTH * CELL_SIZE, MAP_HEIGHT * CELL_SIZE),
                BackColor = Color.Green,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // 为游戏面板启用双缓冲
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, gamePanel, new object[] { true });
                
            gamePanel.Paint += GamePanel_Paint;
            this.Controls.Add(gamePanel);

            // 初始化按键状态
            pressedKeys = new Dictionary<Keys, bool>();

            // 键盘事件
            this.KeyDown += BombermanForm_KeyDown;
            this.KeyUp += BombermanForm_KeyUp;

            // 游戏计时器
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 100; // 降低到10 FPS减少闪烁
            gameTimer.Tick += GameTimer_Tick;
        }

        private void InitializeGame()
        {
            game = new BombermanGame(MAP_WIDTH, MAP_HEIGHT);
            StartSinglePlayer();
        }

        private void StartSinglePlayer()
        {
            game.StartGame(BombermanGame.GameMode.SinglePlayer);
            gameTimer.Start();
            this.Focus();
        }

        private void StartTwoPlayer()
        {
            game.StartGame(BombermanGame.GameMode.TwoPlayer);
            gameTimer.Start();
            this.Focus();
        }

        private void ResetGame()
        {
            game.ResetGame();
            gameTimer.Start();
            this.Focus();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // 处理按键输入
            ProcessInput();
            
            // 更新游戏逻辑
            game.Update();
            
            // 更新UI
            UpdateUI();
            
            // 只重绘游戏面板的特定区域，而不是整个面板
            gamePanel.Invalidate();
            
            // 检查游戏结束
            if (game.IsGameOver())
            {
                gameTimer.Stop();
                ShowGameOverMessage();
            }
        }

        private void ProcessInput()
        {
            // 玩家1控制 (WASD + Space)
            if (IsKeyPressed(Keys.W)) game.MovePlayer(0, BombermanGame.Direction.Up);
            if (IsKeyPressed(Keys.S)) game.MovePlayer(0, BombermanGame.Direction.Down);
            if (IsKeyPressed(Keys.A)) game.MovePlayer(0, BombermanGame.Direction.Left);
            if (IsKeyPressed(Keys.D)) game.MovePlayer(0, BombermanGame.Direction.Right);
            if (IsKeyPressed(Keys.Space)) game.PlaceBomb(0);

            // 玩家2控制 (方向键 + Enter)
            if (game.GetGameMode() == BombermanGame.GameMode.TwoPlayer)
            {
                if (IsKeyPressed(Keys.Up)) game.MovePlayer(1, BombermanGame.Direction.Up);
                if (IsKeyPressed(Keys.Down)) game.MovePlayer(1, BombermanGame.Direction.Down);
                if (IsKeyPressed(Keys.Left)) game.MovePlayer(1, BombermanGame.Direction.Left);
                if (IsKeyPressed(Keys.Right)) game.MovePlayer(1, BombermanGame.Direction.Right);
                if (IsKeyPressed(Keys.Enter)) game.PlaceBomb(1);
            }
        }

        private bool IsKeyPressed(Keys key)
        {
            return pressedKeys.ContainsKey(key) && pressedKeys[key];
        }

        private void BombermanForm_KeyDown(object sender, KeyEventArgs e)
        {
            pressedKeys[e.KeyCode] = true;
        }

        private void BombermanForm_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys[e.KeyCode] = false;
        }

        private void UpdateUI()
        {
            var player1 = game.GetPlayer(0);
            var player2 = game.GetPlayer(1);
            
            if (player1 != null)
            {
                string hearts = new string('❤', player1.Lives);
                player1StatusLabel.Text = $"玩家1: {hearts} 💣×{player1.MaxBombs} 🔥×{player1.BombRange}";
            }

            if (player2 != null && game.GetGameMode() == BombermanGame.GameMode.TwoPlayer)
            {
                string hearts = new string('❤', player2.Lives);
                player2StatusLabel.Text = $"玩家2: {hearts} 💣×{player2.MaxBombs} 🔥×{player2.BombRange}";
            }
            else
            {
                player2StatusLabel.Text = "AI敌人数量: " + game.GetEnemyCount();
            }

            timeLabel.Text = $"时间: {game.GetGameTime():mm\\:ss}";
            scoreLabel.Text = $"得分: P1={game.GetScore(0)} P2={game.GetScore(1)}";
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            var map = game.GetMap();
            
            // 绘制地图背景渐变
            using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, MAP_WIDTH * CELL_SIZE, MAP_HEIGHT * CELL_SIZE),
                Color.FromArgb(144, 238, 144), Color.FromArgb(34, 139, 34), 45f))
            {
                g.FillRectangle(bgBrush, 0, 0, MAP_WIDTH * CELL_SIZE, MAP_HEIGHT * CELL_SIZE);
            }
            
            // 绘制网格线
            using (var gridPen = new Pen(Color.FromArgb(50, 0, 100, 0), 1))
            {
                for (int x = 0; x <= MAP_WIDTH; x++)
                {
                    g.DrawLine(gridPen, x * CELL_SIZE, 0, x * CELL_SIZE, MAP_HEIGHT * CELL_SIZE);
                }
                for (int y = 0; y <= MAP_HEIGHT; y++)
                {
                    g.DrawLine(gridPen, 0, y * CELL_SIZE, MAP_WIDTH * CELL_SIZE, y * CELL_SIZE);
                }
            }
            
            // 绘制地图元素
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    var rect = new Rectangle(x * CELL_SIZE + 1, y * CELL_SIZE + 1, CELL_SIZE - 2, CELL_SIZE - 2);
                    var cell = map[x, y];
                    
                    // 绘制地图元素
                    switch (cell.Type)
                    {
                        case BombermanGame.CellType.Wall:
                            DrawWall(g, rect);
                            break;
                        case BombermanGame.CellType.Box:
                            DrawBox(g, rect);
                            break;
                        case BombermanGame.CellType.PowerUp:
                            DrawPowerUp(g, rect, cell.PowerUpType);
                            break;
                    }
                    
                    // 绘制炸弹
                    if (cell.HasBomb)
                    {
                        DrawBomb(g, rect);
                    }
                    
                    // 绘制爆炸
                    if (cell.HasExplosion)
                    {
                        DrawExplosion(g, rect);
                    }
                }
            }
            
            // 绘制玩家
            var players = game.GetPlayers();
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player.IsAlive)
                {
                    var playerRect = new Rectangle(
                        (int)(player.X * CELL_SIZE) + 2, 
                        (int)(player.Y * CELL_SIZE) + 2, 
                        CELL_SIZE - 4, CELL_SIZE - 4
                    );
                    
                    DrawPlayer(g, playerRect, i, player.HasShield);
                }
            }
            
            // 绘制敌人
            var enemies = game.GetEnemies();
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive)
                {
                    var enemyRect = new Rectangle(
                        (int)(enemy.X * CELL_SIZE) + 2, 
                        (int)(enemy.Y * CELL_SIZE) + 2, 
                        CELL_SIZE - 4, CELL_SIZE - 4
                    );
                    
                    DrawEnemy(g, enemyRect);
                }
            }
        }
        
        private void DrawText(Graphics g, string text, Font font, Brush brush, Rectangle rect)
        {
            var textSize = g.MeasureString(text, font);
            var x = rect.X + (rect.Width - textSize.Width) / 2;
            var y = rect.Y + (rect.Height - textSize.Height) / 2;
            g.DrawString(text, font, brush, x, y);
        }

        private string GetPowerUpSymbol(BombermanGame.PowerUpType type)
        {
            return type switch
            {
                BombermanGame.PowerUpType.BombUp => "B",
                BombermanGame.PowerUpType.FireUp => "F",
                BombermanGame.PowerUpType.SpeedUp => "S",
                BombermanGame.PowerUpType.Shield => "H",
                _ => "?"
            };
        }

        private void DrawWall(Graphics g, Rectangle rect)
        {
            // 绘制3D效果的墙壁
            using (var wallBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                rect, Color.FromArgb(105, 105, 105), Color.FromArgb(47, 79, 79), 45f))
            {
                g.FillRectangle(wallBrush, rect);
            }
            
            // 添加边框
            using (var borderPen = new Pen(Color.FromArgb(25, 25, 25), 2))
            {
                g.DrawRectangle(borderPen, rect);
            }
            
            // 添加高光效果
            using (var highlightPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
            {
                g.DrawLine(highlightPen, rect.Left + 2, rect.Top + 2, rect.Right - 2, rect.Top + 2);
                g.DrawLine(highlightPen, rect.Left + 2, rect.Top + 2, rect.Left + 2, rect.Bottom - 2);
            }
        }

        private void DrawBox(Graphics g, Rectangle rect)
        {
            // 绘制木箱纹理
            using (var boxBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                rect, Color.FromArgb(222, 184, 135), Color.FromArgb(160, 82, 45), 45f))
            {
                g.FillRectangle(boxBrush, rect);
            }
            
            // 绘制木纹效果
            using (var woodPen = new Pen(Color.FromArgb(139, 69, 19), 1))
            {
                for (int i = 0; i < 3; i++)
                {
                    int y = rect.Top + rect.Height / 4 * (i + 1);
                    g.DrawLine(woodPen, rect.Left + 2, y, rect.Right - 2, y);
                }
            }
            
            // 边框
            using (var borderPen = new Pen(Color.FromArgb(101, 67, 33), 2))
            {
                g.DrawRectangle(borderPen, rect);
            }
        }

        private void DrawPowerUp(Graphics g, Rectangle rect, BombermanGame.PowerUpType type)
        {
            // 绘制发光背景
            using (var glowBrush = new SolidBrush(Color.FromArgb(100, 255, 255, 0)))
            {
                g.FillEllipse(glowBrush, rect);
            }
            
            // 绘制道具图标
            var iconRect = new Rectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);
            Color iconColor = type switch
            {
                BombermanGame.PowerUpType.BombUp => Color.Red,
                BombermanGame.PowerUpType.FireUp => Color.Orange,
                BombermanGame.PowerUpType.SpeedUp => Color.Blue,
                BombermanGame.PowerUpType.Shield => Color.Silver,
                _ => Color.Gold
            };
            
            using (var iconBrush = new SolidBrush(iconColor))
            {
                g.FillEllipse(iconBrush, iconRect);
            }
            
            // 绘制符号
            string symbol = GetPowerUpSymbol(type);
            using (var font = new Font("Arial", 12, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                var textSize = g.MeasureString(symbol, font);
                var textPos = new PointF(
                    rect.X + (rect.Width - textSize.Width) / 2,
                    rect.Y + (rect.Height - textSize.Height) / 2
                );
                g.DrawString(symbol, font, textBrush, textPos);
            }
        }

        private void DrawBomb(Graphics g, Rectangle rect)
        {
            // 绘制炸弹主体
            var bombRect = new Rectangle(rect.X + 3, rect.Y + 6, rect.Width - 6, rect.Height - 9);
            using (var bombBrush = new SolidBrush(Color.Black))
            {
                g.FillEllipse(bombBrush, bombRect);
            }
            
            // 绘制引线
            using (var fusePen = new Pen(Color.FromArgb(139, 69, 19), 3))
            {
                g.DrawLine(fusePen, 
                    rect.X + rect.Width/2, rect.Y + 3,
                    rect.X + rect.Width/2 + 5, rect.Y - 2);
            }
            
            // 绘制火花效果
            using (var sparkBrush = new SolidBrush(Color.FromArgb(255, 165, 0)))
            {
                g.FillEllipse(sparkBrush, rect.X + rect.Width/2 + 3, rect.Y - 3, 4, 4);
            }
            
            // 高光效果
            using (var highlightBrush = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
            {
                g.FillEllipse(highlightBrush, bombRect.X + 3, bombRect.Y + 3, 6, 6);
            }
        }

        private void DrawExplosion(Graphics g, Rectangle rect)
        {
            // 绘制爆炸动画效果
            var random = new Random(rect.X + rect.Y); // 固定种子保证一致性
            
            // 多层爆炸效果
            for (int i = 0; i < 3; i++)
            {
                var explosionRect = new Rectangle(
                    rect.X + random.Next(-2, 3), 
                    rect.Y + random.Next(-2, 3), 
                    rect.Width + random.Next(-4, 5), 
                    rect.Height + random.Next(-4, 5)
                );
                
                Color explosionColor = i switch
                {
                    0 => Color.FromArgb(150, 255, 255, 0), // 黄色
                    1 => Color.FromArgb(150, 255, 165, 0), // 橙色
                    _ => Color.FromArgb(150, 255, 69, 0)   // 红橙色
                };
                
                using (var explosionBrush = new SolidBrush(explosionColor))
                {
                    g.FillEllipse(explosionBrush, explosionRect);
                }
            }
            
            // 中心白色高光
            var centerRect = new Rectangle(rect.X + 6, rect.Y + 6, rect.Width - 12, rect.Height - 12);
            using (var centerBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.FillEllipse(centerBrush, centerRect);
            }
        }

        private void DrawPlayer(Graphics g, Rectangle rect, int playerId, bool hasShield)
        {
            // 绘制护盾效果
            if (hasShield)
            {
                var shieldRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
                using (var shieldBrush = new SolidBrush(Color.FromArgb(100, 0, 191, 255)))
                {
                    g.FillEllipse(shieldBrush, shieldRect);
                }
            }
            
            // 绘制玩家主体
            Color playerColor = playerId == 0 ? Color.FromArgb(0, 123, 255) : Color.FromArgb(220, 53, 69);
            using (var playerBrush = new SolidBrush(playerColor))
            {
                g.FillEllipse(playerBrush, rect);
            }
            
            // 绘制玩家编号
            string playerText = (playerId + 1).ToString();
            using (var font = new Font("Arial", 14, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                var textSize = g.MeasureString(playerText, font);
                var textPos = new PointF(
                    rect.X + (rect.Width - textSize.Width) / 2,
                    rect.Y + (rect.Height - textSize.Height) / 2
                );
                g.DrawString(playerText, font, textBrush, textPos);
            }
            
            // 边框
            using (var borderPen = new Pen(Color.White, 2))
            {
                g.DrawEllipse(borderPen, rect);
            }
        }

        private void DrawEnemy(Graphics g, Rectangle rect)
        {
            // 绘制敌人主体 - 红色
            using (var enemyBrush = new SolidBrush(Color.FromArgb(255, 69, 0)))
            {
                g.FillEllipse(enemyBrush, rect);
            }
            
            // 绘制眼睛
            using (var eyeBrush = new SolidBrush(Color.Yellow))
            {
                g.FillEllipse(eyeBrush, rect.X + 6, rect.Y + 8, 4, 4);
                g.FillEllipse(eyeBrush, rect.X + rect.Width - 10, rect.Y + 8, 4, 4);
            }
            
            // 绘制嘴巴
            using (var mouthPen = new Pen(Color.White, 2))
            {
                g.DrawArc(mouthPen, rect.X + 8, rect.Y + 12, rect.Width - 16, 8, 0, 180);
            }
            
            // 边框
            using (var borderPen = new Pen(Color.FromArgb(139, 0, 0), 2))
            {
                g.DrawEllipse(borderPen, rect);
            }
        }

        private void ShowGameOverMessage()
        {
            string message;
            var winner = game.GetWinner();
            
            if (winner == -1)
            {
                message = "平局！";
            }
            else if (winner == 0)
            {
                message = "玩家1获胜！";
            }
            else if (winner == 1)
            {
                message = "玩家2获胜！";
            }
            else
            {
                message = "游戏结束！";
            }
            
            message += $"\n最终得分: P1={game.GetScore(0)} P2={game.GetScore(1)}";
            
            var result = MessageBox.Show(message + "\n\n是否重新开始？", "游戏结束", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                
            if (result == DialogResult.Yes)
            {
                ResetGame();
            }
        }
    }
}