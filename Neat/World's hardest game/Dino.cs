using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Neuroevolution.Neat;

namespace Dino
{
    class Player
    {
        Game game;
        public float Y { get; set; }
        public float X { get; }
        public bool BigJump { get; set; } = false;
        public bool SmallJump { get; set; } = false;
        public bool Duck { get; set; } = false;
        public int Score { get; set; }

        float velY = 0f;
        public float JumpPower { get; set; } = 18;
        int usualHeight;
        public int Height { get; set; }
        public int Width { get; set; }
        public int DuckHeight { get; set; }
        public int DuckWidth { get; set; }
        public bool Dead = false;
        int groundY;
        bool jumping = false;
        Bitmap image;
        Bitmap duckImage;
        bool ducking = false;

        public float usualGravity { get; set;  } = 1.50f;
        public float gravity { get; set; } = 1.50f;
        public float Xc { get => X + Width / 2; }
        public float Yc { get => Y = Height / 2; }

        public Player(float x, float y, Bitmap i, Bitmap d, Game g, int w, int h)
        {
            game = g;
            image = i;
            duckImage = d;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            usualHeight = Height;
            groundY = 400 - Height;
            Score = 0;
            DuckHeight = d.Height;
            DuckWidth = d.Width;
        }

        public void Reset()
        {
            Y = groundY;
            Dead = false;
            Score = 0;
            usualGravity = 1.50f;
            gravity = 1.50f;
            JumpPower = 17;
        }

        public void Move()
        {
            if (Dead)
                return;
            if (Duck)
            {
                Height = 75;
                if (velY > 0)
                    velY = 0;
                gravity += usualGravity;
                ducking = true;
            }
            else
                Height = usualHeight;
            jumping = true;
            Y -= velY;
            if (Y > groundY)
            {
                Y = groundY;
                velY = 0f;
                jumping = false;
                ducking = false;
            }
            else
            {
                velY -= gravity;
            }
            if (jumping == false && ducking == false)
            {
                Jump();
            }
            gravity = usualGravity;
        }


        private void Jump()
        {
            if (BigJump)
                velY = JumpPower;
            else if (SmallJump)
                velY = JumpPower - 2;
        }

        public void PrintYourself(System.Windows.Forms.PaintEventArgs e)
        {
            if (Dead == false)
            {
                if (Duck)
                    e.Graphics.DrawImage(duckImage, X, Y + image.Height - duckImage.Height);
                else
                    e.Graphics.DrawImage(image, X, Y);
            } 
        }
    }

    enum GameState { Ingame, Victory, Loss }

    abstract class Obstacle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public bool Alive { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Xc { get => X + Width / 2; }
        public float Yc { get => Y = Height / 2; }

        protected Bitmap image;
        protected Game game;
        //protected float velX = 0;

        public abstract bool Collision(Player player);
        public abstract void Move(float speed);
        public abstract void PrintYourself(System.Windows.Forms.PaintEventArgs e);
        
    }

    class Bird : Obstacle
    {
        public Bird(int width, int height, Bitmap i, Game g, float x, float y)
        {
            Width = width;
            Height = height;
            image = i;
            game = g;
            X = x;
            Y = y;
            Alive = true;

        }

        public override void Move(float velX)
        {
            if (X + Width - velX < 0)
                Alive = false;
            X -= velX;
        }

        public override bool Collision(Player player)
        {
            float playerWidth = player.Duck ? player.DuckWidth : player.Width;
            float playerHeight = player.Duck ? player.DuckHeight : player.Height;
            float playerY = player.Duck ? player.Y + player.Height - player.DuckHeight : player.Y;
            float playerRight = player.X + playerWidth - 2;
            float obstacleRight = X + Width;
            float playerDown = playerY + playerHeight;
            float obstacleDown = Y + Height;
            if (playerY < obstacleDown)
            {
                if ((player.X <= obstacleRight && playerRight >= X))
                {
                    if (player.Xc <= X)
                    {
                        if (playerDown - playerHeight / 2 >= Y)
                            return true;
                    }
                    else if (player.X + playerWidth / 5 <= obstacleRight)
                    {
                        if (playerDown >= Y)
                            return true;
                    }
                    else if (playerDown - playerHeight / 4 >= Y)
                        return true;
                }
            }

            return false;
        }

        public override void PrintYourself(System.Windows.Forms.PaintEventArgs e) => e.Graphics.DrawImage(image, X, Y);
    }

    class Cactus : Obstacle
    {
        public Cactus(int width, int height, Bitmap i, Game g, float x, float y)
        {
            Width = width;
            Height = height;
            image = i;
            game = g;
            X = x;
            Y = y;
            Alive = true;
            
        }
        public override bool Collision(Player player)
        {
            float playerWidth = player.Duck ? player.DuckWidth : player.Width;
            float playerHeight = player.Duck ? player.DuckHeight : player.Height;
            float playerRight = player.X + playerWidth;
            float obstacleRight = X + Width;
            float playerY = player.Duck ? player.Y + player.Height - player.DuckHeight : player.Y;
            float playerDown = playerY + playerHeight;
            float obstacleDown = Y + Height;
            if ((player.X <= obstacleRight && playerRight >= X))
            {
                if (player.Xc <= X)
                {
                    if (playerDown - playerHeight / 2 >= Y)
                        return true;
                }
                else if (player.X + playerWidth / 5 <= obstacleRight)
                {
                    if (playerDown >= Y)
                        return true;
                }
                else if (playerDown - playerHeight / 4 >= Y)
                    return true;
               
            }
            return false;
        }

        public override void Move(float velX)
        {
            if (X + Width - velX < 0)
                Alive = false;
            X -= velX;
        }

        public override void PrintYourself(System.Windows.Forms.PaintEventArgs e) => e.Graphics.DrawImage(image, X, Y);
    }

    class Game
    {
        public int TimeRun { get; set; } = 0;
        Player player { get; set; }
        List<Obstacle> obstacles = new List<Obstacle>();
        List<Bitmap> images = new List<Bitmap>();
        public List<object> players = new List<object>();
        float obstacleSpeed = 5;
        public int Width { get; set; }
        public int Height { get; set; }
        int run;
        Random rnd;
        bool earlyGame = true;
        public GameState State { get; set; }
        public Game(int w, int h)
        {
            rnd = new Random();
            run = (int)obstacleSpeed * 200;
            Width = w;
            Height = h;
            State = GameState.Ingame;
            LoadImages();
            player = new Player(200, 400 - images[0].Height , images[0], images[4], this, (int)(images[0].Width), (int)(images[0].Height));
           
        }

        public Game(int w, int h, int numberOfPlayers)
        {
            rnd = new Random();
            run = (int)obstacleSpeed * 200;
            Width = w;
            Height = h;
            State = GameState.Ingame;
            LoadImages();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players.Add(new Player(200, 400, images[0], images[4], this, images[0].Width, images[0].Height));
            }
            player = null;

        }

        private void SpeedUpAndScore(ref int elapsed, ref int elapsed2, Player p)
        {
            if (elapsed > 1000 && p.Dead == false)
            {
                p.Score++;
                elapsed = 0;
            }
        }

        private bool SpeedUpAndScoreSimulation(int elapsed, int elapsed2, Player p, float speedUp)
        {
            if (elapsed > 1000 && p.Dead == false)
            {
                p.Score++;
            }
            return false;
        }

        private void LoadImages()
        {
            LoadImage("player.png");
            LoadImage("cactus.png");
            LoadImage("cactusBig.png");
            LoadImage("bird.png");
            LoadImage("playerDuck.png");
        }

        private void LoadImage(string path, int width, int height)
        {
            Rectangle rect = new Rectangle(0, 0, width, height);
            Bitmap tmp = new Bitmap(path);
            var image = tmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
            images.Add(image);
        }

        private void LoadImage(string path)
        {
            Bitmap tmp = new Bitmap(path);
            Rectangle rect = new Rectangle(0, 0, tmp.Width, tmp.Height);
            
            var image = tmp.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);
            images.Add(image);
        }

        public void ManageObstacles()
        {
            obstacles.RemoveAll(o => o.Alive == false);
            if (run > obstacleSpeed * 80)
            {
                CreateObstacle();
                run = 0;
            }
        }

        public List<Node> CreateNodes()
        {
            List<Node> nodes = new List<Node>();
            for (int i = 0; i < 8; i++)
            {
                nodes.Add(new Node(NodeType.Sensor, 0, i));
            }
            for (int i = 0; i < 4; i++)
            {
                nodes.Add(new Node(NodeType.Output, 1, 8 + i));
            }
            return nodes;
        }

        public SortedList<int, Gene> CreateGenes(List<Node> nodes)
        {
            var genes = new SortedList<int, Gene>();
            genes.Add(0, new Gene(nodes[0], nodes[7], rnd.Next(), GeneType.Enabled, 0));
            return genes;
        }

        private void CreateObstacle()
        {
            if (earlyGame)
            {
                switch (rnd.Next(1, 4))
                {
                    case 1:
                        obstacles.Add(new Cactus((int)(images[1].Width), (int)(images[1].Height), images[1], this, Width - images[1].Width, 400 - images[1].Height));
                        break;
                    case 2:
                        obstacles.Add(new Cactus(images[2].Width, images[2].Height, images[2], this, Width - images[2].Width, 400 - images[2].Height));
                        break;
                    case 3:
                        obstacles.Add(new Bird(images[3].Width, images[3].Height, images[3], this, Width - images[3].Width, 400 - images[3].Height - (images[0].Height * 5 )/ 6));
                        break;
                    case 4:
                        obstacles.Add(new Bird(images[3].Width, images[3].Height, images[3], this, Width - images[3].Width, 400 - images[3].Height - (images[0].Height * 2)));
                        break;
                }
            }
            else
                switch (rnd.Next(1, 5))
                {
                    case 1:
                        obstacles.Add(new Cactus((int)(images[1].Width), (int)(images[1].Height), images[1], this, Width - images[1].Width, 400 - images[1].Height));
                        break;
                    case 2:
                        obstacles.Add(new Cactus(images[2].Width, images[2].Height, images[2], this, Width - images[2].Width, 400 - images[2].Height));
                        break;
                    case 3:
                        obstacles.Add(new Bird(images[3].Width, images[3].Height, images[3], this, Width - images[3].Width, 400 - images[3].Height - (images[0].Height * 5) / 6));
                        break;
                    case 4:
                        obstacles.Add(new Bird(images[3].Width, images[3].Height, images[3], this, Width - images[3].Width, 400 - images[3].Height - (images[0].Height * 2)));
                        break;
                }

        }

        public void CheckCollision()    
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                var collision = obstacles[i].Collision(player);
                if (collision)
                    player.Dead = true;
            }
        }

        private void AddObstacleRelatedInfo(Player p, List<float> tmp)
        {
            float distance = float.MaxValue;
            float height = 0f;
            float width = 0f;
            float birdHeight = 0f;
            float PlayerWidth = p.Duck ? p.DuckWidth : p.Width;
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (p.X + PlayerWidth < obstacles[i].X && distance > obstacles[i].X - p.X - PlayerWidth)
                {
                    height = obstacles[i].Height;
                    width = obstacles[i].Width;
                    distance = obstacles[i].X - p.X - PlayerWidth;
                    birdHeight = obstacles[i].Y;
                }
            }
            tmp.Add(distance);
            tmp.Add(height);
            tmp.Add(width);
            tmp.Add(birdHeight);
        }

        public List<List<float>> CountParameters()
        {
            var parameters = new List<List<float>>(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                var p = (Player)players[i];
                var tmp = new List<float>();

                AddObstacleRelatedInfo(p, tmp);
                tmp.Add(p.Duck ? p.Y - p.Height + p.DuckHeight : p.Y);
                tmp.Add(obstacleSpeed * 80);
                tmp.Add(obstacleSpeed);
                tmp.Add(1f); //Bias
                parameters.Add(tmp);
            }
            return parameters;
        }

        public float FitnessFunction(Genome genome)
        {
            var p = (Player)genome.Player;
            return p.Score * p.Score;
        }

        public void ResetPositions()
        {
            earlyGame = true;
            obstacleSpeed = 5f;
            obstacles = new List<Obstacle>();
            for (int i = 0; i < players.Count; i++)
            {
               
                ((Player)players[i]).Reset();
              
            }
        }

        public void ResetGame()
        {
            earlyGame = true;
            obstacleSpeed = 5f;
            obstacles = new List<Obstacle>();
            player.Reset();
        }

        public void CheckCollision(bool simulation = true)
        {
            for (int i = 0; i < players.Count; i++)
            {
                var p= (Player)players[i];
                for (int j = 0; j < obstacles.Count; j++)
                {
                    var collision = obstacles[j].Collision(p);
                    if (collision)
                        p.Dead = true;
                }
            }
        }

        private void DetermineBehaviour(List<float> readKeys, Player p)
        {
            int maxIndex = -1;
            float maxControl = int.MinValue;
            for (int j = 0; j < readKeys.Count; j++)
            {
                if (maxControl < readKeys[j])
                {
                    maxControl = readKeys[j];
                    maxIndex = j;
                }
            }
            switch (maxIndex)
            {
                case 0:
                    p.BigJump = true;
                    p.SmallJump = false;
                    p.Duck = false;
                    break;
                case 1:
                    p.BigJump = false;
                    p.SmallJump = true;
                    p.Duck = false;
                    break;
                case 2:
                    p.BigJump = false;
                    p.SmallJump = false;
                    p.Duck = true;
                    break;
                case 3:
                    p.BigJump = false;
                    p.SmallJump = false;
                    p.Duck = false;
                    break;
            }
        }

        public bool MoveElements(List<List<float>> readKeys, ref int elapsed, ref int elapsed2, ref int max)
        {
            bool allDead = true;
            bool speedUp = false;
            run += (int)obstacleSpeed ;
            for (int i = 0; i < players.Count; i++)
            {
                var p = (Player)players[i];
                if (p.Score > max)
                    max = p.Score;

                //DetermineBehaviour(readKeys[i], p);
                p.BigJump = readKeys[i][0] > 0.5f ? true : false;
                p.SmallJump = readKeys[i][1] > 0.5f ? true : false;
                p.Duck = readKeys[i][2] > 0.5f ? true : false;
                p.Move();
                if (p.Score > 20)
                    earlyGame = false;
                if (p.Dead == false)
                    allDead = false;
                speedUp = SpeedUpAndScoreSimulation(elapsed, elapsed2, p, 1.2f);
            }
            for (int i = 0; i < obstacles.Count; i++)
            {
                
                obstacles[i].Move(obstacleSpeed);
            }
            if (elapsed > 1000)
                elapsed = 0;
            elapsed2 = 0;
            obstacleSpeed += 0.002f;
            // if (speedUp)
            //     obstacleSpeed *= 1.2f;
            return allDead;
        }

        public void MoveElements(bool up, bool down, bool left, ref int elapsed, ref int elapsed2)
        {
            SpeedUpAndScore(ref elapsed, ref elapsed2, this.player);
            player.BigJump = up;
            player.SmallJump = left;
            player.Duck = down;
            run += (int)obstacleSpeed;
            earlyGame = false;

            player.Move();
            elapsed2 = 0;
            obstacleSpeed += 0.002f;
            if (player.Dead)
                State = GameState.Loss;
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacles[i].Move(obstacleSpeed);
            }
        }

        public void PrintMap(System.Windows.Forms.PaintEventArgs e)
        {
            player.PrintYourself(e);
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacles[i].PrintYourself(e);
            }
        }

        public void PrintMap(System.Windows.Forms.PaintEventArgs e, bool simulation = true)
        {
            for (int i = 0; i < players.Count; i++)
            {
                var p = (Player)players[i];
                p.PrintYourself(e);
            }
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacles[i].PrintYourself(e);
            }
        }
    }
}
