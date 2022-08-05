# GroundKontrol &nbsp; <img src="./rocket.svg" height="40" alt="rocket icon"/> &nbsp; <img src="./knob.svg" height="40" alt="knob icon"/> &nbsp; <img src="./sliders.svg" height="40" alt="slider icon" />

GroundKontrol is a library that aims to make it easy to tweak and tune your Unity game's constants and magic numbers using a [Korg NanoKontrol 2](https://www.amazon.com/Korg-nanoKONTROL2-Slim-Line-Control-Surface/dp/B004M8UZS8) MIDI controller.


<img src="./window.png" />

It was introduced during the [Tech Toolbox panel at the 2019 Independent Games Summit](https://www.youtube.com/watch?v=-aXrLvdrnao&t=23m51s). 

## Installation

Clone this git repo, and copy the `GroundKontrol` folder into your Unity project's `Assets/Plugins` folder.

If you have a Korg NanoKontrol 2 hooked up to your computer, and haven't changed its mappings using its desktop tool, it should just work.

**I'm interested in making this better**, but I'm new to distributing open-source libraries for Unity. If you're feeling generous, feel free to open a Pull Request or open an Issue to let me know how to make installation easier or more standard!

## Usage

There are two ways to use GroundKontrol.

### The GroundKontrol Editor Window

Select Window -> GroundKontrol from Unity's menubar. 

To wire up a given knob or slider to a specific variable, click "Add New" for that knob/slider. Drag in the GameObject or prefab you want to modify, then pick the component and property from the dropdown.

**Range** specifies what numbers the knob/slider's values will represent. When the slider or knob is at its lowest point, it will be zero; `range` specifies what the top end will be.

You can wire up multiple mappings to the same single knob or slider.


### The `MidiController` component / inspector window

<img src="./panel.png" />

When you add a mapping via the editor window, it adds a `MidiController` component to that GameObject. If you open the inspector panel for a given object or prefab, the `MidiController` component has its own custom inspector UI that also lets you wire up new mappings that way. 

You can also manually add the `MidiController` component to any object and set things that way.

Changes made in the inspector panel will take effect in the window, and vice versa.

**Warning**: If you have an object open in the inspector, and make changes in the editor window that affect that object, there will be unexpected and undesirable behavior. Make sure to close your inspector pane before opening the GroundKontrol editor window!

### Runtime

Once you click play, things will just work. While the game is running, you should be able to move the knobs/sliders and watch the game update in real-time. 

You can also update the bindings at run-time, and it should still work, although that's relatively untested behavior.

As long as you are holding down the square "stop" button on the controller (TODO: this might be the wrong button!), your inputs won't affect the game. This is useful to "zero out" the controls: since the knobs and sliders both have discrete stop and end points, this exists to let you reset their physical state if you've moved them all the way to the top/bottom but want to keep going in that direction.


### Saving Data Back

When you exit play mode in Unity, GroundKontrol attempts to save all changes you've made via its controls back to the objects you set them on. 

If the objects whose values you are changing are GameObjects directly placed in a scene in the Unity window, this should be straight-forward. If you're modifying a prefab, there's a chance things will get hairy (since GroundKontrol needs to take values changed on individual instances and save them back to the original prefab). 

My apologies if this breaks in ugly ways! In the future, I have plans to build out a configuration system that makes it easier to more explicitly save and load discrete value sets. In the meanwhile, this tool may be more useful in early exploratory stages, and/or on projects where you're confident you have everything up-to-date in version control before you begin tuning.


## Development

If you want to hack on GroundKontrol, you should be able to just edit the files directly.

Because Unity doesn't (to my knowledge) know how to deal with assets that aren't within its project structure, there are two separate copies of the library: the one that lives in the root of this project, and one that lives in `Sample/Assets/Plugins/GroundKontrol`. 

For development purposes, it's probably easier to make changes to the copy within `Sample`. If you have the Sample project open in Unity, you'll be able to see your changes live in real-time.

When you're happy with your changes and want to commit them, run `npm run sync` to copy those changes back to the root copy (or just copy/paste them). This isn't great (and means there's duplicate copies of everything in git) but it seems like the least-bad solution for now to enable a functioning dev environment.

If the concept of running `npm run sync` seems like nonsense to you: you should follow the instructions at https://nodejs.org to install Node.js ("npm" is the "node.js package manager", used under the hood by the new Unity Package Manager). From there, run that command from a terminal window in the root of this project directory. I'm not sure what this process looks like on Windows, so feel free to open a PR or drop me a line if there's subtlety needed there.

## License

GroundKontrol is licensed under the MIT license. See the LICENSE file in this repo for more information.

## Contact

**Em Lazer-Walker**

* https://github.com/lazerwalker
* https://twitter.com/lazerwalker
