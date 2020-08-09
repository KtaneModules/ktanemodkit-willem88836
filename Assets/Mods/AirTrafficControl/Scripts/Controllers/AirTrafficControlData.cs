using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WillemMeijer.NMAirTrafficControl
{
    [Serializable]
    public class AirTrafficControlData
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

        private MonoRandom mRandom;

        public void Generate(int s, int e, int pf, int v, int pa)
        {
            mRandom = new MonoRandom(0);
            GenerateSourceFileNames(s, 4, 8);
            GenerateErrorCodes(e, 6);
            GeneratePatchFiles(pf);
            GenerateVersions(v);
            GenerateParameters(pa);
            GenerateCrossTable(v);
        }


        private void GenerateSourceFileNames(int n, int minLength, int maxLength)
        {
            string modulePrefixes = this.modulePrefixes.text;
            string moduleSuffixes = this.moduleSuffixes.text;
            string extentions = this.extentions.text;

            string[] names = new string[n];

            for (int i = 0; i < n; i++)
            {
                int length = mRandom.Next(minLength, maxLength);
                int pivot = length / 2;

                string name = "";

                // Generates first half.
                int w = mRandom.Next(0, moduleEntryCount);
                string word = modulePrefixes.Substring(w, moduleLength);
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
                w = mRandom.Next(0, moduleEntryCount);
                word = moduleSuffixes.Substring(w, moduleLength);
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
                w = mRandom.Next(0, extentionEntryCount);
                word = extentions.Substring(w, extentionLength);
                word = word.Trim();

                name += "." + word;

                names[i] = name;
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
                    int c = mRandom.Next(0, 16);
                    string h = c.ToString("X");
                    errorCode += h;
                }

                errorCodes[i] = errorCode;
            }

            ErrorCodes = errorCodes;
        }

        private void GeneratePatchFiles(int n)
        {

        }

        private void GenerateVersions(int n)
        {

        }

        private void GenerateParameters(int n)
        {
            // TODO;
        }

        private void GenerateCrossTable(int l)
        {
            int[,] crossTable = new int[ErrorCodes.Length, SourceFileNames.Length];

            for (int i = 0; i < ErrorCodes.Length; i++)
            {
                for (int j = 0; j < SourceFileNames.Length; j++)
                {
                    int k = mRandom.Next(0, l);
                    crossTable[i, j] = k;
                }
            }

            OriginSerialCrossTable = crossTable;
        }


        public PlaneData GeneratePlane()
        {
            int s = Random.Range(0, ErrorCodes.Length);
            int o = Random.Range(0, SourceFileNames.Length);
            int p = Random.Range(10, 30);
            int l = Random.Range(10, 25);

            PlaneData newPlane = new PlaneData(s, o, p, l);

            return newPlane;
        }
    }
}