using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("WorldsHardestGame_UnitTests")]
namespace World_s_hardest_game
{

    enum GameState { Ingame, Victory, Loss}

    partial class Game
    {
        public const int COLUMNS = 24;
        public const int ROWS = 15;

        public GameState State { get; set; }

        internal Tile[,] map = new Tile[ROWS, COLUMNS];
        internal List<Bitmap> bmpTiles = new List<Bitmap>();
        internal List<Ball> balls = new List<Ball>();

        internal int tileSize;
        internal int squareSize;
        internal int ballSize;

        internal Square Player { get; set; }

        internal Game() { }

        public Game(string levelPath, string tilesBmpPath, string squarePath, string ballPath)
        {
           // ReadBmpTiles(tilesBmpPath);
            ReadLevel(levelPath);
            //ReadBmpSquare(squarePath);
            tileSize = AddBmp(new Bitmap(tilesBmpPath));
            squareSize = AddBmp(new Bitmap(squarePath));
            ballSize = AddBmp(new Bitmap(ballPath));

            Players = null;
            State = GameState.Ingame;
            Player = new Square(5 * tileSize, ROWS * tileSize / 2f, 4, this);
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize + tileSize * 0.25f, 5, this, new Directions(-1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize + tileSize * 1.25f, 5, this, new Directions(1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize + tileSize * 2.25f, 5, this, new Directions(-1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize - tileSize * 0.75f, 5, this, new Directions(1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize - tileSize * 1.75f, 5, this, new Directions(-1, 0)));
        }

        public void ReadBmpSquare(string path)
        {
            Bitmap bmp = new Bitmap(path);
        }

        private int AddBmp(Bitmap bmp)
        {
            int size = bmp.Height;

            int count = bmp.Width / size;
            for (int i = 0; i < count; i++)
            {
                Rectangle rect = new Rectangle(i * size, 0, size, size);
                bmpTiles.Add(bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare));
            }
            return size;
        }

        public void ReadBmpTiles(string path)
        {
            Bitmap bmp = new Bitmap(path);
            this.tileSize = bmp.Height;
            int count = bmp.Width / tileSize;
            for (int i = 0; i < count; i++)
            {
                Rectangle rect = new Rectangle(i * tileSize, 0, tileSize, tileSize);
                bmpTiles.Add(bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare));
            }
        }

        public void ReadLevel(string path)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(path);
                string line;
                for (int i = 0; i < ROWS; i++)
                {
                    line = sr.ReadLine();
                    for (int j = 0; j < COLUMNS; j++)
                    {
                        switch (line[j])
                        {
                            case 'X':
                                map[i, j] = new VoidTile(3);
                                break;
                            case 'C':
                                map[i, j] = new CheckpointTile(0);
                                break;
                            case 'W':
                                map[i, j] = new WhiteTile(2);
                                break;
                            case 'P':
                                map[i, j] = new PurpleTile(1);
                                break;
                            case 'E':
                                map[i, j] = new EndTile(0);
                                break;
                        }
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

        }

        internal struct Vector
        {
            public float x;
            public float y;

            public Vector(float xx, float yy)
            {
                x = xx;
                y = yy;
            }

            public static bool operator <=(Vector a, Vector b) => a.x <= b.x && a.y <= b.y;

            public static bool operator >=(Vector a, Vector b) => a.x >= b.x && a.y >= b.y;
        }

        internal Vector LineIntersection(Vector p1, Vector p2, Vector p3, Vector p4)
        {
            float a1 = p1.y - p2.y;
            float b1 = p2.x - p1.x;
            float c1 = a1 * p1.x + b1 * p1.y;
            float a2 = p3.y - p4.y;
            float b2 = p4.x - p3.x;
            float c2 = a2 * p3.x + b2 * p3.y;
            float denominator = a1 * b2 - a2 * b1;

            return new Vector((b2 * c1 - b1 * c2) / denominator, (a1 * c2 - a2 * c1) / denominator);

        }

        internal float Distance(Vector a, Vector b)
        {
            return (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }

        public bool BallPlayerCollision(Square player, Ball ball)
        {
            float radius = ballSize / 2;
            var p1 = new Vector(player.X, player.Y);
            var p2 = new Vector(player.X, player.Y + squareSize);
            var centre = new Vector(ball.Xc, ball.Yc);
            var i = LineIntersection(p1, p2, centre, new Vector(ball.Xc + radius, ball.Yc));
            if (((p1 <= i && i <= p2) || (p2 <= i && i <= p1)) && Distance(centre, i) <= radius)
                return true;

            p2 = new Vector(player.X + squareSize, player.Y);
            i = LineIntersection(p1, p2, centre, new Vector(ball.Xc, ball.Yc + radius));
            if (((p1 <= i && i <= p2) || (p2 <= i && i <= p1)) && Distance(centre, i) <= radius)
                return true;

            p1 = new Vector(player.X + squareSize, player.Y + squareSize);
            i = LineIntersection(p1, p2, centre, new Vector(ball.Xc - radius, ball.Yc));
            if (((p1 <= i && i <= p2) || (p2 <= i && i <= p1)) && Distance(centre, i) <= radius)
                return true;

            p2 = new Vector(player.X, player.Y + squareSize);
            i = LineIntersection(p1, p2, centre, new Vector(ball.Xc, ball.Yc - radius));
            if (((p1 <= i && i <= p2) || (p2 <= i && i <= p1)) && Distance(centre, i) <= radius)
                return true;

            return false;
        }

        public bool CheckForVictory(Square player)
        {
            Tile tile;
            tile = this.map[(int)player.Yc / tileSize, (int)(player.Xc) / tileSize];
            if (tile.GetType() == typeof(EndTile))
                return true;
            tile = this.map[(int)(player.Yc + squareSize / 2) / tileSize, (int)(player.Xc) / tileSize];
            if (tile.GetType() == typeof(EndTile))
                return true;
            tile = this.map[(int)(player.Yc - squareSize / 2) / tileSize, (int)(player.Xc) / tileSize];
            if (tile.GetType() == typeof(EndTile))
                return true;
            tile = this.map[(int)player.Yc / tileSize, (int)(player.Xc + squareSize / 2) / tileSize];
            if (tile.GetType() == typeof(EndTile))
                return true;
            tile = this.map[(int)player.Yc / tileSize, (int)(player.Xc - squareSize / 2) / tileSize];
            if (tile.GetType() == typeof(EndTile))
                return true;
            return false;
        }

        public void CheckBallPlayerCollision(Square player)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                if (BallPlayerCollision(player, balls[i]))
                { //5 * tileSize, ROWS * tileSize / 2f, 4
                   
                    player.Dead = true;
                    return;
                }
            }
        }

        public void PrintMap(System.Windows.Forms.PaintEventArgs e)
        {
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLUMNS; j++)
                {
                  
                    Tile t = map[i, j];
            
                    e.Graphics.DrawImage(bmpTiles[t.ImageIndex], j * tileSize, i * tileSize);
                }
            }

            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].PrintYourself(e);
            }

            Player.PrintYourself(e);

        }

        public void MoveElements(bool up, bool down, bool left, bool right)
        {
            if (Player.Dead)
            {
                Player.X = 5 * tileSize;
                Player.Y = ROWS * tileSize / 2f;
            }
            Player.Up = up;
            Player.Down = down;
            Player.Left = left;
            Player.Right = right;

            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Move();
            }
            Player.Move();
        }
    }
}
