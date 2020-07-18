using System.Collections.Generic;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
    public class AirTrafficUtilities
	{
		public static void GenerateSerials(int n)
		{
            string to = "\"";

            for (int i = 0; i < n; i++)
            {
                string sc = "";

                int r = Random.Range(65, 90);
                sc += (char)r;

                int r1 = Random.Range(65, 90);
                sc += (char)r1;

                float a = Random.Range(0f, 1f);
                if (a < 0.5f)
                {
                    sc += "-";
                }

                int r2 = Random.Range(0, 9);
                sc += r2.ToString();

                if (a >= 0.5f)
                {
                    sc += "-";
                }

                int r3 = Random.Range(0, 9);
                sc += r3.ToString();

                int r4 = Random.Range(0, 9);
                sc += r4.ToString();

                to += sc;
                to += "\", \"";
            }

            Debug.Log(to);
        }
    
        public static void GenerateOrigins(int n)
        {
            string[] cities = AirTrafficControlData.AllCities;

            string o = "";
            List<string> list = new List<string>();
            for (int i = 0; i< n; i++)
            {
                int r = Random.Range(0, cities.Length);
                string option = cities[r];

                //if (cities.Contains(option))
                //{
                //    i--;
                //}
                //else
                //{
                list.Add(option);
                o+="\"";
                o += option;
                o += "\",\n";
                //}
            }

            Debug.Log(o);
            Debug.Log(list.ToString());
        }
    
        public static void GenerateCrossTable(int x, int y, int a)
        {
            string o = "{\n";
            int[,] table = new int[x, y];
            for (int i = 0; i < x; i++)
            {
                o += "{";
                for (int j = 0; j < y; j++)
                {
                    int b = Random.Range(0, a);
                    table[i, j] = b;
                    o += b;
                    if(j < y - 1)
                    {
                        o += ",";
                    }
                }

                o += "}";
                if(i < x - 1)
                {
                    o += ",\n";
                }
            }

            Debug.Log(o);
        }
    
    }
}
