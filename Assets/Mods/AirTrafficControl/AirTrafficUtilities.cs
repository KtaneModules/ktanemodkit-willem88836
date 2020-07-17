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
    }
}
