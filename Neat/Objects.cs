using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroevolution.Neat
{
    public enum NodeType { Sensor, Output, Hidden }
    public enum GeneType { Enabled, Disabled, AddGene, AddNode}

    public class Node : IEquatable<Node>
    {
        /// <summary>
        /// Value that is stored in node - either before activation or after
        /// </summary>
        public float Activation { get; set; }

        /// <summary>
        /// Tells if value in Activation is activate or not
        /// </summary>
        public bool Activated { get; set; }

        /// <summary>
        /// Type of node - sensor, hidden, output
        /// </summary>
        public NodeType Type { get; }

        /// <summary>
        /// Layer in nerual network (sensors are on Layer 0)
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Id of node
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// List of connection genes that come from this node - this node is marked as In node for the genes and In.Layer < Out.Layer
        /// </summary>
        public List<Gene> OutgoingConnections = new List<Gene>();


        public Node(NodeType type, int layer, int number, float activation = 0f, bool activated = false)
        {
            Activation = activation;
            Activated = activated;
            Type = type;
            Layer = layer;
            Number = number;
        }

        public Node ShallowCopy()
        {
            return new Node(Type, Layer, Number, Activation, Activated);
        }

        public override int GetHashCode()
        {
            return Number;
        }

        /// <summary>
        /// Activates the node if not sensor - call Activation function.
        /// Also updates the activation value in connected nodes.
        /// </summary>
        public void Activate()
        {
            if (Type != NodeType.Sensor)
                Activation = ActivationFunction(Activation);

            for (int i = 0; i < OutgoingConnections.Count; i++)
            {
                if (OutgoingConnections[i].Type == GeneType.Enabled)
                    OutgoingConnections[i].Out.Activation += OutgoingConnections[i].Weight * Activation;  
            }
        }


        /// <summary>
        /// Currently is used steepened sigmoid function
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private float ActivationFunction(float x) => 1 / (1 + (float)Math.Pow(Math.E, -4.9 * x));// (float)Math.Tanh(x);// 1 / (1 + (float)Math.Pow(Math.E, - 4.9 * x));

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Node))
                return false;
            return ((Node)obj).Number == this.Number;
        }
        public bool Equals(Node other)
        {
            return other.Number == this.Number;
        }
    }

    public class Gene : IEquatable<Gene>
    {
        /// <summary>
        /// Node which the gene comes from ( In.Layer < Out.Layer)
        /// </summary>
        public Node In { get; set; }

        /// <summary>
        /// Output node of gene
        /// </summary>
        public Node Out { get; set; }

        /// <summary>
        /// Value on the gene
        /// </summary>
        public float Weight { get; set; }
        
        /// <summary>
        /// Primarly tells if the gene is enabled or not
        /// </summary>
        public GeneType Type { get; set; }

        /// <summary>
        /// Id of genomes
        /// </summary>
        public int Innovation { get; set; }

        public override int GetHashCode()
        {
            int hash = 23;
            hash += hash * 17 + In.GetHashCode();
            hash += hash * 17 + Out.GetHashCode();
            return hash;
        }

        public Gene ShallowCopy()
        {
            return new Gene(In, Out, Weight, Type, Innovation);
        }

        /// <summary>
        /// Used in crossover. If gene in one of the parents is disabled, it has 75% chance to be disabled.
        /// </summary>
        internal bool CrossoverChildDisabled(Random rnd, Gene other) => (Type == GeneType.Enabled && other.Type == GeneType.Enabled) || (rnd.Next(0, 100) > 75);

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Gene))
                return false;
            return ((Gene)obj).In.Number == In.Number && ((Gene)obj).Out.Number == Out.Number;
        }

        /// <summary>
        /// Mutates Weight. Either can just perturbe the value or assign a random one
        /// Uses Gaussian distribution
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="perturbeChance"></param>
        public void Mutate(Random rnd, int perturbeChance)
        {
            if (rnd.Next(0, 100) < perturbeChance)
            {
                var per = rnd.NextDouble();
                Weight += RandomGaussian(rnd, 0f, 0.1f);// Not suitable for XOR(float)per * ((rnd.Next(0, 10) >= 5) ? -1f : 1f); // Works well on XOR
                //
            }
            else
                Weight = AssignRandomWeight(rnd);
          
        }

        internal float RandomGaussian(Random rnd, float mean, float stdDevSq)
        {
            double u1 = 1.0 - rnd.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rnd.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDevSq * randStdNormal; //random normal(mean,stdDev^2)
            return (float)randNormal;
        }

        public float AssignRandomWeight(Random rnd)
        {
            return RandomGaussian(rnd, 0, 5);
            var tmp = (float)rnd.Next(10, 100) / 100f;
            tmp *= 2f;
            if (rnd.Next(0, 10) > 5)
                tmp = -1 * tmp;
            if (tmp > 1f)
                tmp = 1f;
            else if (tmp < -1f)
                tmp = -1f;
            return tmp;
        }

        public bool Equals(Gene other) => other.In.Number == In.Number && other.Out.Number == Out.Number;

        public Gene(Node @in, Node @out, float weight, GeneType type, int innovation)
        {
            In = @in;
            Out = @out;
            Weight = weight;
            Type = type;
            Innovation = innovation;
        }

    }

    public class Genome
    {
        /// <summary>
        /// Linked instance of player from given list of players
        /// </summary>
        public object Player { get; set; }

        /// <summary>
        /// List of all nodes in genome (sensors, hidden, output)
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// List of all genes used in genome
        /// </summary>
        public SortedList<int, Gene> Genes { get; set; }

        public float Fitness { get; set; } = 0f;

        public bool ToRemove { get; set; } = false;
        public Species Group { get; set; } = null;
        public bool champ = false;

        public Genome(List<Node> nodes, SortedList<int, Gene> genes, object player)
        {
            Nodes = nodes;
            Genes = genes;
            Player = player;
        }

        /// <summary>
        /// Creates a copy of genome - safe. References in Gene point to nodes in new list Nodes
        /// </summary>
        /// <returns></returns>
        public Genome ShallowCopy()
        {
            var genes = new SortedList<int, Gene>(Genes.Count);
            Gene gene;
            var dict = new Dictionary<int, Node>();
            

                // Copy all genes
            foreach (var item in Genes)
            {
                    // Shallow copies only the pointers to In, Out nodes
                gene = item.Value.ShallowCopy();
                AddNodesToDictIfNotIn(dict, gene);
                genes.Add(gene.Innovation, gene);
            }

                //Copy Nodes
            Node node;
            var nodes = new List<Node>(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (dict.TryGetValue(Nodes[i].Number, out node))
                    nodes.Add(node);
                else
                    nodes.Add(Nodes[i].ShallowCopy());
            }

            var gen = new Genome(nodes, genes, Player);
            gen.ToRemove = false;
            gen.Group = Group;
            gen.Fitness = Fitness;
            return gen;
        }

        private void AddNodesToDictIfNotIn(Dictionary<int, Node> dict, Gene gene)
        {
            Node node;
            if (dict.TryGetValue(gene.In.Number, out node))
                gene.In = node;
            else
            {
                node = gene.In.ShallowCopy();
                dict.Add(node.Number, node);
                gene.In = node;
            }
            if (dict.TryGetValue(gene.Out.Number, out node))
                gene.Out = node;
            else
            {
                node = gene.Out.ShallowCopy();
                dict.Add(node.Number, node);
                gene.Out = node;
            }
            gene.In.OutgoingConnections.Add(gene);
        }

        private void AddGeneFromThis(Gene curr1, Gene curr2, Dictionary<int, Node> dict, Random rnd, SortedList<int, Gene> genes)
        {
            var gene = curr1.ShallowCopy();
            AddNodesToDictIfNotIn(dict, gene); // Layers are OK
            gene.Type = (curr1.CrossoverChildDisabled(rnd, curr2)) ? GeneType.Enabled : GeneType.Disabled;
            genes.Add(gene.Innovation, gene);
        }

        private void AddGeneFromOther(Gene curr1, Gene curr2, Dictionary<int, Node> dict, Random rnd, SortedList<int, Gene> genes)
        {
            var gene = curr2.ShallowCopy();
            AddNodesToDictIfNotIn(dict, gene); // Layers are not OK
            gene.Type = (curr2.CrossoverChildDisabled(rnd, curr1)) ? GeneType.Enabled : GeneType.Disabled;
            genes.Add(gene.Innovation, gene);
                // Correct Layers from more fit parent A.K.A. this genome   
            gene.In.Layer = curr1.In.Layer; 
            gene.Out.Layer = curr1.Out.Layer;
        }

        /// <summary>
        /// Fitness of this is greater than or equal to fitness of other
        /// </summary>
        /// <returns>child of this and other</returns>
        internal Genome Crossover(Genome other, Random rnd, bool sameFitness, int disableChance = 75)
        {
            var enumThis = Genes.GetEnumerator();
            var enumOther = other.Genes.GetEnumerator();
            var dict = new Dictionary<int, Node>();
            SortedList<int, Gene> genes = new SortedList<int, Gene>(Genes.Count);
         
            Gene curr1 = enumThis.MoveNext() ? enumThis.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
            Gene curr2 = enumOther.MoveNext() ? enumOther.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
            while (true)
            {
                if (curr1.Innovation == curr2.Innovation)
                {
                    if (curr1.Innovation == -1)
                        break;
                    if (rnd.Next(0, 100) > 50)
                        AddGeneFromThis(curr1, curr2, dict, rnd, genes);
                    else
                        AddGeneFromOther(curr1, curr2, dict, rnd, genes);
                    curr1 = enumThis.MoveNext() ? enumThis.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    curr2 = enumOther.MoveNext() ? enumOther.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                }
                else if (curr1.Innovation < curr2.Innovation)
                {
                    if (curr1.Innovation == -1)
                    {
                        if (sameFitness)
                            AddGeneFromThis(curr2, curr1, dict, rnd, genes);
                        curr2 = enumOther.MoveNext() ? enumOther.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                    else
                    {
                        AddGeneFromThis(curr1, curr2, dict, rnd, genes);
                        curr1 = enumThis.MoveNext() ? enumThis.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                }
                else
                {
                    if (curr2.Innovation == -1)
                    {
                        AddGeneFromThis(curr1, curr2, dict, rnd, genes);
                        curr1 = enumThis.MoveNext() ? enumThis.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                    else
                    {
                        if (sameFitness)
                            AddGeneFromThis(curr2, curr1, dict, rnd, genes);
                        // THE OTHER GENE HAS LOWER FITNESS SO WHY ADD IT'S GENE ??? - case onlu with same fitness of genomes
                        curr2 = enumOther.MoveNext() ? enumOther.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                }
            }
           
            var nodes = new List<Node>(Nodes.Count);
            // Copy Nodes from Genes from Other Genome
            foreach (var item in dict) 
            {
                nodes.Add(item.Value);
               
            }
            Node node;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (dict.TryGetValue(Nodes[i].Number, out node))
                    ;
                else
                    nodes.Add(Nodes[i].ShallowCopy());
            }

            nodes.Sort(new NodeNumberComparer());


            var child = new Genome(nodes, genes, null);
                // CORRECT LAYER MESS
            if (sameFitness) 
            {
                bool notdone = false;
                do
                {
                    notdone = false;
                    foreach (var item in genes)
                    {
                        var g = item.Value;
                        while (g.In.Layer >= g.Out.Layer)
                        {
                            g.Out.Layer++;
                            notdone = true;
                        }
                    }
                }
                while (notdone);
            }
            return child;
        }

        class NodeNumberComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                return  x.Number.CompareTo(y.Number);
            }
        }

        private bool FindMistakeOfGenome(Genome genome)
        {
            foreach (var item in genome.Genes)
            {
                var gene = item.Value;
                bool inFound = false;
                bool outFound = false;
                for (int i = 0; i < genome.Nodes.Count; i++)
                {
                    if (genome.Nodes[i].Number == gene.In.Number)
                        inFound = true;
                    if (genome.Nodes[i].Number == gene.Out.Number)
                        outFound = true;
                }
                if (inFound == false || outFound == false)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Fitness is shared in each species -> The bigger the species the lower fitness.
        /// </summary>
        /// <param name="fitnessFunction"></param>
        internal void FitnessSharingFunction(Func<Genome, float> fitnessFunction) => Fitness = fitnessFunction(this) / Group.Count;

        private float ActivationFunction(float x) => 1 / (1 + (float)Math.Pow(Math.E, -4.9 * x));// (float)Math.Tanh(x);// 1 / (1 + (float)Math.Pow(Math.E, - 4.9 * x));

        public int GetMaxInnovation()
        {
            int max = int.MinValue;
            foreach (var item in Genes)
            {
                if (item.Value.Innovation > max)
                    max = item.Value.Innovation;
            }
            return max;
        }

        public List<float> GetOutputs()
        {
            var result = new List<float>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type != NodeType.Output) // filter out all non-output nodes
                    continue;
                if (Nodes[i].Activated == false)
                {
                    Nodes[i].Activation = ActivationFunction(Nodes[i].Activation);
                    Nodes[i].Activated = true;
                    result.Add(Nodes[i].Activation);
                }
                else
                {
                    result.Add(Nodes[i].Activation);
                }
            }
            return result;
        }

        public void SetInputs(List<float> inputs)
        {
            int j = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type == NodeType.Sensor)
                {
                    Nodes[i].Activation = inputs[j]; // WHY -> no activation is better ? for raw inputs?
                    ++j;
                    Nodes[i].Activated = true;
                }
            }
        }

        class NodeComp : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                if (x <= y)
                    return -1;
                else return 1;
            }
        }

        /// <summary>
        /// Takes inputs and computes outputs of neural network
        /// </summary>
        public List<float> ActivateNetwork(List<float> inputValues)
        {
            int par = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type == NodeType.Sensor)
                {
                    Nodes[i].Activation = inputValues[par];
                    par++;
                    Nodes[i].Activated = true;
                }
                else
                {
                    Nodes[i].Activated = false;
                    Nodes[i].Activation = 0f;
                }
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].Activate();
            }

            var result = new List<float>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Type == NodeType.Output)
                    result.Add(Nodes[i].Activation);
            }
            return result;
        }

        public List<float> ComputeOutputs()
        {
            Gene gene;
            SortedList<int, Gene> actGenes = new SortedList<int, Gene>(new NodeComp());
            foreach (var item in Genes)
            {
                gene = item.Value;
                actGenes.Add(gene.In.Layer, gene);
            }

            foreach (var pair in actGenes)
            {
                gene = pair.Value;
                if (gene.Type == GeneType.Disabled)
                    continue;
                if (gene.In.Activated == false)
                {
                    gene.In.Activation = ActivationFunction(gene.In.Activation);
                    gene.In.Activated = true;
                }
                gene.Out.Activation += gene.Weight * gene.In.Activation;
            }
            return GetOutputs();
        }

        /// <summary>
        /// Mutates all weights of all genes present in genome 
        /// </summary>
        private void MutateWeights(Random rnd, int perturbeChance)
        {
            foreach (var gene in Genes)
            {
                gene.Value.Mutate(rnd, perturbeChance);
            }
        }

        private bool FindExistingNode(Gene gene, ref Node node, ref Gene firstGene, ref Gene latterGene, List<Gene> allGenes, ref int innovation, ref int number)
        {
            foreach (var genePair in allGenes)
            {           // find first of the two genes
                if (genePair.Type != GeneType.AddGene && genePair.Innovation != gene.Innovation && genePair.In.Number == gene.In.Number)
                {
                    foreach (var latterPair in allGenes)
                    {           // find latter of the two genes
                        if (latterPair.Type != GeneType.AddGene && latterPair.Innovation != gene.Innovation && latterPair.Out.Number == gene.Out.Number 
                            && genePair.Out.Number == latterPair.In.Number)
                        {
                            firstGene = genePair.ShallowCopy();
                            firstGene.In = gene.In;
                            firstGene.Type = GeneType.Enabled;
                            latterGene = latterPair.ShallowCopy();
                            latterGene.Out = gene.Out;
                            latterGene.Type = GeneType.Enabled;
                            node = latterGene.In.ShallowCopy(); // Can't exist in this genome 
                            firstGene.Out = node;
                            latterGene.In = node;
                            node.Layer = gene.In.Layer + 1;
                            return true;
                        }
                    }
                }
            }
     
            return false;
        }

        /// <summary>
        /// Corrects invariant that forEach gene : In.Layer < Out.Layer.
        /// Just one round of correction
        /// </summary>
        internal void IncrementLayer(Node node)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (node.Number != Nodes[i].Number && Nodes[i].Layer >= node.Layer)
                    Nodes[i].Layer++;
            }
        }

        /// <summary>
        /// Add nodes to random connection gene in genome
        /// </summary>
        private void AddNode(Random rnd, ref int innovation, ref int number, List<Gene> allGenes)
        {
            int index = rnd.Next(0, Genes.Count);
            var pair = Genes.ElementAt(index);
            Gene gene = pair.Value;
            gene.Type = GeneType.Disabled;
            Node node = null;
            Gene firstGene = null;
            Gene latterGene = null;
            bool exists = FindExistingNode(gene, ref node, ref firstGene, ref latterGene, allGenes, ref innovation, ref number);
            if (!exists)
            {
                node = new Node(NodeType.Hidden, gene.In.Layer + 1, ++number);
                firstGene = new Gene(gene.In, node, 1f, GeneType.Enabled, ++innovation);
                latterGene = new Gene(node, gene.Out, gene.Weight, GeneType.Enabled, ++innovation);
            }   
                // Check if the latter gene connection cannot be from the same layer to the same layer
            if (gene.Out.Layer == node.Layer)
                IncrementLayer(node);
            
            if (!Genes.ContainsKey(firstGene.Innovation)) // should always go to if branch
            {
                Genes.Add(firstGene.Innovation, firstGene);
                firstGene.In.OutgoingConnections.Add(firstGene);
                if (!exists)
                {
                    var g = firstGene.ShallowCopy();
                    g.Type = GeneType.AddNode;
                    allGenes.Add(g); // still points to nodes in this genome

                }
            }
            else
                ;
            if (!Genes.ContainsKey(latterGene.Innovation)) // should always go to if branch
            {
                Genes.Add(latterGene.Innovation, latterGene);
                latterGene.In.OutgoingConnections.Add(latterGene);
                if (!exists)
                {
                    var g = latterGene.ShallowCopy();
                    g.Type = GeneType.AddNode;
                    allGenes.Add(g); // still points to nodes in this genome
                }
            }
            else
                ;
            if (!Nodes.Contains(node)) // should always go to if branch
                Nodes.Add(node);
            else
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (node.Number == Nodes[i].Number)
                    {
                        if (ReferenceEquals(node, Nodes[i]))
                            ;
                        else
                            ;
                    }
                }
            }
        }

        /// <summary>
        /// Adds gene between two nodes.
        /// There cannot be two genes between same nodes.
        /// </summary>
        private void AddGene(Random rnd, ref int innovation, List<Gene> allGenes)
        {
            int latter;
            int first;
            do
            {
                    first = rnd.Next(0, Nodes.Count);
            }
            while(Nodes[first].Type == NodeType.Output);
            Node input = Nodes[first];
            Node output;
            int j = 0;
            do
            {
                latter = rnd.Next(0, Nodes.Count);
                output = Nodes[latter];
                ++j;
                if (j > 10 && input.Layer >= output.Layer)
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        if (input.Layer < Nodes[i].Layer)
                            output = Nodes[i];
                    }
            } // SHOULD NOT PICK THE SAME LAYER
            while (input.Layer >= output.Layer); 
            
            Gene gene = null;
            foreach (var item in allGenes)
            {
                if (item.In.Number == input.Number && item.Out.Number == output.Number)
                {
                    gene = item.ShallowCopy();
                    gene.In = input;
                    gene.Type = GeneType.Enabled;
                    gene.Out = output;
                    break;
                }
            }

            if (gene == null)
            {
                gene = new Gene(input, output, 0f, GeneType.Enabled, ++innovation);
                var g = gene.ShallowCopy();
                g.Type = GeneType.AddGene;
                allGenes.Add(g);
            }
            if (Genes.ContainsValue(gene))
                return;
            gene.Weight = gene.AssignRandomWeight(rnd);
            gene.In.OutgoingConnections.Add(gene);
            Genes.Add(gene.Innovation, gene);
        }

        /// <summary>
        /// Mutates Weights, adds a node, a gene if lucky ;)
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="innovation"></param>
        /// <param name="number"></param>
        /// <param name="allGenes"></param>
        /// <param name="mutWeightChance"></param>
        /// <param name="addNodeChance"></param>
        /// <param name="addGeneChance"></param>
        /// <param name="perturbeChance"></param>
        public void Mutate(Random rnd, ref int innovation, ref int number, List<Gene> allGenes,
            int mutWeightChance = 80, int addNodeChance = 3, int addGeneChance = 5, int perturbeChance = 90)
        {
            if (Genes.Count == 0)
                AddGene(rnd, ref innovation, allGenes);

            if (rnd.Next(0, 100) < mutWeightChance)
                MutateWeights(rnd, perturbeChance);

            if (rnd.Next(0, 100) < addGeneChance)
                AddGene(rnd, ref innovation, allGenes);

            if (rnd.Next(0, 100) < addNodeChance)
                AddNode(rnd, ref innovation, ref number, allGenes);
        }
    }

    public class Species
    {
        public List<Genome> Organisms;
        public float FitnessSharing { get; set; } = 0f;
        public int Stagnant { get; set; } = 0;
        public float MaxFitness { get; set; } = float.MinValue;

        public Species(List<Genome> org)
        {
            Organisms = org;
            for (int i = 0; i < org.Count; i++)
            {
                Organisms[i].Group = this;
            }
        }

        public Species()
        {
            Organisms = null;
        }

        public int Count => Organisms.Count;

        public Genome this[int index]
        {
            get
            {
                return Organisms[index];
            }
            set => Organisms[index] = value;
        }

       

            
        private void TryAddingToRemove(List<Genome> toRemove, Genome genome, int number)
        {
            if (toRemove.Count < number)
                toRemove.Add(genome);
            else
            {
                int max = -1;
                float maxVal = float.MinValue;
                for (int i = 0; i < number; i++)
                {
                    if (maxVal < toRemove[i].Fitness)
                    {
                        maxVal = toRemove[i].Fitness;
                        max = i;
                    }
                }
                if (genome.Fitness < toRemove[max].Fitness)
                    toRemove[max] = genome;
            }
        }

        public void Cull()
        {
            Organisms.RemoveAll(org => org.ToRemove == true);
        }

        /// <summary>
        /// Counts shared fitness of all organism in this species.
        /// Adds low preforming organisms to list toRemove.
        /// </summary>
        public Genome CountFitnessAndCull(Func<Genome, float> fitnessFunction, List<Genome> toRemove, int howMany, ref Genome beast)
        {
            Genome champ = new Genome(null, null, null);
            champ.Fitness = float.MinValue;
            
            for (int i = 0; i < Count; i++)
            {
                var genome = Organisms[i];
                if (beast.Fitness <= fitnessFunction(genome))
                {
                    beast = genome.ShallowCopy();
                    beast.Fitness = fitnessFunction(beast);
                }
                genome.FitnessSharingFunction(fitnessFunction);
                if (genome.Fitness >= champ.Fitness)
                    champ = genome;
                TryAddingToRemove(toRemove, genome, howMany);
            }
            if (champ.Fitness <= MaxFitness)
                Stagnant++;
            else
            {
                MaxFitness = champ.Fitness;
                Stagnant = 0;
            }
            return champ;
        }

        public void Add(Genome genome) => Organisms.Add(genome);
    }
}
