using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace World_s_hardest_game
{
    #region Tiles
    abstract class Tile
    {
        internal int ImageIndex { get; set; }
    }

    class EndTile : Tile
    {
        public EndTile(int imageIndex)
        {
            ImageIndex = imageIndex;
        }
    }

    class CheckpointTile : Tile
    {
        public CheckpointTile(int imageIndex)
        {
            ImageIndex = imageIndex;
        }
    }

    class WhiteTile : Tile
    {
        public WhiteTile(int imageIndex)
        {
            ImageIndex = imageIndex;
        }
    }

    class PurpleTile : Tile
    {
        public PurpleTile(int imageIndex)
        {
            ImageIndex = imageIndex;
        }
    }

    class VoidTile : Tile
    {
        public VoidTile(int imageIndex)
        {
            ImageIndex = imageIndex;
        }
    }
    #endregion

    abstract class Ball
    {
        protected Game game;
        public float X { get; set; }
        public float Y { get; set; }

        public float Xc => X + game.ballSize / 2;
        public float Yc => Y + game.ballSize / 2;

        public abstract void Move();
        public abstract void PrintYourself(System.Windows.Forms.PaintEventArgs e);
    }

    internal struct Directions
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Directions(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class BouncingBall : Ball
    {
        const float speed = 7f;

        public int ImageIndex { get; }

        public Directions Direction { get; set; }

        public BouncingBall(float x, float y, int imageIndex, Game g, Directions dir)
        {
            X = x;
            Y = y;
            ImageIndex = imageIndex;
            game = g;
            Direction = dir;
        }

        private void CorrectWallCollision()
        {
            Tile tile;
            if (Direction.Y != 0)
            { }
            else if (Direction.X == 1)
            {
                tile = game.map[(int)Y / game.tileSize, (int)(X + game.ballSize) / game.tileSize];
                if (tile.GetType() == typeof(VoidTile))
                {
                    Direction = new Directions(-1, Direction.Y);
                    float dx = (X + game.ballSize) % game.tileSize;
                    X = X - 2 * dx;
                }
            }
            else
            {
                tile = game.map[(int)Y / game.tileSize, (int)(X) / game.tileSize];
                if (tile.GetType() == typeof(VoidTile))
                {
                    Direction = new Directions(1, Direction.Y);
                    float dx = game.tileSize - X % game.tileSize;
                    X = X + 2 * dx;
                }
            }
        }

        public override void Move()
        {
            X += speed * Direction.X;
            Y += speed * Direction.Y;
            CorrectWallCollision();
        }

        public override void PrintYourself(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.DrawImage(game.bmpTiles[ImageIndex], X, Y);
        }
    }

    class Square
    {
        public int ImageIndex { get; }
        const float speed = 4f;
        public float X { get; set; }
        public float Y { get; set; }
        internal bool Up { get; set; }
        internal bool Down { get; set; }
        internal bool Left { get; set; }
        internal bool Right { get; set; }
        internal bool Dead { get; set; } = false;
        internal int Elapsed { get; set; } = 0;
        internal int RoundTime { get; set; } = 0;

        private Game game;

        public float Xc => X + game.squareSize / 2;
        public float Yc => Y + game.squareSize / 2;

        public Square(float x, float y, int imageIndex, Game g)
        {
            X = x;
            Y = y;
            ImageIndex = imageIndex;
            game = g;
        }

        public void PrintYourself(System.Windows.Forms.PaintEventArgs e, bool champ = false)
        {
            if (champ)
                e.Graphics.DrawImage(game.bmpTiles[6], X, Y);
            else
                e.Graphics.DrawImage(game.bmpTiles[ImageIndex], X, Y);
        }

        /// <summary>
        /// Corrects that square cannot go into walls. Magic assures what direction the square came into the wall 
        /// (corner case of getting to wall with 2 movements and detection of the moreoverlaping one of them)
        /// </summary>
        private void CorrectWallCollision()
        {
            Tile tile;
            bool left = false;
            bool bot = false;
            bool top = false;
            bool right = false;
            tile = game.map[(int)Y / game.tileSize, (int)X / game.tileSize]; // left, top
            if (tile.GetType() == typeof(VoidTile))
            {
                if (!Up)
                    left = true;
                else if (!Left)
                    top = true;
                else
                {
                    if (game.map[(int)(Y + speed) / game.tileSize, (int)X / game.tileSize].GetType() != typeof(VoidTile))
                        top = true;
                    else if (game.map[(int)Y / game.tileSize, (int)(X + speed) / game.tileSize].GetType() != typeof(VoidTile))
                        left = true;
                }
            }
            tile = game.map[(int)(Y - 1 + game.squareSize) / game.tileSize, (int)X / game.tileSize]; // left, bot corner
            if (tile.GetType() == typeof(VoidTile))
            {
                if (!Down)
                    left = true;
                else if (!Left)
                    bot = true;
                else
                {
                    if (game.map[(int)(Y - 1 + game.squareSize - speed) / game.tileSize, (int)X / game.tileSize].GetType() != typeof(VoidTile))
                        bot = true;
                    else if (game.map[(int)(Y - 1 + game.squareSize) / game.tileSize, (int)(X + speed) / game.tileSize].GetType() != typeof(VoidTile))
                        left = true;
                }
            }
            tile = game.map[(int)Y / game.tileSize, (int)(X - 1 + game.squareSize) / game.tileSize]; // right, top corner
            if (tile.GetType() == typeof(VoidTile))
            {
                if (!Up)
                    right = true;
                else if (!Right)
                    top = true;
                else
                {
                    if (game.map[(int)(Y + speed) / game.tileSize, (int)(X - 1 + game.squareSize) / game.tileSize].GetType() != typeof(VoidTile))
                        top = true;
                    else if (game.map[(int)Y / game.tileSize, (int)(X + game.squareSize - 1 - speed) / game.tileSize].GetType() != typeof(VoidTile))
                        right = true;
                }
            }
            tile = game.map[(int)(Y - 1 + game.squareSize) / game.tileSize, (int)(X - 1 + game.squareSize) / game.tileSize]; // right, bot corner
            if (tile.GetType() == typeof(VoidTile))
            {
                if (!Down)
                    right = true;
                else if (!Right)
                    bot = true;
                else
                {
                    if (game.map[(int)(Y - 1 + game.squareSize - speed) / game.tileSize, (int)(X - 1 + game.squareSize) / game.tileSize].GetType() != typeof(VoidTile))
                        bot = true;
                    else if (game.map[(int)(Y - 1 + game.squareSize) / game.tileSize, (int)(X - 1 + game.squareSize - speed) / game.tileSize].GetType() != typeof(VoidTile))
                        right = true;
                }
            }

            if (top)
                Y = Y - Y % game.tileSize + game.tileSize;
            if (bot)
                Y = (Y + game.tileSize) - Y % game.tileSize - game.squareSize;
            if (left)
                X = X - X % game.tileSize + game.tileSize;
            if (right)
                X = (X + game.tileSize) - X % game.tileSize - game.squareSize;
        }

        public void FloatMove(float dx_, float dy_)
        {
            if (Math.Abs(dx_) > speed)
                ;
            if (Math.Abs(dy_) > speed)
                ;
            float dx = dx_ * 4;
            float dy = dy_ * 4;
            if (dx > 0)
                X += dx > speed ? speed : dx;
            else
                X += dx < speed ? -speed : dx;
            if (dy > 0)
                Y += dy > speed ? speed : dy;
            else
                Y += dy < speed ? -speed : dy;
            CorrectWallCollision();
        }

        public void Move()
        {
            if (Up)
                Y -= speed;
            if (Down)
                Y += speed;
            if (Left)
                X -= speed;
            if (Right)
                X += speed;
            CorrectWallCollision();
        }
    }

}
