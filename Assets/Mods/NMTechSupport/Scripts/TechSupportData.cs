using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TechSupportData : MonoBehaviour
{
    public static string[] SourceFileNames;
    public static string[] ErrorCodes;

    public static string[] PatchFiles;
    public static string[] VersionNumbers;
    public static string[] Parameters;

    public static int[,] OriginSerialCrossTable;

    [SerializeField] private int extentionEntryCount = 657;
    [SerializeField] private int extentionLength = 4;
    [SerializeField] private TextAsset extentions;
    [SerializeField] private int moduleEntryCount = 58112;
    [SerializeField] private int moduleLength = 6;
    [SerializeField] private TextAsset modulePrefixes;
    [SerializeField] private TextAsset moduleSuffixes;


    [NonSerialized] private string extentionText;
    [NonSerialized] private string modulePrefixText;
    [NonSerialized] private string moduleSuffixtext;
    private MonoRandom monoRandom;


    public ErrorData GenerateError(string moduleName)
    {
        int e = Random.Range(0, ErrorCodes.Length);
        int s = Random.Range(0, SourceFileNames.Length);
        int l = Random.Range(10, 150);
        int c = Random.Range(10, 100);

        ErrorData newError = new ErrorData(e, s, l, c, moduleName);

        return newError;
    }


    public void Generate(MonoRandom monoRandom, int s, int e, int pf, int v, int pa)
    {
        this.monoRandom = monoRandom;

        extentionText = extentions.text;
        modulePrefixText = modulePrefixes.text;
        moduleSuffixtext = moduleSuffixes.text;

        GenerateSourceFileNames(s, 4, 8);
        GenerateErrorCodes(e, 6);
        GeneratePatchFiles(pf, 4, 8);
        GenerateVersions(v);
        GenerateParameters(pa, 1, 4);
        GenerateCrossTable(v);
    }


    private string GenerateFileName(int minLength, int maxLength)
    {
        int length = monoRandom.Next(minLength, maxLength);
        int pivot = length / 2;

        string name = "";

        // Generates first half.
        int w = monoRandom.Next(0, moduleEntryCount) * moduleLength;
        string word = modulePrefixText.Substring(w, moduleLength);
        for (int j = 0; j < pivot; j++)
        {
            if (j >= word.Length || word[j] == ' ')
            {
                break;
            }
            else
            {
                name += word[j];
            }
        }

        // Generates second half.
        w = monoRandom.Next(0, moduleEntryCount) * moduleLength;
        word = moduleSuffixtext.Substring(w, moduleLength);
        for (int j = word.Length - pivot; j < word.Length; j++)
        {
            if (j < 0 || word[j] == ' ')
            {
                continue;
            }
            else
            {
                name += word[j];
            }
        }

        // generates extention
        w = monoRandom.Next(0, extentionEntryCount) * extentionLength;
        word = extentionText.Substring(w, extentionLength);
        word = word.Trim();

        name += "." + word;

        return name;
    }

    private void GenerateSourceFileNames(int n, int minLength, int maxLength)
    {
        string[] names = new string[n];

        for (int i = 0; i < n; i++)
        {
            names[i] = GenerateFileName(minLength, maxLength);
        }

        SourceFileNames = names;
    }

    private void GenerateErrorCodes(int n, int l)
    {
        string[] errorCodes = new string[n];

        for (int i = 0; i < n; i++)
        {
            string errorCode = "0x";

            for (int j = 0; j < l; j++)
            {
                int c = monoRandom.Next(0, 16);
                string h = c.ToString("X");
                errorCode += h;
            }

            errorCodes[i] = errorCode;
        }

        ErrorCodes = errorCodes;
    }

    private void GeneratePatchFiles(int n, int minLength, int maxLength)
    {
        string[] names = new string[n];

        for (int i = 0; i < n; i++)
        {
            names[i] = GenerateFileName(minLength, maxLength);
        }

        PatchFiles = names;
    }

    private void GenerateVersions(int n)
    {
        string[] versions = new string[n];

        for (int i = 0; i < n; i++)
        {
            versions[i] = "Version " + (i + 1);
        }

        VersionNumbers = versions;
    }

    private void GenerateParameters(int n, int minLength, int maxLength)
    {
        string[] parameters = new string[n];

        for (int i = 0; i < n; i++)
        {
            string p = "";

            List<char> par = new List<char>();

            int k = monoRandom.Next(minLength, maxLength);
            for (int j = 0; j < k; j++)
            {
                // the range 97 to 123 are lower case letters in ascii.
                char c = (char)monoRandom.Next(97, 123);
                if (par.Contains(c))
                {
                    j--;
                }
                else
                {
                    p += "-" + c + " ";
                    par.Add(c);
                }
            }

            parameters[i] = p;
        }

        Parameters = parameters;
    }

    private void GenerateCrossTable(int l)
    {
        int[,] crossTable = new int[ErrorCodes.Length, SourceFileNames.Length];

        for (int i = 0; i < ErrorCodes.Length; i++)
        {
            for (int j = 0; j < SourceFileNames.Length; j++)
            {
                int k = monoRandom.Next(0, l);
                crossTable[i, j] = k;
            }
        }

        OriginSerialCrossTable = crossTable;
    }
}
