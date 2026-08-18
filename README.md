Piston Motion Calc v0.4

WebUI Test:
https://discosimulacrum.github.io/Piston-Motion-Calc/

This script calculates the velocity of an reciprocating engine's piston(s) during it's cycle.
It also calculates various other results, such as compression ratio. The goal is to keep elaborating on the code to create a useful kinematic model.

The output of this script can be useful for:
* Analyzing the affect of stroke, rod ratio, and max rpm on peak piston velocity and angle
* Engine Management Tuning
* Comparing different engines geometries
* Making informed decisions about operating RPM as related to longevity
* Arguing with people on car forums


Console Example:

![Console Example](https://github.com/TomBarstow/Piston-Motion-Calc/blob/main/example.png?raw=true)

CSV Output Example Visualized as Graph:

![Output Example](https://github.com/TomBarstow/Piston-Motion-Calc/blob/main/exampleGraph.png?raw=true)


Future development:
* Piston to valve (p2v) modeling
* Static and dynamic compression calculation
* Piston acceleration
* Wrist pin offset (Left out right now because it seems to have a negligible effect on piston velocity)
* GUI
