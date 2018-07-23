# Frixel
*Frame x Pixel* 

![Alt Text](https://github.com/EmilPoulsen/Frixel/blob/master/doc/logo_100x100.png)

Frixel is a finite element analysis program that’s so easy a 5 year old could use it. Provided only a closed curve representing a building massing and location for core, Frixel generates a 2 dimensional grid and runs structural analysis on it. You can tweak grid size, gravitation magnitude and lateral wind force to see how your design perfoms under different conditions. Additionally, it can run structural topology optimization to improve its mechanical performance by adding bracing in appropriate places.

User Interface             |  Building analysis
:-------------------------:|:-------------------------:
![Alt Text](https://github.com/EmilPoulsen/Frixel/blob/master/doc/Frixel_D.gif)  |  ![Alt Text](https://github.com/EmilPoulsen/Frixel/blob/master/doc/Frixel_A.gif)
**Cantilever analysis** | **Optimization**
![Alt Text](https://github.com/EmilPoulsen/Frixel/blob/master/doc/Frixel_C.gif)  |  ![Alt Text](https://github.com/EmilPoulsen/Frixel/blob/master/doc/Frixel_B.gif)

## Functionality
- A power-up for architect-engineer collaboration.
- A great way to test and guide options in early stage design.
- Fast. You can quickly get an understanding of structural behavior.
- Topology optimization for minimal displacement.
- Platform agnostic

## Context
Frixel was developed from scratch under 24 intense hours at the [beyondAEC Hackathon](https://beyondaec.tech) July 2018, in Boston MA. Developers were:

- [Leland Jobson](https://github.com/lelandjobson), CORE Studio | Thornton Tomasetti
- [Emil Poulsen](https://github.com/EmilPoulsen), CORE Studio | Thornton Tomasetti

## Tech Stack
- [GeneticSharp](https://github.com/giacomelli/GeneticSharp) - Genetic algorithm library.
- [SharpFE](https://github.com/iainsproat/SharpFE) - Finite element analysis library.
- WPF - Windows native UI.

## Use Frixel
- Clone the source code and build it using Visual studio or download the binaries from the [latest release](https://github.com/EmilPoulsen/Frixel/releases).
- Open up Rhinoceros 6.
- Install the plugin by drag and drop the `Frixel.Rhinoceros.rhp` file to the Rhino viewport.
- Type `Frixel` in the Rhino command window.
- Analyze and optimize designs!

## Notes
- Platform agnostic, prototype for Rhino 6
- Needs a special build of `SharpFE.Core.dll`, which is included in the lib folder.

## Roadmap
- Make optimization robust/verify output.
- Accurate structural quantities as input/output (not just relative numbers).
- Develop standalone version with sketch functionality.
