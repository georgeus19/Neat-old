using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neuroevolution.Neat;


namespace World_s_hardest_game
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Game game;
        Graphics g;
        Neat neat;
        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;

        /// <summary>
        /// Runs World's hardest game
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            g = CreateGraphics();
            game = new Game("levels.txt","tilesF.png", "square.png", "ball.png");
            this.Size = new Size(Game.COLUMNS * game.tileSize, Game.ROWS * game.tileSize);
            timer1.Enabled = true;
            button4.Visible = false;
            button4.Enabled = false;
            button5.Visible = false;
            button5.Enabled = false;
            button1.Visible = false;
            button1.Enabled = false;
            button2.Visible = false;
            button2.Enabled = false;
            elapsed = 0;
            genCount = 0;
            button3.Visible = false;
            button3.Enabled = false;
        }

        /// <summary>
        /// Runs World's hardest game simulation
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            g = CreateGraphics();
            game = new Game("levels.txt", "tilesF.png", "square.png", "ball.png", "champ.png", 300);
            this.Size = new Size(Game.COLUMNS * game.tileSize, Game.ROWS * game.tileSize);
            button4.Visible = false;
            button4.Enabled = false;
            button5.Visible = false;
            button5.Enabled = false;
            button1.Visible = false;
            button1.Enabled = false;
            button2.Visible = false;
            button2.Enabled = false;
            elapsed = 0;
            genCount = 0;
            button3.Visible = false;
            button3.Enabled = false;
            var inputNodes = game.CreateInputNodes();
            var outputNodes = game.CreateOutputNodes();
            var nodes = game.CreateNodes();
            var genes = game.GetStartGenes(nodes);
            neat = new Neat(nodes, genes, new Func<Genome, float>(game.FitnessFunction), game.Players);
            neat.SetParameters(c3: 0.5f);
            timer2.Enabled = true;
            label1.Text = "Generation: " + genCount.ToString();
            label1.Visible = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (game.State)
            {           // paint gamemap and movement of things
                case GameState.Ingame:
                    game.MoveElements(up, down, left, right);
                    game.CheckBallPlayerCollision(game.Player);
                    if (game.CheckForVictory(game.Player))
                        game.State = GameState.Victory;
                    Invalidate();
                    break;
                case GameState.Victory:

                    break;
            }
        }

        int elapsed = 0;
        int timeForGeneration = 12000;
        int genCount = 0;

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Interval = timer1.Interval;
            switch (game.State)
            {
                case GameState.Ingame:
                    if (elapsed > timeForGeneration)
                    {
                        game.State = GameState.Loss;
                    }
                    game.UpdateTime(elapsed, timeForGeneration);
                    var simulationResult = game.SimulateStep(neat);
                    game.MoveElements(simulationResult);
                    game.CheckBallPlayerSCollision();
                    Invalidate();
                    
                    elapsed += timer2.Interval;
                    break;
                case GameState.Loss:
                    genCount++;
                    label1.Text = "Generation: " + genCount.ToString();
                    neat.CreateNewGeneration();
                    //game.RunChampionsAgain(neat.Champions);
                    Invalidate();
                    game.State = GameState.Ingame;

                    game.ResetPositions();
                    elapsed = 0;
                    break;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Up)
                up = true;
            else if (e.KeyValue == (char)Keys.Down)
                down = true;
            else if (e.KeyValue == (char)Keys.Left)
                left = true;
            else if (e.KeyValue == (char)Keys.Right)
                right = true;
            else if (e.KeyValue == (char)Keys.Escape)
            {
                button4.Visible = true;
                button4.Enabled = true;
                button5.Visible = true;
                button5.Enabled = true;
                button3.Enabled = true;
                button3.Visible = true;
                button1.Visible = true;
                button1.Enabled = true;
                button2.Visible = true;
                button2.Enabled = true;
                label1.Visible = false;
                label2.Visible = false;
                dinoGame = null;
                game = null;
                elapsed = 0;
                genCount = 0;
                timeRun = 0;
                timer4.Enabled = false;
                timer3.Enabled = false;
                timer2.Enabled = false;
                timer1.Enabled = false;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Up)
                up = false;
            else if (e.KeyValue == (char)Keys.Down)
                down = false;
            else if (e.KeyValue == (char)Keys.Left)
                left = false;
            else if (e.KeyValue == (char)Keys.Right)
                right = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (game != null)
            {
                if (timer1.Enabled == true)
                    game.PrintMap(e);
                else
                    game.PrintMap(e, neat.Beast, true);
                base.OnPaint(e); // works also without this
            }
            if (dinoGame != null && timer3.Enabled == true)
                dinoGame.PrintMap(e);
            else if (dinoGame != null && timer4.Enabled == true)
                dinoGame.PrintMap(e, true);
        }

        /// <summary>
        /// Runs XOR test
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            Xor xor = new Xor();
            xor.RunSimulation();
        }

        Dino.Game dinoGame;
        int timeRun = 0;
      
        /// <summary>
        /// Runs Dino game
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.SandyBrown;
            int width = 1000;
            int height = 500;
            this.Size = new Size(width, height);
            dinoGame = new Dino.Game(width, height);
            timer3.Enabled = true;
            button4.Visible = false;
            button4.Enabled = false;
            button5.Visible = false;
            button5.Enabled = false;
            button1.Visible = false;
            button1.Enabled = false;
            button2.Visible = false;
            button2.Enabled = false;
            elapsed = 0;
            genCount = 0;
            button3.Visible = false;
            button3.Enabled = false;
            timer3.Interval = timer1.Interval;
        }

        /// <summary>
        /// Runs simulation of Dino game
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.SandyBrown;
            int width = 1000;
            int height = 500;
            this.Size = new Size(width, height);
            dinoGame = new Dino.Game(width, height, 500);
            var nodes = dinoGame.CreateNodes();
            neat = new Neat(nodes, new SortedList<int, Gene>(), dinoGame.FitnessFunction, dinoGame.players);
            neat.SetParameters();
            timer4.Enabled = true;
            button4.Visible = false;
            button4.Enabled = false;
            button5.Visible = false;
            button5.Enabled = false;
            button1.Visible = false;
            button1.Enabled = false;
            button2.Visible = false;
            button2.Enabled = false;
            elapsed = 0;
            genCount = 0;
            button3.Visible = false;
            button3.Enabled = false;
            label1.Visible = true;
            label2.Visible = true;
            timer4.Interval = timer1.Interval;
        }

        int score = int.MinValue;

        private void timer4_Tick(object sender, EventArgs e)
        {
            switch (dinoGame.State)
            {
                case Dino.GameState.Ingame:
                    dinoGame.ManageObstacles();
                    var results = neat.SimulateStepFromNeat(dinoGame.CountParameters());
                    if (dinoGame.MoveElements(results, ref elapsed, ref timeRun, ref score))
                        dinoGame.State = Dino.GameState.Loss;
                    dinoGame.CheckCollision(true);
                    label2.Text = "Score: " + score;
                    elapsed += timer4.Interval;
                    timeRun += timer4.Interval;
                    Invalidate();
                    break;
                case Dino.GameState.Loss:
                    score = int.MinValue;
                    genCount++;
                    label1.Text = "Generation: " + genCount.ToString();
                    neat.CreateNewGeneration();
                    elapsed = 0;
                    timeRun = 0;
                    dinoGame.ResetPositions();
                    dinoGame.State = Dino.GameState.Ingame;
                    Invalidate();
                    break;
                
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            switch (dinoGame.State)
            {
                case Dino.GameState.Ingame:
                    timeRun += timer3.Interval;
                    dinoGame.ManageObstacles();
                    dinoGame.MoveElements(up, down, left, ref elapsed, ref timeRun);
                    dinoGame.CheckCollision();
                    Invalidate();
                    elapsed += timer3.Interval;
                    timeRun += timer3.Interval;
                    break;
                case Dino.GameState.Loss:
                    dinoGame.ResetGame();
                    elapsed = 0;
                    dinoGame.State = Dino.GameState.Ingame;
                    timeRun = 0;
                    break;
            }
        }
    }
}
