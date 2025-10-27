# Weighted Spawning Sample

Everyone needs code samples nowadays. Well, why not make it useful? This repo contains the code to allow you to smartly spawn actors in the context of a game using a minimal set of designer-friendly parameters.

### The Problem

Lets take the idea of spawning a bunch of enemies that converge on the player. There are a few things we want to accomplish.
- Set what enemies may appear and, be able to modify how commonly they appear.
- Scale the difficulty of the waves of enemies based on a number.
- Changing/setting a difficulty should never require a new list of enemies. It should all be derived from the same set of data.

### Solution

We use an algorithm that solves the knapsack problem. In this algorithm, we have a knapsack with a fixed size. We want to store as many items as possible in the knapsack. The twist is that each item has a value associated with it, and we want to maximize that value.

In the context of game design, this will manifest as a few things to play with.
- A list of items to pick from.
- Each item has a weight representing the amount of space it takes up. More weight == less room for items.
- Each item has a value representing how valuable or difficult the given item is. Generally, more weight == more value. In cases where two items are evenly weighted, the algorithm will choose the higher valued one.

## Getting Started

### Integrating into an existing Unity project (Easiest)

To get started, download the _WeightedSpawning.unitypackage_ and import it directly into your Unity project. It contains the example scene and the algorithm in a separate scripts folder.

### Cloning

Start by cloning/downloading the project into a directory. Then, open it using the UnityHub application.

__NOTE: Unity Hub may get angry about the version. This code will run in any Unity version past Unity 6.1 (And likely earlier than that). Use whatever version you want.__

## What am I looking at?

The project includes a few things.

1. An example scene contains two game objects that you care about. __SPAWNER (Evenly weighted)__ and __SPAWNER (Duplicates valued less)__. You can play with the item lists and capacities. Both are fine to have enabled and can be changed at runtime without issue. Pressing play will have them spawn colored cubes over time as a visual example of the "enemies" you're spawning.

2. Importantly, the _WeightedSpawner_ behaviour is what's driving the spawning. You can see the results of any given capacity and items by pressing the __Solve__ button. Displaying the input weights, values, resulting knapsack value, and the items that were selected.

3. Within the scripts folder is a _"Knapsack.cs"_ file. This is where the algorithim is, and is the only file you need to integrate this solution into any of your own projects.

In this project, the blue cubes are the least weighted and least valuable. Greens are in the middle, and reds are slow, weighty cubes with a lot of value. This is important because you'll notice that there are two different spawners.

- __SPAWNER (Evenly weighted)__ has fixed weights and values to each enemy cube type. The capacity of the knapsack is the only thing driving variation. 

- __SPAWNER (Duplicates valued less)__ has fixed weights for each enemy cube type. But it also lowers the value of repeated uses of the enemy cubes in the list. E.g. the first blue cubes are valued at 5, then 4, 3, 2, and finally 1. This is true for the greens and reds as well, which encourages the algorithm to pick a variation of cubes that still increases in difficulty, unlike the other spawner, which fits as many reds, then greens, then blues, as possible.

## Real Application

This is the same approach to spawning that is used in [Vox Machinae's](https://store.steampowered.com/app/334540/Vox_Machinae/) Bot Stomp game mode, where players rush to clear enemy waves as quickly as they can before a timer expires.

Spawning in this mode has each grinder (mech) with its own fixed weight. The value attached to the grinder reduces each time it is picked for a wave. This decrease is not the same for each grinder type and was carefully selected.

On top of that, players choose a difficulty for the game mode. That difficulty sets a base number for the capacity of the knapsack. We also add the number of players * difficulty coefficient to that base capacity. Then, as the match progresses, we again increase the capacity.
If you were to code this using the solver in the project, you might end up with a little something like...
```c#
// Something to contain the weights and values of a given grinder.
class SpawnableGrinderEntry
{
	public int Weight;
	public int Value;
	public Object Prefab;
}

const int MAX_CAPACITY = 128;
const int MAX_ITEMS = 128;
// This is the solver in the project.
static Knapsack.Solver<Object> Solver = new Knapsack.Solver<Object>(MAX_CAPACITY, MAX_ITEMS);

// When the level loads, we give the solver our working set of grinders. Repeated calls won't require
// rebuilding the solver's internal list of items.
void OnLevelLoaded()
{
	foreach (SpawnableGrinderEntry grinder in GetGrinderList()) Solver.Add(grinder.Weight, grinder.Value, grinder.Prefab);
}

// Spawn the wave by dynamically deriving capacity from difficulty, increasing as the match progresses. 
void SpawnWave()
{
	int capacity = GetDifficulty() + PlayerCount() * GetDifficultyCoefficient() + GetCurrentWave() * GetDifficultyCoefficient();
	m_Solver.Solve(capacity);
	for (int i = 0; i < Solver.Length; i++) SpawnGrinderFrom(Solver[i]);
}
```