# FG23GP Assignment 2

## Basic info
- Name: Brandon Swee Yee  
- Link: https://github.com/bsweeyee/fg23gp-assignment.git  
- Unity version: 2022.3.9f1  
- Assignment: 2 - ILander game  

## Mechanics and goals
You control a frog stuck at a bottom of a well.  
- Your goal is to **escape this well by going up**, landing on platforms with a combination of jumping and flying controls  
- You have a **limited amount of energy** to fly and jump so make sure to spend them correctly.  
- **Control your speed**, do not bump onto walls too quickly and make sure to land slowly or you will die!  

## Game Controls:  
- Press and hold _A_ to fly left  
- Press and hold _D_ to fly right  
- Hold _Space_ to charge jump  
- Release _Space_ to jump  
- Press _ESC_ to pause game  
- Interact with UI using the mouse  

## Additional packages
- New Unity Input System
- 2D Tilemaps Editor

Both of the above packages should be installed when you load the project. If not, you can install it from Unity's Package Manager  

## Playing the game ( From Unity )
- Go to Scenes folder
- Go to scene titled "Entrypoint"
- Play scene

## Developer information
### Game Settings
This file contains all the information required to initialize the game correctly. This includes:  
- Window size ( for build )
- Controller prefabs
- UI prefabs
- Level prefabs
- Obstacle prefabs
- Particle prefabs
- Physics layers
  
You can find this file in the **Data/Resources/** folder

### Levels
#### General
In the **GameSettings**, under the "Level" section you will see an array called "Level Data"  
The number of levels in this game is defined by the size of this array  
  
In each entry you define the following:
- Level ID: This is the level number. First level starts from index 0
- Start Block: The prefab that is initialized before the first set of PlatformBlocks
- End Block: The prefab that is initialized after the last set of PlatformBlocks
- Platform Blocks: An array of platform prefabs instantiated in order of entries

You can find all PlatformBlocks from the **Prefabs/PlatformGroups** folder

Each level is made up of level blocks.  
Each level block is a prefab found in the **Prefabs/PlatformGroups** folder

#### Creating levels
To create a level, the main things to create are **Platforms** and the **Walls**.

To create platforms:
1. Create / Select an existing PlatformGroup
2. Create a Rectangular Tilemap by Right clicking -> 2D Object -> Tilemap -> Rectangular
3. Rename the "Grid" to "Platform"
4. Open the Tile Palette by going to the top menu bar under, Window -> 2D -> Tile Palette
5. Highlight the Gameobject and select the sprite you want from the Tile Palette and start creating platforms
6. After finishing, attach the **PlatformGenerator** script to the GameObject you renamed to "Platform"
7. Set Axis Direction to **'X'**

To create walls:
1. Create / Select an existing PlatformGroup
2. Create / Select a Rectangular Tilemap. You can create a Tilemap by right clicking -> 2D Object -> Tilemap -> Rectangular
3. Rename the "Grid" to "Wall"
4. Open the Tile Palette by going to the top menu bar under, Window -> 2D -> Tile Palette
5. Highlight the Gameobject and select the sprite you want from the Tile Palette and start creating walls
6. After finishing, attach the **PlatformGenerator** script to the GameObject you renamed to "Platform"
7. Set Axis Direction to **'Y'**

Differences between platforms and walls:  
- Create walls only if they are tiles to be linked in a vertical direction.
- Create platforms only if they are tiles to be linked in a horizontal direction.

#### Changing Spawn points
![change_spawn.png](https://github.com/bsweeyee/fg23gp-assignment/blob/main/images/changing_spawn_points.png)
After the PlatformGenerator script, you will see additional Transform Gizmos appearing.  
Each set of linked platform is a checkpoint in this game.  

Modify the position where the player will respawn by moving this Transform Gizmo

### Debugging and Testing
#### Respawning
![respawn.png](https://github.com/bsweeyee/fg23gp-assignment/blob/main/images/respawning.png)
While running the game in the editor, you might want to respawn at a specific point to test out a specifc part of the game.  
You can do this by tapping on red spheres that appear where you set the respawn points for each platform

#### Debug information
You can see some additional debug information and debug tools ingame by adding the following Preprocessor Directives in the Player Setting:  
**DISPLAY_DEBUG_MENU**

## Code structure

### Single Entry point initialization
For this project, I have designed the Code Flow from a single initialization script called **Init.cs**  
This script instantiates all the necessary prefabs from a ScriptableObject data file called **GameSettings**  
  
**Game.cs** is the main game script that initializes and updates all other scripts because it is the _ONLY_ script that contains the following Unity Events:
- Start
- Awake
- Update
- FixedUpdate  
  
All other scripts will have to implement one or more of the following interfaces from **IBaseEntity.cs** to run any Initialization or Update functionalities:
- IGameInitializeEntity
- IGameTickEntity
- ILevelPlayEntity
- ILevelStartEntity
- ILevelPauseEntity
- ILevelCompleteEntity
- ILevelEndEntity
- ILevelTitleEntity

There are 2 main reasons why I decided to structure the code this way:
1. I wanted to practice the Game flow and Interface techniques taught by Sebastian
2. I wanted to have explicit control over order of execution. Especially for physics objects. Using this interface method, I was able to defer physics update AFTER game logic updates since I had written my own simple physics scripts. This will be messy if I had used Monobehaviour's Unity Events over different scripts.

### Game State
This game uses the **State Pattern** to control game flow.  
Depending on the game's state, different scripts and code snippets are ran.  

There are 2 main reasons why I decided to structure the code this way:
1. I wanted to practice the State Pattern in an OOP way as described by the Game Programming Pattern book by Bob Nystrom
2. The game was simple enough to be controlled fully by a state machine and it helps me in debugging certain pieces of code.

## Inspirations
- Mechanics Inspiration: https://www.pomelogames.com/mars-mars/
- Concept inspiration: https://en.wikibooks.org/wiki/Chinese_Stories/The_frog_of_the_well

## References and assets
- Collision detection video by Sebastian Lague: https://youtu.be/OBtaLCmJexk?si=kcYwC_1ONQlgj2FS
- Shader based screen transition by DDRKirby: https://ddrkirby.com/articles/shader-based-transitions/shader-based-transitions.html
- Chevy Ray's Pixel Fonts by Chevy Ray: https://chevyray.itch.io/pixel-fonts
- Kenny's Assets
    - https://www.kenney.nl/assets/1-bit-platformer-pack
    - https://www.kenney.nl/assets/1-bit-input-prompts-pixel-16
    - https://www.kenney.nl/assets/game-icons
