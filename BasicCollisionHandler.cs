/*Author: Kevin Griggs
 *
 *This is a class for a basic collision handler. It has a single static function and a public struct
 *The static function takes the change in time as well as the initial velocity, the initial position, intended direction, and the collider of the target object
 *It then uses shape casts to figure out the remaining distance that the object can travel in this tick as well as the object's new velocity and position in the form of a struct
 *
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCollisionHandler : MonoBehaviour
{
    //The output type of calculateCollision()
    public struct CollisionResults
    {
        public float distanceRemaining; //The distance that the object can still travel this tick after colliding
        public Vector2 velocity; //The new velocity of the object after colliding
        public Vector2 position; //The new position of the object after colliding
    }

    //Calculates the distance remaining that the object can travel this tick and the new velocity and position of the object after a collision
    //Takes the change in time for the tick as well as the initial velocity, initial position, intended direction (based on input), and the collider of the object
    public static CollisionResults calculateCollision(float delta, Vector2 initialVelocity, Vector2 initialPosition, Vector2 intendedDirection, Collider2D collider)
    {
        //instantiates the output struct
        CollisionResults collisionResults = new CollisionResults 
        { 
            distanceRemaining = initialVelocity.magnitude * delta, //distance remaining starts as the distance that the object can travel at its initial velocity 
            velocity = initialVelocity, 
            position = initialPosition 
        };

        //Used to prevent infinite loops caused by sliding back and forth
        int numSlides = 0;

        //The shape cast loop
        while (numSlides < 10 && collisionResults.distanceRemaining > 0.002 && collisionResults.velocity.magnitude > 0.001)
        {
            //Objects necessary for shape casting. Can be modified for individual use.
            ContactFilter2D contactFilter = new ContactFilter2D();
            List<RaycastHit2D> hitResults = new List<RaycastHit2D>();

            //Performs the cast along the remaining distance
            collider.Cast(collisionResults.velocity, contactFilter, hitResults, collisionResults.distanceRemaining);
            
            //if there was a collision
            if (hitResults.Count > 0)
            {
                //used to break loop if all of the collisions are parallel
                bool hasValidHit = false;

                //edits the distance remaining, position, and velocity per hit object
                foreach (RaycastHit2D hitResult in hitResults)
                {
                    //The dot product of the velocity and the collision normal
                    float collisionDot = Vector2.Dot(collisionResults.velocity.normalized, hitResult.normal);

                    //If the collision was not parallel
                    if (collisionDot < -0.0001)
                    {
                        collisionResults.position = hitResult.centroid; //move up to the collision location
                        collisionResults.distanceRemaining -= hitResult.distance; //remove the distance traveled from the remaining distance to travel
                        float oldMag = collisionResults.velocity.magnitude * delta; //store the old magnitude
                        collisionResults.velocity += hitResult.normal * collisionResults.velocity.magnitude * Mathf.Abs(collisionDot); //calculate the new velocity
                        collisionResults.distanceRemaining -= Mathf.Abs(oldMag - collisionResults.velocity.magnitude * delta); //subtract the difference in velocity from the distance remaining
                        hasValidHit = true;
                        break; //begin a new cast
                    }
                }

                if (!hasValidHit) break; //if there were no non-parallel collision, stop calculating

                numSlides++;
            }
            else //if there were no collisions, stop calculating
            {
                break;
            }
        }

        if (collisionResults.velocity.magnitude < 0.0001)
        {
            collisionResults.velocity = Vector2.zero;
        }

        if (collisionResults.distanceRemaining <= 0.002)
        {
            collisionResults.distanceRemaining = 0;
        }

        //if caught in slide loop, stop the object
        if (numSlides == 10)
        {
            collisionResults.distanceRemaining = 0;
            collisionResults.velocity = Vector2.zero;
        }

        //if the new velocity is moving backward from the intended direction, stop
        if (Vector2.Dot(intendedDirection.normalized, collisionResults.velocity.normalized) < -0.0001)
        {
            collisionResults.velocity = Vector2.zero;
            collisionResults.distanceRemaining = 0;
        }

        return collisionResults;
    }
}
