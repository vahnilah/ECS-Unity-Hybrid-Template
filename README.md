ECS Hybrid Template for Unity
==========

This is an example to use the new DOTS system. Building a game using pure ECS isn’t always possible as many of the old tools will not work. So a hybrid is the best choice to transition your code to pure ECS when all the tools are available. 

Using a hybrid system won’t be useful unless you can minimize the time you access the entities list, this script will cache references to prevent an O(n) time every update down to a O(1) time for most updates to allow the new C# Job System to Burst Compile giving the performance benefits shown in the image below which I’ve achieved in my current project I’m building.

![alt text](https://i.imgur.com/s1h4W0I.png)

Uses: 
- Entities preview.30 - 0.0.12
- Unity 2019.1

Installation
------------

Download the github project. Attach a Game Object Entity script that comes with the Entities package and TemplateProxyHybrid.cs also to your GameObject. TemplateEnemyMovementSystemHybrid.cs will run in the Entity Debugger when the GameObject is instantiated.
