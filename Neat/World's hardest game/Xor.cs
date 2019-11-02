using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroevolution.Neat;
using System.IO;
namespace World_s_hardest_game
{
    class XorInputs
    {
        public float X { get; set; }
        public float Y { get; set; }

        public XorInputs(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    class Xor
    {
        XorInputs Inputs { get; set; }
        List<List<float>> first;
        List<List<float>> second;
        List<List<float>> third;
        List<List<float>> fourth;
        public Genome bestGenome = null;
        private List<Node> CreateInputNodes()
        {
            List<Node> inputNodes = new List<Node>();
            inputNodes.Add(new Node(NodeType.Sensor, 0, 0));
            inputNodes.Add(new Node(NodeType.Sensor, 0, 1));
            inputNodes.Add(new Node(NodeType.Sensor, 0, 2));
            return inputNodes;
        }

        private List<List<float>> GetParameters(int par)
        {
            var res = new List<List<float>>(150);
            for (int i = 0; i < 150; i++)
            {
                List<float> param = new List<float>(3);
                if (par == 0)
                {
                    param.Add(1);
                    param.Add(1);
                    param.Add(1);
                }
                else if (par == 1)
                {
                    param.Add(0);
                    param.Add(1);
                    param.Add(1);
                }
                else if (par == 2)
                {
                    param.Add(1);
                    param.Add(0);
                    param.Add(1);
                }
                else if (par == 3)
                {
                    param.Add(0);
                    param.Add(0);
                    param.Add(1);
                }
                res.Add(param);
            }
          
            return res;
            
        }

        class Player
        {
            public int Id { get; set; }
            public Xor xor { get; set; }
        
            public Player(int id, Xor x)
            {
                Id = id;
                xor = x;
            }
        }

        private void OptimalGenome(out List<Node> input, out SortedList<int, Gene> genes)
        {
            Random rnd = new Random();
            input = new List<Node>(3);
            var n1 = new Node(NodeType.Sensor, 0, 0);
            var n2 = new Node(NodeType.Sensor, 0, 1);
            var n3 = new Node(NodeType.Sensor, 0, 2);
            input.Add(n1);
            input.Add(n2);
            input.Add(n3);
            var h1 = new Node(NodeType.Hidden, 1, 3);
            input.Add(h1);
            var o1 = new Node(NodeType.Output, 2, 4);
            input.Add(o1);
            genes = new SortedList<int, Gene>(7);
            genes.Add(0, new Gene(n1, o1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 0));
            genes.Add(1, new Gene(n2, o1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 1));
            genes.Add(2, new Gene(n3, o1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 2));
            genes.Add(3, new Gene(n1, h1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 3));
            genes.Add(4, new Gene(n2, h1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 4));
            genes.Add(5, new Gene(n3, h1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 5));
            genes.Add(6, new Gene(h1, o1, (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, 6));
        }

        public void RunSimulation()
        {
            var results = new List<int>(100);
            int generation = 0;
            for (int j = 0; j < 100; j++)
            {
                List<object> players = new List<object>(100);
                for (int i = 0; i < 150; i++)
                {
                    players.Add(new Player(i, this));
                }
                //List<Node> input;
                //SortedList<int, Gene> genes;
                Random rnd = new Random();
                //OptimalGenome(out input, out genes);
                //var inp = CreateInputNodes();
                var g = new SortedList<int, Gene>(3);
                List<Node> nodes = new List<Node>(4);
                nodes.Add(new Node(NodeType.Sensor, 0, 0));
                nodes.Add(new Node(NodeType.Sensor, 0, 1));
                nodes.Add(new Node(NodeType.Sensor, 0, 2));
                nodes.Add(new Node(NodeType.Output, 1, 3));
                for (int i = 0; i < 3; i++)
                {
                    var gene = new Gene(nodes[i], nodes[3], (float)rnd.NextDouble() * (rnd.Next(0, 10) >= 5 ? 1f : -1f), GeneType.Enabled, i);
                    g.Add(i, gene);
                    nodes[i].OutgoingConnections.Add(gene);
                }
                Neat neat = new Neat(nodes, g, FitnessFunction, players);
                neat.SetParameters(c3:1);
                // Neat neat = new Neat(nodes, new SortedList<int, Gene>(), FitnessFunction, players);
                // Neat neat = new Neat(input, genes, FitnessFunction, players);
                generation = 0;
                bestGenome = null;
                while (bestGenome == null)
                {
                    first = neat.SimulateStepFromNeat(GetParameters(0));
                    second = neat.SimulateStepFromNeat(GetParameters(1));
                    third = neat.SimulateStepFromNeat(GetParameters(2));
                    fourth = neat.SimulateStepFromNeat(GetParameters(3));
                    neat.CreateNewGeneration();
                    generation++;
                }
                results.Add(generation);
            }
            StreamWriter sw = new StreamWriter("xor.txt");
            sw.WriteLine("XOR results, 100 runs");
            sw.WriteLine("how many generation it took for each run:");
            int max = 0;
            for (int i = 0; i < results.Count; i++)
            {
                max += results[i];
                sw.WriteLine(i + ". run -> " + results[i]);
            }
            sw.WriteLine("Average generation in a run: " + (double)max / results.Count);
            sw.Close();
        }

        public float FitnessFunction(Genome genome)
        {
            float output = first[((Player)genome.Player).Id][0];
            float result = 0f;
            float fitness = 0f;
            result += (output < 0.5f) ? 0f : 1f;
            fitness += (0 - output) * (0 - output);

            output = second[((Player)genome.Player).Id][0];
            result += (output > 0.5f) ? 0f : 1f;
            fitness += (1 - output) * (1 - output);

            output = third[((Player)genome.Player).Id][0];
            result += (output > 0.5f) ? 0f : 1f;
            fitness += (1 - output) * (1 - output);

            output = fourth[((Player)genome.Player).Id][0];
            result += (output < 0.5f) ? 0f : 1f;
            fitness += (0 - output) * (0 - output);

            if (result == 0)
                bestGenome = genome;
            return 4f - fitness;
        }
    }
}
