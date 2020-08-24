using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyGenerator : MonoBehaviour
{

    public int galaxyRadius = 1000;
    public float distanceBetweenEllipses = 0.2f;
    public float ellipseWidenessMultiplier = 0.5f;
    public int numberOfStarsPerRadius = 5;
    public bool DarkMatter = false;
    public float radMultiplier = 1f;
    public float timeStep = 1f;
    public float angleT = 10f;

    public float angleOffsetPerRadius = 10f;

    public float distanceNoiseMult;

    public float anglePerturb;

    public orbitData[] orbiterPrefabs;
    public float h2SpawnChance;

    public Gradient galaxyColor;




    // Start is called before the first frame update
    void Update()
    {

        foreach (Transform child in transform)
        {
            float rad = child.GetComponent<orbitData>().orbitRadius;
            float speedRad = (transform.position - child.position).magnitude;

            float distToCamera = (Camera.main.transform.position - child.position).magnitude;

            foreach (Transform particleEffect in child)
            {
                if (particleEffect.gameObject.tag == "Info")
                {
                    ParticleSystem.MainModule settings = particleEffect.GetComponent<ParticleSystem>().main;
                    settings.startColor = galaxyColor.Evaluate(speedRad / galaxyRadius);
                }
            }


            float newTheta = child.GetComponent<orbitData>().orbitTheta + (getAngularVelocity(speedRad) * Time.deltaTime * timeStep);
            if(newTheta > 360)
            {
                newTheta -= 360;
            }

            float eccentricity = ellipseWidenessMultiplier;

            if (rad < 3f)
            {
                eccentricity *= 1.75f;
            }

            child.position = computePosition((rad*anglePerturb) * angleOffsetPerRadius, rad, rad * eccentricity, newTheta, child.GetComponent<orbitData>().orbitInclination, child.GetComponent<orbitData>().orbitInclinationOffsetTheta);
            child.GetComponent<orbitData>().orbitTheta = newTheta;
        }

    }

    private void Start()
    {

        List<orbitData> objectWeightedList = new List<orbitData>(); ;

        foreach (orbitData orbiter in orbiterPrefabs)
        {
            int iterator = 0;

            while (iterator < orbiter.spawnWeight)
            {
                objectWeightedList.Add(orbiter);
                iterator++;
            }
        }


        int i = 0;
        float minradius = 0.25f;

        while (i <= galaxyRadius)
        {
            int k = 1;
            while (k <= numberOfStarsPerRadius)
            {


                float radOffset = Random.Range(-distanceBetweenEllipses*5000, distanceBetweenEllipses*5000) * (i / distanceNoiseMult);
                if(i < 3)
                {
                    radOffset *= 2.5f;
                }

                float rad = i + radOffset + minradius;

                GameObject star;


                float eccentricity = ellipseWidenessMultiplier;

                if(rad < 3f)
                {
                    eccentricity *= 0.5f;
                }


                star = Instantiate(objectWeightedList[Random.Range(0, objectWeightedList.Count)].gameObject, computePosition(angleT, rad, rad * ellipseWidenessMultiplier, 0, 0, 0), Random.rotation);


                star.transform.parent = transform;
                star.transform.localScale *= 0.02f;
                float start = k * (360 / numberOfStarsPerRadius) + Random.Range(-60f, 60f);
                star.GetComponent<orbitData>().orbitTheta = start;
                star.GetComponent<orbitData>().orbitRadius = rad;

                star.GetComponent<orbitData>().orbitInclination = Mathf.Clamp(Random.Range(-2, 2), -rad, rad);
                star.GetComponent<orbitData>().orbitInclinationOffsetTheta = Random.Range(-3.14f, 3.14f);


                k++;
            }
            i++;
        }
    }


    public static float MS(float r)
        {
            float d = 2000;  // Dicke der Scheibe
            float rho_so = 1;  // Dichte im Mittelpunkt
            float rH = 2000; // Radius auf dem die Dichte um die Hälfte gefallen ist
            return rho_so * Mathf.Exp(-r / rH) * (r * r) * Mathf.PI * d;
        }

        public static float MH(float r)
        {
            float rho_h0 = 0.15f; // Dichte des Halos im Zentrum
            float rC = 2500;     // typische skalenlänge im Halo
            return rho_h0 * 1 / (1 + Mathf.Pow(r / rC, 2)) * (4 * Mathf.PI * Mathf.Pow(r, 3) / 3);
        }

        // Velocity curve with dark matter
        public static float v(float r)
        {
            float MZ = 100;
            float G = 6.672e-11f;
            return 20000 * Mathf.Sqrt(G * (MH(r) + MS(r) + MZ) / r);
        }

         // velocity curve without dark matter
        public static float vd(float r)
        {
            float MZ = 100;
            float G = 6.672e-11f;
            return 20000 * Mathf.Sqrt(G * (MS(r) + MZ) / r);
        }
    

    private float getAngularVelocity(float rad)
    {

        // Realistically looking velocity curves for the Wikipedia models.
        float vel_kms;

            if (DarkMatter)
            {
                //  with dark matter
                vel_kms = v(rad);
            }
            else
            {
                // without dark matter:
                vel_kms = vd(rad);
            }

        // Calculate velocity in degree per year
            float u = 2 * Mathf.PI * rad * 3.086e+13f;        // Umfang in km
            float time = u / (vel_kms * 3.154e+7f);  // Umlaufzeit in Jahren

            return 360.0f / time;                                   // Grad pro Jahr
        }
    

    private Vector3 computePosition(float angle, float a, float b, float theta, float inc, float incOffset)
    {
        float beta = -angle;
        float alpha = theta * (Mathf.PI/180);

        float cosalpha = Mathf.Cos(alpha);
        float sinalpha = Mathf.Sin(alpha);
        float cosbeta = Mathf.Cos(beta);
        float sinbeta = Mathf.Sin(beta);

        float height = inc * Mathf.Sin(alpha + incOffset);

        Vector3 pos = new Vector3(transform.position.x + (a * cosalpha * cosbeta - b * sinalpha * sinbeta), height, transform.position.z + (a * cosalpha * sinbeta + b * sinalpha * cosbeta));


        return pos;
    }
}
