# Basic agent for Geometry Friends
Simple Geometry Friends agent to use as a baseline.
Feel free to study the code to understand how to create an agent or even continue developing from it.
There are many things that can be improved.

## How it works
The agent tries to solve a level by following these steps:
* Graph creation
* Search
* Control

### Graph creation
A graph is created at the beginning of a level and follows these steps:
* Vertex generation (one per each side of each platform of the level)
* Edge generation (directed edges linking the vertices after checking if the connection is possible)
Vertex and edge generation is very simple and not completely reliable.

### Search
Depth-First Search
* Several goals (one per diamond)
* Search stops at first goal found.
* Updates graph when searching for another goal, by removing the one that has already been reached

### Control
* If the desired position is above the agent and the agent is close to it, returns JUMP or MORPH_UP
* Else it rolls or slides to the side of the desired position
This is a very basic controller that is not sufficient to guide the agent well enough to its goal
