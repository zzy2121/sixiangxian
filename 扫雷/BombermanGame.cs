using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper
{
    public class BombermanGame
    {
        public enum GameMode { SinglePlayer, TwoPlayer }
        public enum Direction { Up, Down, Left, Right }
        public enum CellType { Empty, Wall, Box, PowerUp }
        public enum PowerUpType { BombUp, FireUp, SpeedUp, Shield }

        public class Cell
        {
            public CellType Type { get; set; }
            public bool HasBomb { get; set; }
            public bool HasExplosion { get; set; }
            public PowerUpType PowerUpType { get; set; }
        }

        public class Player
        {
            public float X { get; set; }
            public float Y { get; set; }
            public int Lives { get; set; } = 3;
            public int MaxBombs { get; set; } = 1;
            public int BombRange { get; set; } = 2;
            public float Speed { get; set; } = 0.1f;
            public bool IsAlive { get; set; } = true;
            public int Score { get; set; } = 0;
            public int CurrentBombs { get; set; } = 0;
            public bool HasShield { get; set; } = false;
            public int ShieldTime { get; set; } = 0;
        }

        public class Enemy
        {
            public float X { get; set; }
            public float Y { get; set; }
            public bool IsAlive { get; set; } = true;
            public Direction LastDirection { get; set; } = Direction.Right;
            public int MoveTimer { get; set; } = 0;
        }

        public class Bomb
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Timer { get; set; } = 60; // 3秒 (60 * 50ms)
            public int Range { get; set; }
            public int PlayerId { get; set; }
        }

        private Cell[,] map;
        private List<Player> players;
        private List<Enemy> enemies;
        private List<Bomb> bombs;
        private List<(int x, int y, int timer)> explosions;
        private GameMode currentMode;
        private Random random;
        private DateTime gameStartTime;
        private int mapWidth, mapHeight;

        public BombermanGame(int width, int height)
        {
            mapWidth = width;
            mapHeight = height;
            random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            map = new Cell[mapWidth, mapHeight];
            players = new List<Player>();
            enemies = new List<Enemy>();
            bombs = new List<Bomb>();
            explosions = new List<(int, int, int)>();
            
            GenerateMap();
        }

        private void GenerateMap()
        {
            // 初始化空地图
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    map[x, y] = new Cell { Type = CellType.Empty };
                }
            }

            // 生成边界墙
            for (int x = 0; x < mapWidth; x++)
            {
                map[x, 0].Type = CellType.Wall;
                map[x, mapHeight - 1].Type = CellType.Wall;
            }
            for (int y = 0; y < mapHeight; y++)
            {
                map[0, y].Type = CellType.Wall;
                map[mapWidth - 1, y].Type = CellType.Wall;
            }

            // 生成内部固定墙（每隔一格）
            for (int x = 2; x < mapWidth - 1; x += 2)
            {
                for (int y = 2; y < mapHeight - 1; y += 2)
                {
                    map[x, y].Type = CellType.Wall;
                }
            }

            // 生成随机箱子
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    if (map[x, y].Type == CellType.Empty)
                    {
                        // 避免在玩家起始位置放箱子
                        if ((x <= 2 && y <= 2) || (x >= mapWidth - 3 && y <= 2) ||
                            (x <= 2 && y >= mapHeight - 3) || (x >= mapWidth - 3 && y >= mapHeight - 3))
                            continue;

                        if (random.NextDouble() < 0.6) // 60%概率放置箱子
                        {
                            map[x, y].Type = CellType.Box;
                        }
                    }
                }
            }
        }

        public void StartGame(GameMode mode)
        {
            currentMode = mode;
            gameStartTime = DateTime.Now;
            
            players.Clear();
            enemies.Clear();
            bombs.Clear();
            explosions.Clear();

            // 创建玩家
            players.Add(new Player { X = 1, Y = 1 });
            
            if (mode == GameMode.TwoPlayer)
            {
                players.Add(new Player { X = mapWidth - 2, Y = mapHeight - 2 });
            }
            else
            {
                // 单人模式：创建AI敌人
                CreateEnemies();
            }
        }

        private void CreateEnemies()
        {
            var enemyPositions = new[]
            {
                (mapWidth - 2, 1),
                (1, mapHeight - 2),
                (mapWidth - 2, mapHeight - 2)
            };

            foreach (var (x, y) in enemyPositions)
            {
                enemies.Add(new Enemy { X = x, Y = y });
            }
        }

        public void Update()
        {
            UpdateBombs();
            UpdateExplosions();
            UpdateEnemies();
            UpdatePlayers();
        }

        private void UpdateBombs()
        {
            for (int i = bombs.Count - 1; i >= 0; i--)
            {
                var bomb = bombs[i];
                bomb.Timer--;
                
                if (bomb.Timer <= 0)
                {
                    ExplodeBomb(bomb);
                    bombs.RemoveAt(i);
                }
            }
        }

        private void ExplodeBomb(Bomb bomb)
        {
            map[bomb.X, bomb.Y].HasBomb = false;
            
            // 减少玩家当前炸弹数
            if (bomb.PlayerId < players.Count)
            {
                players[bomb.PlayerId].CurrentBombs--;
            }

            // 创建爆炸
            CreateExplosion(bomb.X, bomb.Y, bomb.Range);
        }

        private void CreateExplosion(int centerX, int centerY, int range)
        {
            // 中心爆炸
            AddExplosion(centerX, centerY);
            
            // 四个方向的爆炸
            var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
            
            foreach (var (dx, dy) in directions)
            {
                for (int i = 1; i <= range; i++)
                {
                    int x = centerX + dx * i;
                    int y = centerY + dy * i;
                    
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        break;
                        
                    if (map[x, y].Type == CellType.Wall)
                        break;
                        
                    AddExplosion(x, y);
                    
                    if (map[x, y].Type == CellType.Box)
                    {
                        DestroyBox(x, y);
                        break;
                    }
                }
            }
        }

        private void AddExplosion(int x, int y)
        {
            map[x, y].HasExplosion = true;
            explosions.Add((x, y, 20)); // 爆炸持续20帧
            
            // 检查是否有其他炸弹被引爆
            var chainBomb = bombs.FirstOrDefault(b => b.X == x && b.Y == y);
            if (chainBomb != null)
            {
                chainBomb.Timer = 1; // 立即爆炸
            }
            
            // 检查玩家伤害
            CheckPlayerDamage(x, y);
            
            // 检查敌人伤害
            CheckEnemyDamage(x, y);
        }

        private void DestroyBox(int x, int y)
        {
            map[x, y].Type = CellType.Empty;
            
            // 随机生成道具
            if (random.NextDouble() < 0.3) // 30%概率
            {
                map[x, y].Type = CellType.PowerUp;
                map[x, y].PowerUpType = (PowerUpType)random.Next(4);
            }
            
            // 给破坏箱子的玩家加分
            // 这里简化处理，给最近的玩家加分
            foreach (var player in players)
            {
                player.Score += 10;
            }
        }

        private void CheckPlayerDamage(int x, int y)
        {
            foreach (var player in players)
            {
                if (player.IsAlive && Math.Abs(player.X - x) < 0.8f && Math.Abs(player.Y - y) < 0.8f)
                {
                    if (player.HasShield && player.ShieldTime > 0)
                    {
                        player.ShieldTime = 0;
                        player.HasShield = false;
                    }
                    else
                    {
                        player.Lives--;
                        if (player.Lives <= 0)
                        {
                            player.IsAlive = false;
                        }
                    }
                }
            }
        }

        private void CheckEnemyDamage(int x, int y)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive && Math.Abs(enemy.X - x) < 0.8f && Math.Abs(enemy.Y - y) < 0.8f)
                {
                    enemy.IsAlive = false;
                    
                    // 给玩家加分
                    foreach (var player in players)
                    {
                        player.Score += 100;
                    }
                }
            }
        }

        private void UpdateExplosions()
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                var explosion = explosions[i];
                explosion.timer--;
                
                if (explosion.timer <= 0)
                {
                    map[explosion.x, explosion.y].HasExplosion = false;
                    explosions.RemoveAt(i);
                }
                else
                {
                    explosions[i] = explosion;
                }
            }
        }

        private void UpdateEnemies()
        {
            foreach (var enemy in enemies.Where(e => e.IsAlive))
            {
                enemy.MoveTimer++;
                
                if (enemy.MoveTimer >= 20) // 每20帧移动一次
                {
                    enemy.MoveTimer = 0;
                    MoveEnemyAI(enemy);
                }
            }
        }

        private void MoveEnemyAI(Enemy enemy)
        {
            var directions = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            var validDirections = new List<Direction>();
            
            foreach (var dir in directions)
            {
                var (newX, newY) = GetNewPosition(enemy.X, enemy.Y, dir, 0.5f);
                if (CanMoveTo((int)newX, (int)newY))
                {
                    validDirections.Add(dir);
                }
            }
            
            if (validDirections.Count > 0)
            {
                var direction = validDirections[random.Next(validDirections.Count)];
                var (newX, newY) = GetNewPosition(enemy.X, enemy.Y, direction, 0.5f);
                enemy.X = newX;
                enemy.Y = newY;
                enemy.LastDirection = direction;
            }
        }

        private void UpdatePlayers()
        {
            foreach (var player in players)
            {
                if (player.ShieldTime > 0)
                {
                    player.ShieldTime--;
                    if (player.ShieldTime <= 0)
                    {
                        player.HasShield = false;
                    }
                }
                
                // 检查道具收集
                CheckPowerUpCollection(player);
            }
        }

        private void CheckPowerUpCollection(Player player)
        {
            int x = (int)Math.Round(player.X);
            int y = (int)Math.Round(player.Y);
            
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight && 
                map[x, y].Type == CellType.PowerUp)
            {
                ApplyPowerUp(player, map[x, y].PowerUpType);
                map[x, y].Type = CellType.Empty;
            }
        }

        private void ApplyPowerUp(Player player, PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.BombUp:
                    player.MaxBombs++;
                    break;
                case PowerUpType.FireUp:
                    player.BombRange++;
                    break;
                case PowerUpType.SpeedUp:
                    player.Speed = Math.Min(player.Speed + 0.02f, 0.2f);
                    break;
                case PowerUpType.Shield:
                    player.HasShield = true;
                    player.ShieldTime = 300; // 15秒护盾
                    break;
            }
            
            player.Score += 50;
        }

        public void MovePlayer(int playerId, Direction direction)
        {
            if (playerId >= players.Count || !players[playerId].IsAlive)
                return;
                
            var player = players[playerId];
            var (newX, newY) = GetNewPosition(player.X, player.Y, direction, player.Speed);
            
            if (CanMoveTo((int)Math.Round(newX), (int)Math.Round(newY)))
            {
                player.X = newX;
                player.Y = newY;
            }
        }

        private (float, float) GetNewPosition(float x, float y, Direction direction, float speed)
        {
            return direction switch
            {
                Direction.Up => (x, y - speed),
                Direction.Down => (x, y + speed),
                Direction.Left => (x - speed, y),
                Direction.Right => (x + speed, y),
                _ => (x, y)
            };
        }

        private bool CanMoveTo(int x, int y)
        {
            if (x < 1 || x >= mapWidth - 1 || y < 1 || y >= mapHeight - 1)
                return false;
                
            return map[x, y].Type == CellType.Empty || map[x, y].Type == CellType.PowerUp;
        }

        public void PlaceBomb(int playerId)
        {
            if (playerId >= players.Count || !players[playerId].IsAlive)
                return;
                
            var player = players[playerId];
            
            if (player.CurrentBombs >= player.MaxBombs)
                return;
                
            int x = (int)Math.Round(player.X);
            int y = (int)Math.Round(player.Y);
            
            if (map[x, y].HasBomb)
                return;
                
            map[x, y].HasBomb = true;
            player.CurrentBombs++;
            
            bombs.Add(new Bomb
            {
                X = x,
                Y = y,
                Range = player.BombRange,
                PlayerId = playerId
            });
        }

        public void ResetGame()
        {
            InitializeGame();
            StartGame(currentMode);
        }

        // Getter methods
        public Cell[,] GetMap() => map;
        public List<Player> GetPlayers() => players;
        public List<Enemy> GetEnemies() => enemies;
        public Player GetPlayer(int id) => id < players.Count ? players[id] : null;
        public GameMode GetGameMode() => currentMode;
        public int GetEnemyCount() => enemies.Count(e => e.IsAlive);
        public TimeSpan GetGameTime() => DateTime.Now - gameStartTime;
        public int GetScore(int playerId) => playerId < players.Count ? players[playerId].Score : 0;

        public bool IsGameOver()
        {
            if (currentMode == GameMode.SinglePlayer)
            {
                return !players[0].IsAlive || enemies.All(e => !e.IsAlive);
            }
            else
            {
                return players.Count(p => p.IsAlive) <= 1;
            }
        }

        public int GetWinner()
        {
            if (currentMode == GameMode.SinglePlayer)
            {
                if (players[0].IsAlive && enemies.All(e => !e.IsAlive))
                    return 0; // 玩家胜利
                else
                    return -2; // 玩家失败
            }
            else
            {
                var alivePlayers = players.Where(p => p.IsAlive).ToList();
                if (alivePlayers.Count == 1)
                {
                    return players.IndexOf(alivePlayers[0]);
                }
                return -1; // 平局或游戏未结束
            }
        }
    }
}