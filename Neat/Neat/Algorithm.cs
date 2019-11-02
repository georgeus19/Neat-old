using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WorldsHardestGame_UnitTests")]
namespace Neuroevolution.Neat
{

    public class Neat : IEnumerable<Genome>
    {
        /// <summary>
        /// Number of organisms in a generation. Can be set via constructor.
        /// </summary>
        int GenerationSize { get; set; } = 100;

        /// <summary>
        /// Delegate to function passed in constructor.
        /// Evaluates genome how well it performs.
        /// </summary>
        Func<Genome, float> FitnessFunction { get; }

        /// <summary>
        /// List of all organisms in a generation
        /// </summary>
        List<Species> Population;

        /// <summary>
        /// List of structural mutation that happed in the current generation.
        /// Helps to track if two same structural mutation don't have same innovation/number number.
        /// </summary>
        List<Gene> AllGenes { get; set; }

        /// <summary>
        /// Current innovation number. 
        /// </summary>
        int innovationNumber = -1;

        /// <summary>
        /// Current node id number
        /// </summary>
        int nodesNumber = -1;

        /// <summary>
        /// List of players passed in constructor.
        /// Each player is linked to one genome.
        /// </summary>
        List<object> Players { get; set; }
        
        /// <summary>
        /// The best organism of current/ previous generation
        /// </summary>
        public Genome Beast;

        /// <summary>
        /// List of best genomes of each species of current/ previous generation
        /// </summary>
        public List<Genome> Champions = new List<Genome>();

        /// <summary>
        /// used for parallel simulation but not reccommended!
        /// </summary>
        public List<Thread> threadPool = new List<Thread>();
        public List<Job> jobsPool = new List<Job>();

        Random rnd = new Random();
        int numberOfThreads;
        int currentFreeThread = 0;
        
        // compatibility par - explained in method SetParameters(...)
        // For more info visit paper on nEAT mentioned in documentation
        float c1;
        float c2;
        float c3;
        float compatibilityThreshold;
        // mutation par
        int mutationWeightsChance;
        int addGeneChance;
        int addNodeChance;

        int Mutation { get; } = 25;

        public Neat(List<Node> nodes, SortedList<int, Gene> genes,
                    Func<Genome, float> fitnessFunction, List<object> players)
        {
            GenerationSize = players.Count;
            nodesNumber = nodes.Count - 1;
            FitnessFunction = new Func<Genome, float>(fitnessFunction);
            Players = players;
            InitDefaultParameters();
            innovationNumber = genes.Count - 1;
            Beast = new Genome(null, null, null);
            Beast.Fitness = float.MinValue;
                // Create Population
            Population = new List<Species>();
            var species = new Species();
            List<Genome> genomes = new List<Genome>(GenerationSize);
            Genome genome = new Genome(nodes, genes, players[0]);
            genomes.Add(genome);
            genome.Group = species;
            Genome copy;
            for (int i = 1; i < GenerationSize; i++)
            {
                copy = genome.ShallowCopy();
                copy.Player = players[i];
                copy.Group = species;
                genomes.Add(copy);
            }
            species.Organisms = genomes;
            Population.Add(species);
        }

        public Neat(List<Node> nodes, SortedList<int, Gene> genes,
                   Func<Genome, float> fitnessFunction, List<object> players, int numberOfThreads)
        {
            GenerationSize = players.Count;
            nodesNumber = nodes.Count - 1;
            FitnessFunction = new Func<Genome, float>(fitnessFunction);
            Players = players;
            InitDefaultParameters();
            innovationNumber = genes.Count - 1;
            Beast = new Genome(null, null, null);
            Beast.Fitness = float.MinValue;
            // Create Population
            Population = new List<Species>();
            var species = new Species();
            List<Genome> genomes = new List<Genome>(GenerationSize);
            Genome genome = new Genome(nodes, genes, players[0]);
            genomes.Add(genome);
            genome.Group = species;
            Genome copy;
            for (int i = 1; i < GenerationSize; i++)
            {
                copy = genome.ShallowCopy();
                copy.Player = players[i];
                copy.Group = species;
                genomes.Add(copy);
            }

            this.numberOfThreads = numberOfThreads;
            species.Organisms = genomes;
            Population.Add(species);

            for (int i = 0; i < numberOfThreads; i++)
            {
                jobsPool.Add(new Job(null, null, 0, 0));
                threadPool.Add(new Thread(jobsPool[i].SimulateStepParallel));
            }
        }

        private void InitDefaultParameters()
        {
            c1 = 1f;
            c2 = 1f;
            c3 = 0.2f;
            compatibilityThreshold = 3f;
        }

        /// <summary>
        /// Sets parameters for compatibility computation and for mutation chances.
        /// Imperative to call this function just to set up default parameters.
        /// </summary>
        /// <param name="c1">compatibility computation - disjoint coef</param>
        /// <param name="c2">compatibility computation - excess coef</param>
        /// <param name="c3">compatibility computation - weight difference coef</param>
        /// <param name="compThreshold">threshold from compatibility computation</param>
        /// <param name="mutWeightsChance"></param>
        /// <param name="addGeneChance"></param>
        /// <param name="addNodeChance"></param>
        /// <param name="justMutation">% how many from old gen should just mutate to get to new generation</param>
        public void SetParameters(float c1 = 1f, float c2 = 1f, float c3 = 0.2f, float compThreshold = 3f, int mutWeightsChance = 80,
                                int addGeneChance = 5, int addNodeChance = 3, int justMutation = 25)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
            this.compatibilityThreshold = compThreshold;
            this.mutationWeightsChance = mutWeightsChance;
            this.addGeneChance = addGeneChance;
            this.addNodeChance = addNodeChance;

        }

        /// <summary>
        /// For each organisms in generation the function feeds it's neural network the inputs passed in argument and returns the outputs
        /// </summary>
        public List<List<float>> SimulateStepFromNeat(List<List<float>> parameters)
        {
            List<List<float>> results = new List<List<float>>(GenerationSize);
            List<float> genomeRes = new List<float>();
            int par = 0;
            for (int i = 0; i < Population.Count; i++)
            {
                for (int j = 0; j < Population[i].Count; j++)
                {
                    results.Add(Population[i][j].ActivateNetwork(parameters[par]));
                        // alternative below
                    //Population[i][j].SetInputs(parameters[par]);
                    //results.Add(Population[i][j].ComputeOutputs());
                    ++par;
                }
            }
            return results;
        }

        public class Job
        {
            public List<Species> Population { get; set; }
            public List<List<float>> Parameters { get; set; }
            public int From { get; set; }
            public int To { get; set; }
            public List<List<float>> Result = new List<List<float>>();

            public Job(List<Species> population, List<List<float>> parameters, int from, int to)
            {
                Population = population;
                Parameters = parameters;
                From = from;
                To = to;
            }

            public void SimulateStepParallel()
            {
                Result = new List<List<float>>(To - From);
                List<float> genomeRes = new List<float>();
                int before = 0;
                int index = -1;
                while (before < From)
                {
                    index++;
                    before += Population[index].Count;
                }
                before -= Population[index].Count;
                int fromGenome = From - before;
                for (int i = index; i < Population.Count; i++)
                {
                    for (int j = fromGenome; j < Population[i].Count; j++)
                    {
                        if (From == To)
                            return;
                        Result.Add(Population[i][j].ActivateNetwork(Parameters[From]));
                        From++;
                    }
                }
            }
        }

        public Tuple<Job, Thread> SimulateStepFromNeatParallel(List<List<float>> parameters, int from, int to)
        {
            if (threadPool.Count == 0)
                throw new InvalidOperationException();
            var job = jobsPool[currentFreeThread];
            job.Population = Population;
            job.Parameters = parameters;
            job.From = from;
            job.To = to;
            threadPool[currentFreeThread].Start();
            currentFreeThread++;
            return new Tuple<Job, Thread>(job, threadPool[currentFreeThread - 1]);
        }

        public List<List<float>> SimulateStepFromTo(List<List<float>> parameters, int from, int to)
        {
            var result = new List<List<float>>(to - from);
            List<float> genomeRes = new List<float>();
            int before = 0;
            int index = -1;
            while (before < from)
            {
                index++;
                before += Population[index].Count;
            }
            if (index != -1)
                before -= Population[index].Count;
            int fromGenome = from - before;
            for (int i = index; i < Population.Count; i++)
            {
                for (int j = fromGenome; j < Population[i].Count; j++)
                {
                    if (from == to)
                        return result;
                    result.Add(Population[i][j].ActivateNetwork(parameters[from]));
                    from++;
                }
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Count fitness of all organisms in current generation.
        /// Also removes worst performing organisms
        /// </summary>
        /// <returns>The best genome of each species</returns>
        internal List<Genome> CountFitness()
        {
            List<Genome> champions = new List<Genome>();
            int cullCount = GenerationSize / 3;
            var toRemove = new List<Genome>();
            for (int i = 0; i < Population.Count; i++)
            {
                bool add = false;
                if (Population[i].Count > 5)
                    add = true;
                var champ = Population[i].CountFitnessAndCull(FitnessFunction, toRemove, cullCount, ref Beast);
                if (add) // Can be commented for each champ to be included...
                    champions.Add(champ);
            }
            ChampionsCheck(champions);
            Cull(toRemove);
            champions.RemoveAll(org => org.ToRemove == true || org.Group.Stagnant >= 15); 
            return champions;
        }

        // Checks if the removal is correct
        private bool CheckToRemove(List<Genome> toRemove)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                for (int j = 0; j < Population.Count; j++)
                {
                    for (int k = 0; k < Population[j].Count; k++)
                    {
                        if (toRemove[i].Fitness > Population[j][k].Fitness)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes genomes passed in arg from population
        /// </summary>
        private void Cull(List<Genome> toRemove)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                toRemove[i].ToRemove = true;
            }
            for (int i = 0; i < Population.Count; i++)
            {
                Population[i].Cull();
            }
        }
        
        /// <summary>
        /// Counts how compatible the two genomes in agrs are.
        /// Depends on number of different gene connection and average weight difference in the same genes
        /// </summary>
        internal float CountCompatibility(Genome g1, Genome g2)
        {
            int N = ((g1.Genes.Count > g2.Genes.Count) ? g1.Genes.Count : g2.Genes.Count) - 20;
            if (N < 1)
                N = 1;
            float W = 0f;
            int E = 0, D = 0;
            int matching = 0;
            var enumG1 = g1.Genes.GetEnumerator();
            var enumG2 = g2.Genes.GetEnumerator();
            Gene curr1;
            Gene curr2;
            curr1 = enumG1.MoveNext() ? enumG1.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
            curr2 = enumG2.MoveNext() ? enumG2.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
            while (true)
            {
                if (curr1.Innovation == curr2.Innovation)
                {
                    if (curr1.Innovation == -1)
                        break;
                    W += (float)Math.Abs(curr1.Weight - curr2.Weight);
                    if (W > 0)
                        ;
                    matching++;
                    curr1 = enumG1.MoveNext() ? enumG1.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    curr2 = enumG2.MoveNext() ? enumG2.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                }
                else if (curr1.Innovation < curr2.Innovation)
                {
                    if (curr1.Innovation == -1)
                    {
                        E++;
                        curr2 = enumG2.MoveNext() ? enumG2.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                    else
                    {
                        D++;
                        curr1 = enumG1.MoveNext() ? enumG1.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                }
                else
                {
                    if (curr2.Innovation == -1)
                    {
                        E++;
                        curr1 = enumG1.MoveNext() ? enumG1.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                    else
                    {
                        D++;
                        curr2 = enumG2.MoveNext() ? enumG2.Current.Value : new Gene(null, null, 0f, GeneType.Enabled, -1);
                    }
                }
            }
            if (matching == 0)
                matching = 1;
            var res = (c1 * E) / N + (c2 * D) / N + c3 * ( W / matching );
            return res;
        }

        // Puts the fitter genome into g1, the other into g2
        private bool CompareFitnessCorrectOrder(Genome g1, Genome g2)
        {
            bool sameFitness = false;
            Genome tmp;
            if (g1.Fitness > g2.Fitness)
            {
                tmp = g1;
                g1 = g2;
                g2 = tmp;
            }
            else if (g1.Fitness < g2.Fitness)
            {
                tmp = g2;
                g2 = g1;
                g1 = tmp;
            }
            else
                sameFitness = true;
            return sameFitness;
        }

        internal List<Node> CopyList(List<Node> toCopy)
        {
            var res = new List<Node>(toCopy.Count);
            for (int i = 0; i < toCopy.Count; i++)
            {
                res.Add(toCopy[i].ShallowCopy());
            }
            return res;
        }
      
        /// <summary>
        /// Return a child of two genomes what were in the previous generation
        /// </summary>
        /// <param name="interspeciesMatingRate">How big is chance that two genomes are from same generation</param>
        /// <returns></returns>
        private Genome GetCrossoverOffspring(Random rnd, int interspeciesMatingRate = 99)
        {
            int speciesIndex;
            speciesIndex = rnd.Next(0, Population.Count);
           
            Genome g1 = Population[speciesIndex][rnd.Next(0, Population[speciesIndex].Count)];
            Genome g2;
            if (rnd.Next(0, 100) > interspeciesMatingRate)
            {
                int speciesIndex2;
                do
                {
                    speciesIndex2 = rnd.Next(0, Population.Count);
                    g2 = Population[speciesIndex2][rnd.Next(0, Population[speciesIndex2].Count)];
                }
                while (Population.Count > 2 && speciesIndex != speciesIndex2);
            }
            else
            {
                g2 = Population[speciesIndex][rnd.Next(0, Population[speciesIndex].Count)];
            }
            bool sameFitness = CompareFitnessCorrectOrder(g1, g2);
            return g1.Crossover(g2, rnd, sameFitness);
        }

        /// <summary>
        /// Returns true if two genomes are compatible enough to be in same species
        /// </summary>
        private bool Compatible(Genome g1, Genome g2) => CountCompatibility(g1, g2) < compatibilityThreshold ? true : false;

        /// <summary>
        /// Inserts genome in arg into one of current species.
        /// If it does not fit anywhere, new species is created
        /// </summary>
        /// <param name="newPopulation">new population of genomes that will happen to be next generation of organisms</param>
        private void InsertIntoSpecies(Genome genome, List<Species> newPopulation)
        {
            bool foundSpecies = false;
            for (int j = 0; j < newPopulation.Count; j++)
            {
                if (Compatible(newPopulation[j][0], genome))
                {
                    foundSpecies = true;
                    newPopulation[j].Add(genome);
                    genome.Group = newPopulation[j];
                    break;
                }
            }
            if (!foundSpecies)
            {
                Species species = new Species(new List<Genome>());
                species.Add(genome);
                newPopulation.Add(species);
                genome.Group = species;
            }
        }

        private void ChampionsCheck(List<Genome> newChamps)
        {
            for (int i = 0; i < Champions.Count; i++)
            {
                bool a = false;
                for (int j = 0; j < Population.Count; j++)
                {
                    for (int k = 0; k < Population[i].Count; k++)
                    {
                        if (FitnessFunction(Champions[i]) == FitnessFunction(Population[i][k]))
                            a = true;
                    }
                    
                }
                if (a == false)
                    ;
            }
        }

        /// <summary>
        /// Adds the fittest genome of last generation into new one
        /// </summary>
        /// <param name="newPopulation"></param>
        private void AddBeast(List<Species> newPopulation)
        {
            var tmp = new List<Genome>();
            Genome champ = Beast;
            tmp.Add(champ);
            float maxFitness = champ.Fitness;
            int stagnant = champ.Group.Stagnant;
            Species species = new Species(tmp);
            species.Stagnant = stagnant;
            species.MaxFitness = maxFitness;
            champ.Group = species;
            InsertIntoSpecies(Beast, newPopulation);
        }

        /// <summary>
        /// Count fitness of all org and removes worst perfotming and Creates new generation from the old one.
        /// Genome that is added to new gen can be champion of one of the species,
        /// mutated genome from last gen or a child of genomes from last gen.
        /// This function also links new genomes to players correctly.
        /// </summary>
        public void CreateNewGeneration()
        {
            AllGenes = new List<Gene>();
            var newPopulation = new List<Species>();
            var champions = CountFitness();
            Champions = champions;
            Population.RemoveAll(sp => sp.Count == 0 || sp.Stagnant >= 15);
            for (int i = 0; i < champions.Count; i++)
            {
                var tmp = new List<Genome>();
                Genome champ = champions[i].ShallowCopy();
                tmp.Add(champ);
                float maxFitness = champ.Fitness;
                int stagnant = champ.Group.Stagnant;
                Species species = new Species(tmp);
                species.Stagnant = stagnant;
                species.MaxFitness = maxFitness;
                champ.Group = species;

                newPopulation.Add(species);
            }
            AddBeast(newPopulation); // maybe try insesting beast into population species?
            int justMutationCount = (int)(GenerationSize * (Mutation / 100.0));
            for (int i = 0; i < GenerationSize - champions.Count - 1 - justMutationCount; i++)
            {
                Genome genome = GetCrossoverOffspring(rnd);
                genome.Mutate(rnd, ref innovationNumber, ref nodesNumber, AllGenes, mutationWeightsChance, addNodeChance, addGeneChance);
                InsertIntoSpecies(genome, newPopulation);
            }
            for (int i = 0; i < justMutationCount; i++)
            {
                int speciesIndex = rnd.Next(0, Population.Count);
                Genome genome = Population[speciesIndex][rnd.Next(0, Population[speciesIndex].Count)].ShallowCopy();
                genome.Mutate(rnd, ref innovationNumber, ref nodesNumber, AllGenes, mutationWeightsChance, addNodeChance, addGeneChance);
                InsertIntoSpecies(genome, newPopulation);
            }
            Population = newPopulation;
            int playerIndex = 0;
            // zarucuje spravne namatchovani na hrace
            for (int i = 0; i < Population.Count; i++)
            {
                for (int j = 0; j < Population[i].Count; j++)
                {
                    Population[i][j].Player = Players[playerIndex];
                    playerIndex++;
                }
            }
        }

        public IEnumerator<Genome> GetEnumerator()
        {
            for (int i = 0; i < Population.Count; i++)
            {
                for (int j = 0; j < Population[i].Count; j++)
                {
                    yield return Population[i][j];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}