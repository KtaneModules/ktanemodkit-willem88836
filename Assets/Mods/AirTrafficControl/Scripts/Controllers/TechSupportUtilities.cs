using UnityEngine;

namespace WillemMeijer.NMTechSupport
{
    public class TechSupportUtilities
	{
        public static void PrintCS(string[] list)
        {
            string cs = "{\n";

            for (int i = 0; i < list.Length; i++)
            {
                cs += "\"" + list[i] + "\"";
                if (i < list.Length - 1)
                {
                    cs += ",\n";
                }
            }
            cs += "\n};";

            Debug.Log(cs);
        }


        public static void PrintCrosstableAsCS()
        {
            int[,] data = TechSupportData.OriginSerialCrossTable;

            string cs = "{\n";

            for (int i = 0; i < data.GetLength(0); i++)
            {
                cs += "{";
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    cs += data[i, j] ;
                    if (j < data.GetLength(1) - 1)
                    {
                        cs += ",";
                    }
                }
                cs += "}";
                if (i < data.GetLength(0) - 1)
                {
                    cs += ",\n";
                }
            }
            cs += "};";

            Debug.Log(cs);
        }

        public static void PrintCrossTableAsHTML()
        {
            int[,] data = TechSupportData.OriginSerialCrossTable;

            string html = "                        <tr class=\"Serial-Origin-Labels\"><th></th>";
            
            // labels.
            for (int k = 0; k < data.GetLength(1); k++)
            {
                html += "<th><div>"
                    + TechSupportData.SourceFileNames[k] 
                    + "</div></th>";
            }
            html += "</tr>\n";
            for(int i = 0; i < data.GetLength(0); i++)
            {
                html += "                        <tr>";
                html += "<th>" + TechSupportData.ErrorCodes[i] + "</th>";
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    html += "<th>" + (data[i, j] + 1) + "</th>";
                }
                html += "</tr>\n";
            }

            Debug.Log(html);
        }
    }
}
