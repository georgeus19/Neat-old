# Neat learning - documentation
The assignment was to implement an AI that can learn and solve complex task and to create a testing game to be solved by AI. The AI learns through Neat algorithm - `NeuroEvolution of Augmenting Topologies`. Neat consists of a typical `genetic algorithm` approach (it simulates behavior of multiple organisms in each generation and selects the fittest ones to be a part of next generation) and `neuroEvolution` approach (it uses neural networks to determine behavior based on input data).
## Tests
### XOR 
A good way to test the functionality of AI is to make it solve a simple task such as solving XOR (there are 2 inputs and the output is XOR of the inputs).

In 100 runs with 150 organisms in each generation, the Neat found suitable neural network for XOR in an average of 60 generations. It did not fail once. The best performance took 11 generations, the worst 188.
### Google Dinosaur game
In this game the player controls a dinosaur that has to evade obstacles that come to it from the right. The dinosaur can only jump and duck and the obstacles keep speeding up while the game progresses.
### World's hardest game
In this game the player controls a square with keys up, down, left, right. The goal is to get to end area that is marked green (same as starting area). Contact with blue balls kills the square.

## Program
### Interface
#### Whole App Interface
The app uses WinForms as interface. After opening there are 5 buttons : WHG-play, WHG-simulation, Dino-play, Dino-Simulation, XOR. WHG means World's hardest game, Dino means Google Dinosaur game and by clicking on corresponding button the corresponding game can be played or simulation can be commenced. 
The XOR button runs XOR simulation that consists of 100 runs. The generation count need for each run to complete is stored in file **xor.txt** as well as average generation count in the 100 runs.
At any point, if you want to get to the menu with 5 buttons, just press Esc.
### World's hardest game
Player can be controlled by pressing UP-ARROW (go up), DOWN-ARROW (go down), RIGHT-ARROW (go right), LEFT-ARROW(go left).
### Google Dinosaur game
The player/dinosaur can be controlled by pressing UP-ARROW (do a big jump), LEFT-ARROW (do a small jump) and DOWN-ARROW (duck). The score is visible in the upper part of window.
#### Neat Library Interface
Simulation can be run from **class Neat**. The initial neural network must be passed in the constructor of class Neat as well as `List<object>` with players and fitness function delegate.
The fitness function must have as an one parameter with type `Genome` but each genome contains a reference to an object instance of one of given players. 

```csharp
public Neat(List<Node> nodes, SortedList<int, Gene> genes, Func<Genome, float> fitnessFunction, List<object> players)
```
**Class Node** represents nodes in neural network. Input nodes have in property `Type` set **NodeType.Sensor**, output nodes **NodeType.Output** and hidden nodes **NodeType.Hidden**. Each node must be given an unique id that is stored in property `Number`.

**Class Gene** represents a connection between two nodes in neural network. Each gene must be given an **unique innovation number**. To correctly create `Sortedlist<int, Gene>` genes in Neat.ctor the first `int` must be an innovation number of the corresponding gene. 

Function **SimulateStepFromNeat** is used for getting outputs of network which is fed the given inputs passed in parameter.
```csharp
public List<List<float>> SimulateStepFromNeat(List<List<float>> parameters)
```
Function **CreateNewGeneration** creates new generation of organisms from the old one.
```csharp
public void CreateNewGeneration()
```
It is imperative to call function **SetParameters** even if you don't want to change them.
### Backend
#### World's hardest game
This game/simulation is run using a timer. In each tick, the movement of all game objects occur as well as collision detection.
##### Game itself
**Class Game** is the main class of the game. In its constructor the images of game objects are loaded and game objects are created. The movement of all elements is done in **MoveElements**.  Collision Detection between player and all enemy objects is done in function **CheckBallPlayerCollision**. The drawing of game map and objects is done via **PrintMap** function which is called when objects are drawn on different place than where they currently are.

##### Simulation
The game part of simulation uses overloaded version of functions mentioned above except collision detection function is called **CheckBallPlayerSCollision**.

After the button click, the class Neat is created. One generation run takes 10 seconds. In each tick of timer **Game.SimulateStep** is called which creates parameters for the neural network, calls **SimulateFromNeat** to retrive results which the function returns. The results are fed to **MoveElements** function. Once the time is up, the game is reseted and **Neat.CreateNewGeneration** is called to create new generation. **Fitness function** measures the distance to end area - the closer the better.
#### Google Dinosaur Game
Both the game and the simulation use timer to move objects and simulate steps. The game is created by calling constructor of class Game which loads images . The movement of objects is done in **MoveElements** function. Collision detection is done in **CheckCollision**. (There are 2 overloads - game, simulation) Function **ManageObstacles** constantly adds obstacles on the right side of the window. Inputs for the NN are computed in **CountParameters** method. This simulation also uses **Neat.SimulateStepFromNeat** that computes the outputs of NN which are fed to function **MoveElements**.

#### XOR 
The simulation runs in the **RunSimulation** function. This function runs the test 100 times. One run is done, if the neural network can output correct solution to each input. The **fitness function** is the squared length of distance to correct solution (Cost function).
#### NEAT
The main functions are **SimulateStepFromNeat** and **CreateNewGeneration**.
**SimulateStepFromNeat** takes the inputs to NN as parameters and for each genome/ player calls function **ActivateNetwork** which feeds the inputs to network and compute outputs which it returns. For parallel use it is possible to use function **SimulateStepFromTo** which only takes in account genomes from given `from` to `to` range (but function itself is not parallel). For run in other thread it is possible to use function **SimulateStepFromNeatParallel** which also takes `from` and `to` in parameter but to have correct functionality it returns the thread which runs the simulation step and class with results (not reccommended to use since it may not give much bonus speed).

The Genomes are divided between species, because it allows them to mutate slightly and stay long enough to actually use the mutation if it is worth it. If they were not in species and a genome mutated/ or was created by crossover of other two genomes, it would not survive long enough to perfect itself. That way local minima can be overcome.

Two genomes are in one species if they are similar to each other with regard to number of different Genes and different values in the same Genes (compatibility is computed by **CountCompatibility** function). 

**CreateNewGeneration** creates new generation from the previous one. Firstly, it computes fitness of all genomes and removes the worst performing one (by using function **CountFitness**). Then it pushes the champions of each species to next generation. The other genomes that will be added to next generation are created by either **Crossover & Mutation** or just **Mutation**. Function **Genome.Mutate** can mutate the values on Genes connection or it can add a node gene or add a gene connection. **Genome.Crossover** creates a new genome from 2 parents by taking the different Genes (the genes which are only included in the fitter parent) from fitter one and at random from either parent Genes which are in both of them. All Genomes are then put into their respective species (or if none found, they create their own).

## User Interface
After opening the application, a menu with 5 buttons pops up. The buttons are : WHG-play, WHG-simulation, Dino-play, Dino-Simulation, XOR. WHG means World's hardest game, Dino means Google Dinosaur game and by clicking on corresponding button the corresponding game can be played or simulation can be commenced. 
The XOR button runs XOR simulation that consists of 100 runs. The generation count need for each run to complete is stored in file **xor.txt** as well as average generation count in the 100 runs.
At any point, if you want to get to the menu with 5 buttons, just press Esc.

### World's hardest game
Player can be controlled by pressing UP-ARROW (go up), DOWN-ARROW (go down), RIGHT-ARROW (go right), LEFT-ARROW(go left).
### Google Dinosaur game
The player/dinosaur can be controlled by pressing UP-ARROW (do a big jump), LEFT-ARROW (do a small jump) and DOWN-ARROW (duck). The score is visible in the upper part of window.
## Conclusion
The Neat algorithm took me almost a month to implement since there are almost no resources available online apart from one paper which I tried to implement it from. So basically it is how I understood the text and by trial and error tuned it into way that would actually make sense... 

link to paper -> https://drive.google.com/viewerng/viewer?url=http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf



