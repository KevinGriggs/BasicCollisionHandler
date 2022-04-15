# BasicCollisionHandler
A Basic Collision Handler written in C# for Unity

Author: Kevin Griggs

This is a class for a basic collision handler. It has a single static function and a public struct. The static function takes the change in time as well as the initial velocity, the initial position, intended direction, and the collider of the target object. It then uses shape casts to figure out the remaining distance that the object can travel in this tick as well as the object's new velocity and position in the form of a struct.
