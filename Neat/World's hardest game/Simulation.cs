using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Neuroevolution.Neat;

namespace World_s_hardest_game
{
    internal struct MovementKeys
    {
        public bool Up { get; set; }
        public bool Down { get; set; }
        public bool Right { get; set; }
        public bool Left { get; set; }

        public MovementKeys(bool up, bool down, bool right, bool left) : this()
        {
            Up = up;
            Down = down;
            Right = right;
            Left = left;
        }
    }

    partial class Game
    {
        public List<object> Players { get; set; }
        private float col = 19f;
        private float row = 4.5f;

        public Game(string levelPath, string tilesBmpPath, string squarePath, string ballPath, string champPath, int numberOfPlayers = 100)
        {
            ReadLevel(levelPath);
            tileSize = AddBmp(new Bitmap(tilesBmpPath));
            squareSize = AddBmp(new Bitmap(squarePath));
            ballSize = AddBmp(new Bitmap(ballPath));
            var tmp = AddBmp(new Bitmap(champPath));
            State = GameState.Ingame;
            Players = CreatePlayers(numberOfPlayers);
            Player = null;
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize + tileSize * 0.25f, 5, this, new Directions(-1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize + tileSize * 1.25f, 5, this, new Directions(1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize + tileSize * 2.25f, 5, this, new Directions(-1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize - tileSize * 0.75f, 5, this, new Directions(1, 0)));
            balls.Add(new BouncingBall(COLUMNS / 2 * tileSize - tileSize * 0.25f, ROWS / 2 * tileSize - tileSize * 1.75f, 5, this, new Directions(-1, 0)));
        }

        private List<object> CreatePlayers(int numberOfPlayers = 100)
        {
            List<object> players = new List<object>(numberOfPlayers);
            for (int i = 0; i < numberOfPlayers; i++)
            {
                players.Add(new Square(5 * tileSize, (ROWS + 3) * tileSize / 2f, 4, this));
            }
            return players;
        }
        public void UpdateTime(int elapsed, int timeRound)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                ((Square)Players[i]).Elapsed = elapsed;
                ((Square)Players[i]).RoundTime = timeRound;
            }
        }

        public List<Node> CreateNodes()
        {
            var nodes = new List<Node>(balls.Count * 3 + 7);
            for (int i = 0; i < 6; i++)
            {
                nodes.Add(new Node(NodeType.Sensor, 0, i));
            }
            for (int i = 0; i < 4; i++)
            {
                nodes.Add(new Node(NodeType.Output, 1, 6 + i));
            }
            return nodes;
        }

        public SortedList<int, Gene> GetStartGenes(List<Node> nodes)
        {
            SortedList<int, Gene> genes = new SortedList<int, Gene>(24);
            int innovation = 0;
            Random rnd = new Random();
            for (int i = 0; i < 6; i++)
            {
                Gene gene = new Gene(nodes[i], nodes[6], 0f, GeneType.Enabled, i);
                gene.Weight = gene.AssignRandomWeight(rnd);
                genes.Add(gene.Innovation, gene);
                gene.In.OutgoingConnections.Add(gene);
            }
            for (int i = 0; i < 6; i++)
            {
                Gene gene = new Gene(nodes[i], nodes[7], 0f, GeneType.Enabled, i + 6);
                gene.Weight = gene.AssignRandomWeight(rnd);
                genes.Add(gene.Innovation, gene);
                gene.In.OutgoingConnections.Add(gene);
            }
            for (int i = 0; i < 6; i++)
            {
                Gene gene = new Gene(nodes[i], nodes[8], 0f, GeneType.Enabled, i + 12);
                gene.Weight = gene.AssignRandomWeight(rnd);
                genes.Add(gene.Innovation, gene);
                gene.In.OutgoingConnections.Add(gene);
            }
            for (int i = 0; i < 6; i++)
            {
                Gene gene = new Gene(nodes[i], nodes[9], 0f, GeneType.Enabled, i + 18);
                gene.Weight = gene.AssignRandomWeight(rnd);
                genes.Add(gene.Innovation, gene);
                gene.In.OutgoingConnections.Add(gene);
            }
            return genes;
        }

        private List<List<float>> CountParameters(List<Square> players, List<Ball> balls_)
        {
            List<List<float>> results = new List<List<float>>(players.Count);
            List<float> tmp;
            for (int i = 0; i < players.Count; i++)
            {
                tmp = new List<float>(balls_.Count + 4);
                for (int j = 0; j < balls_.Count; j++)
                {
                    tmp.Add(Distance(players[i].Xc, balls_[j].Xc, players[i].Yc, balls_[j].Yc));
                }
                tmp.Add(CountDistanceToWallX(players[i], 1));
                tmp.Add(CountDistanceToWallX(players[i], -1));
                tmp.Add(CountDistanceToWallY(players[i], 1));
                tmp.Add(CountDistanceToWallY(players[i], -1));
                results.Add(tmp);
            }
            return results;
        }

        // for parallel simulation can be used Players in param and balls in param 
        private List<List<float>> CountParameters()
        {
            List<List<float>> results = new List<List<float>>(Players.Count);
            List<float> tmp;
          
            for (int i = 0; i < Players.Count; i++)
            {
                tmp = new List<float>(balls.Count + 4);
                var player = (Square)Players[i];
                //for (int j = 0; j < balls.Count; j++)
                //{
                //    tmp.Add(Distance(((Square)Players[i]).Xc, balls[j].Xc, ((Square)Players[i]).Yc, balls[j].Yc));
                //    tmp.Add(balls[j].Xc);
                //    tmp.Add(balls[j].Yc);
                //}
                tmp.Add(CountDistanceToWallX(player, 1));
                tmp.Add(CountDistanceToWallX(player, -1));
                tmp.Add(CountDistanceToWallY(player, 1));
                tmp.Add(CountDistanceToWallY(player, -1));
                //tmp.Add(((Square)Players[i]).Xc);
                //tmp.Add(((Square)Players[i]).Yc);
                
                float x = col * tileSize;
                float y = row * tileSize;
                tmp.Add(Distance(new Vector(player.Xc, player.Yc), new Vector(x, y)));
                tmp.Add(1);
                results.Add(tmp);
            }
            return results;
        }

        public void ResetPositions()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                ((Square)Players[i]).X = 5 * tileSize;
                ((Square)Players[i]).Y = (ROWS + 3) * tileSize / 2f;
                ((Square)Players[i]).Dead = false;
            }

            balls[0].X = COLUMNS / 2 * tileSize - tileSize * 0.25f; 
            balls[0].Y = ROWS / 2 * tileSize + tileSize * 0.25f;

            balls[1].X = COLUMNS / 2 * tileSize - tileSize * 0.25f;
            balls[1].Y = ROWS / 2 * tileSize + tileSize * 1.25f;

            balls[2].X = COLUMNS / 2 * tileSize - tileSize * 0.25f;
            balls[2].Y = ROWS / 2 * tileSize + tileSize * 2.25f;

            balls[3].X = COLUMNS / 2 * tileSize - tileSize * 0.25f;
            balls[3].Y = ROWS / 2 * tileSize - tileSize * 0.75f;

            balls[4].X = COLUMNS / 2 * tileSize - tileSize * 0.25f;
            balls[4].Y = ROWS / 2 * tileSize - tileSize * 1.75f;
        }

        private float CountDistanceToWallY(Square player, int direction)
        {
            Tile tile;
            tile = map[(int)(player.Yc / tileSize),(int) player.Xc / tileSize];
            int i = 1;
            while (tile.GetType() != typeof(VoidTile))
            {
                tile = map[(int)(player.Yc + i++ * direction * tileSize) / tileSize, (int)player.Xc / tileSize];
            }
            return (i - 1) * tileSize + squareSize / 2;
        }
        private float CountDistanceToWallX(Square player, int direction)
        {
            Tile tile;
            tile = map[(int)(player.Yc / tileSize), (int)player.Xc / tileSize];
            int i = 1;
            while (tile.GetType() != typeof(VoidTile))
            {
                tile = map[(int)(player.Yc / tileSize), (int)(player.Xc + (i++ * direction * tileSize)) / tileSize];
            }
            return (i - 1) * tileSize + squareSize / 2;
        }

        public List<List<float>> SimulateStep(Neat neat)
        {
            var parameters = CountParameters();
            var floatResults = neat.SimulateStepFromNeat(parameters);
            return floatResults;
        }

        public List<Node> CreateOutputNodes()
        {
            var outputNodes = new List<Node>(4);
            for (int i = 0; i < 4; i++)
            {
                outputNodes.Add(new Node(NodeType.Output, 1, balls.Count + 7)); // CHECK FOR CORRECT NODE NUMBER !!!
            }
            return outputNodes;
        }

        private float Distance(float x1, float x2, float y1, float y2)
        {
            return (float)(Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)));
        }

        public List<Node> CreateInputNodes()
        {
            List<Node> inputNodes = new List<Node>(balls.Count);
            for (int i = 0; i < balls.Count * 3; i++)
            {
                inputNodes.Add(new Node(NodeType.Sensor, 0, i)); // for distance from each ball
            }
            for (int i = 0; i < 7; i++)
            {
                inputNodes.Add(new Node(NodeType.Sensor, 0, balls.Count + i)); // 4 for distance to walls, 1 bias, player position
            }
            return inputNodes;
        }

        public float FitnessFunction(Genome genome)
        {
            Square player = (Square)genome.Player;
            int time = player.RoundTime - player.Elapsed;
            float x = col * tileSize;
            float y = row * tileSize;
            float max = (float)Math.Sqrt(COLUMNS * tileSize * COLUMNS * tileSize + ROWS * tileSize * ROWS * tileSize);
            return (max - Distance(new Vector(player.Xc, player.Yc), new Vector(x, y))) * (max - Distance(new Vector(player.Xc, player.Yc), new Vector(x, y)));
        }

        public void MoveElements(List<List<float>> readKyes)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].Move();
            }

            for (int i = 0; i < Players.Count; i++)
            {
                Square player = (Square)Players[i];
                if (!player.Dead)
                {
                    player.Up = readKyes[i][0] > 0.5f ? true : false;
                    player.Down = readKyes[i][1] > 0.5f ? true : false;
                    player.Left = readKyes[i][2] > 0.5f ? true : false;
                    player.Right = readKyes[i][3] > 0.5f ? true : false;
                    player.Move();
                }
            }
        }

        
        public void CheckBallPlayerSCollision()
        {
            for (int i = 0; i < Players.Count; i++)
            {       
                    // uncomment for checking collision!
                //CheckBallPlayerCollision((Square)Players[i]);
            }
        }

        /// <summary>
        /// Prints the whole map and objects on it
        /// </summary>
        public void PrintMap(System.Windows.Forms.PaintEventArgs e, Genome Beast, bool simulation = true)
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

            for (int i = 0; i < Players.Count; i++)
            {
                ((Square)Players[i]).PrintYourself(e);
            }
            if (Beast.Player != null)
                ((Square)Beast.Player).PrintYourself(e, true);
        }
    }
}
