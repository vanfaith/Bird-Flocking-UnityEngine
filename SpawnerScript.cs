using UnityEngine;
using System.Collections;

public class SpawnerScript : MonoBehaviour {
    float[]  F_el;
    float[] F_tot; //total force vector
    float[] sum_velocity; //velocity vectors
    float[] center;
    float[] sum_positions;
    public float DesiredDistance=25;
    public float neighbordist = 50;
	
    // Array of prefabs.
    private int number;
     int bouncyness=45; 
     public int numOfPart = 50;// number of particles for future use
	 
    public float Separation_Weight = 1.5f;
    public float Coherence_Weight = 1;
    public float Alignment_Weight = 1;
	
    public int max_speed = 2;
    float[,] StatesArray = new float[1000, 6];// array of position and velocity of particles
    public GameObject[] particles;
    private int count;

    // Use this for initialization
    void Start () {
        //numOfPart = 5;
        particles = new GameObject[numOfPart];
        number = 0;
        //DesiredDistance = 25;

        
        center = new float[3];
        F_el = new float[3] { 0.0f, 0.0f, 0.0f };
        F_tot = new float[3] { 0.0f, 0.0f, 0.0f };
        sum_velocity= new float[3] { 0.0f, 0f, 0.0f };
        sum_positions = new float[3] { 0.0f, 0f, 0.0f };


        ////center
        for (int d = 0; d < 3; d++) center[d] = 0;// center of world

        for (int i = 0; i < numOfPart; i++) //creation of "birds"
        {
            ////position
            for (int d = 0; d < 3; d++) StatesArray[i,d] = Random.Range(-5, 5);
            ////velocity
            for (int d = 0; d < 3; d++) StatesArray[i, d + 3] = Random.Range(0, 15);
            GameObject temp1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp1.transform.position = new Vector3(StatesArray[i,0], StatesArray[i, 1], StatesArray[i, 2]);
            particles[i] = temp1;
            

        }



    }


    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        F_el = new float[3]{ 0.0f,0.0f,0.0f };
        F_tot = new float[3] { 0.0f, 0.0f, 0.0f };
        sum_velocity = new float[3] { 0.0f, 0.0f, 0.0f };
        sum_positions= new float[3] { 0.0f, 0.0f, 0.0f };
        for (int i = 0; i < numOfPart; i++)
        {
            
            F_tot = new float[3] { 0.0f, 0.0f, 0.0f };
            count = 0;

            //------------------------------------------------------------------------------------
            for (int j = 0; j < numOfPart; j++) //impulsive force calculation
            {
                if (i != j)// avoid comparing with self
                {
                     

                    F_el = new float[3] { 0.0f, 0.0f, 0.0f };


                    for (int d = 0; d < 3; d++)
                    {
                        F_el[d] = -((StatesArray[j, d] - StatesArray[i, d]));// Force from i to j
                    }


                    float magnitude = (float)System.Math.Sqrt(F_el[0] * F_el[0] + F_el[1] * F_el[1] + F_el[2] * F_el[2]);//also the distance between birds
                    float epsilon = 1e-10f;

					
					//.......separation.........
                    if (magnitude < DesiredDistance) 
                    {


                        for (int d = 0; d < 3; d++)
                        {
                            F_el[d] /= (magnitude + epsilon);// unit vector of direction
                            F_el[d] *= (Separation_Weight* max_speed);// platos analogo apostasis
                        }
                    

                    for (int d = 0; d < 3; d++) F_tot[d] += F_el[d];// add force to Total Force
                    }
					
					
					
					//...........allignment(velocity) & coherence(position)
                    if (magnitude < neighbordist) 
                    {


                        for (int d = 0; d < 3; d++)
                        {
                            sum_velocity[d] += (StatesArray[j, d+3]);//sum of velocities of nearby birds
                            sum_positions[d] += (StatesArray[j,  d]);//sum of positions of nearby birds
                            count++;
                        }


                        //for (int d = 0; d < 3; d++) F_tot[d] += F_el[d];// add force to sinistameni
                    }






                }
      
            }
            if (count > 0)
            {
                //---------------------------------------------------
                for (int d = 0; d < 3; d++) //average position and velocity of nearby birds
                {
                    sum_velocity[d] /= (float)count;
                    sum_positions[d] /= count;     
                }

                float magnitude = (float)System.Math.Sqrt(sum_velocity[0] * sum_velocity[0] + sum_velocity[1] * sum_velocity[1] + sum_velocity[2] * sum_velocity[2]);
                float magnitude2 = (float)System.Math.Sqrt(sum_positions[0] * sum_positions[0] + sum_positions[1] * sum_positions[1] + sum_positions[2] * sum_positions[2]);
                float epsilon = 1e-10f;
                for (int d = 0; d < 3; d++) //normalization and weighting
                {
                    sum_velocity[d] /= (magnitude + epsilon);
                    sum_velocity[d] *= (Alignment_Weight* max_speed);
                    sum_velocity[d] -= StatesArray[i, d + 3];

                    sum_velocity[d] /= (magnitude2 + epsilon);
                    sum_positions[d] *= (Coherence_Weight*max_speed);
                    sum_positions[d] -= StatesArray[i, d];


                }


                



            
                for (int d = 0; d < 3; d++) F_tot[d] += (sum_velocity[d] + sum_positions[d]);// add force ij to Net Force
            }
            //==========================================

            for (int d = 0; d < 3; d++) F_el[d] = F_tot[d];


            for (int d = 0; d < 3; d++)
                {
                    StatesArray[i, d + 3] += F_el[d] * dt;  //update velocity according to attraction forces
                }

                // StatesArray[i, 4] += (-10) * dt; // update velocity in y axis according to gravity force


                if (StatesArray[i, 0] * StatesArray[i, 0] + StatesArray[i, 1] * StatesArray[i, 1] + StatesArray[i, 2] * StatesArray[i, 2] >= 25000)// sinthiki mesa se sfera 
                
				
				
				//.......collision detection , no loss of energy hypothesis ( there is a transparent sphere around birds)
                {
                    //StatesArray[i, 4] = 10.1f;
                    for (int d = 0; d < 3; d++)//finding normal vector on the surface of the sphere
                    {
                        StatesArray[i, 3 + d] = ((0 - StatesArray[i, d]));//Net Force

                    }

                    float magnitude = (float)System.Math.Sqrt(StatesArray[i, 3 ] * StatesArray[i, 3 ] + StatesArray[i, 4] * StatesArray[i, 4] + StatesArray[i, 5] * StatesArray[i, 5]);
                    float epsilon = 1e-10f;

                    for (int d = 0; d < 3; d++)
                    {
                        StatesArray[i, 3+d] /= (magnitude + epsilon);// unit normal vector for collision 
                        StatesArray[i, 3 + d] *= bouncyness;
                    }




                }// else { g = -0.1f; }  } 


                for (int d = 0; d < 3; d++) { StatesArray[i, d] += StatesArray[i, 3 + d] * dt; } //position update according to velocity and Net Forces

                particles[i].transform.position = new Vector3(StatesArray[i, 0], StatesArray[i, 1], StatesArray[i, 2]);
            
        }


        }

}