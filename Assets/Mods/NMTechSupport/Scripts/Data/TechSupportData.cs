using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NMTechSupport
{
    public class TechSupportData : MonoBehaviour
    {
        public static string[] SourceFileNames;
        public static string[] ErrorCodes;

        public static string[] PatchFiles;
        public static string[] VersionNumbers;
        public static string[] Parameters;

        public static int[,] OriginSerialCrossTable;

        [Header("Assets")]
        [SerializeField] private TextAsset modulePrefixes;
        [SerializeField] private TextAsset moduleSuffixes;
        [SerializeField] private TextAsset extentions;

        [Header("Asset Data")]
        [SerializeField] private int moduleEntryCount = 58112;
        [SerializeField] private int moduleLength = 6;
        [SerializeField] private int extentionEntryCount = 657;
        [SerializeField] private int extentionLength = 4;

        [NonSerialized] private string extentionText;
        [NonSerialized] private string modulePrefixText;
        [NonSerialized] private string moduleSuffixtext;
        private MonoRandom monoRandom;


        public ErrorData GenerateError(string moduleName)
        {
            int errorIndex = Random.Range(0, ErrorCodes.Length);
            int sourceIndex = Random.Range(0, SourceFileNames.Length);
            int lineIndex = Random.Range(10, 150);
            int columnIndex = Random.Range(10, 100);
            return new ErrorData(errorIndex, sourceIndex, lineIndex, columnIndex, moduleName);
        }


        public void Generate(RuleParameters parameters)
        {
            this.monoRandom = parameters.Random;

            extentionText = extentions.text;
            modulePrefixText = modulePrefixes.text;
            moduleSuffixtext = moduleSuffixes.text;

            GenerateSourceFileNames(parameters.SourceFileCount, 4, 8);
            GenerateErrorCodes(parameters.ErrorCodeCount, 6);
            GeneratePatchFiles(parameters.PatchFileCount, 4, 8);
            GenerateVersions(parameters.VersionCount);
            GenerateParameters(parameters.ParameterCount, 1, 4);
            GenerateCrossTable(parameters.VersionCount);
        }

        private string GenerateFileName(int minLength, int maxLength)
        {
            int length = monoRandom.Next(minLength, maxLength);
            int pivot = length / 2;

            // Generates first half of the file name.
            int prefixOffset = monoRandom.Next(0, moduleEntryCount) * moduleLength;
            string prefix = modulePrefixText.Substring(prefixOffset, moduleLength).Trim();

            // Generates second half of the file name.
            int suffixOffset = monoRandom.Next(0, moduleEntryCount) * moduleLength;
            string suffix = moduleSuffixtext.Substring(suffixOffset, moduleLength).Trim();

            // generates extention
            int extentionOffset = monoRandom.Next(0, extentionEntryCount) * extentionLength;
            string extention = extentionText.Substring(extentionOffset, extentionLength).Trim();

            // Concatenates parts.
            string name = prefix.Substring(0, pivot)
                + suffix.Substring(suffix.Length - pivot)
                + "." + extention;

            return name;
        }

        private void GenerateSourceFileNames(int n, int minLength, int maxLength)
        {
            string[] names = new string[n];

            for (int i = 0; i < n; i++)
                names[i] = GenerateFileName(minLength, maxLength);

            SourceFileNames = names;
        }

        private void GenerateErrorCodes(int n, int l)
        {
            string[] errorCodes = new string[n];

            for (int i = 0; i < n; i++)
            {
                string errorCode = "0x";

                // Adds the hexadecimal value to the string.
                for (int j = 0; j < l; j++)
                    errorCode += monoRandom.Next(0, 16).ToString("X");

                errorCodes[i] = errorCode;
            }

            ErrorCodes = errorCodes;
        }

        private void GeneratePatchFiles(int n, int minLength, int maxLength)
        {
            string[] names = new string[n];

            for (int i = 0; i < n; i++)
                names[i] = GenerateFileName(minLength, maxLength);

            PatchFiles = names;
        }

        private void GenerateVersions(int n)
        {
            string[] versions = new string[n];

            for (int i = 0; i < n; i++)
                versions[i] = "Version " + (i + 1);

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
                for (int j = 0; j < SourceFileNames.Length; j++)
                {
                    int k = monoRandom.Next(0, l);
                    crossTable[i, j] = k;
                }

            OriginSerialCrossTable = crossTable;
        }
    }
}
