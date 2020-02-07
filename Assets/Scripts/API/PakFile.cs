// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

#region Using Statements
using System;
using System.IO;
using DaggerfallConnect.Utility;
#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// Connects to CLIMATE.PAK or POLITIC.PAK to extract and read meta-data about the Daggerfall world map.
    /// </summary>
    public class PakFile
    {
        #region Class Variables

        /// <summary>Length of each PAK row.</summary>
        public const int pakWidthValue = 29;//1001;err119-25, //25 23,20, -Pos:25,575, =24,  //25 24,25, -Pos:25,600, =29 //50 24,28, -Pos:28,600, =29
        public const int pakHeightValue = 16;//500


        //300x15    14,10, -Pos:300,4200, =29
        //300x30    15,28, -Pos:289,4500, =29
        //300x28    15,28, -Pos:289,4500, =29
        //300x26    same

        //27 24,28, -Pos:28,648, =29
        ///


        //25 24,25, -Pos:25,600, =29
        //26 24,26, -Pos:26,624, =29
        //27 24,27, -Pos:27,648, =29

        //50x27 =   24,28, -Pos:28,1200, =29
        //          24,28, -Pos:28,1200, =29
        //40x27     24,28, -Pos:28,960, =29
        //45x27     24,28, -Pos:28,1080, =29
        //47x27     24,28, -Pos:28,1176, =29
        //100x27    22,28, -Pos:86,2200, =29
        //200x27    19,28, -Pos:173,3800, =29
        //--->      //300x27                    //15,28, -Pos:289,4500, =29
        //600x27    5,28,  -Pos:579,3000, =29
        //601x27    5,28, -Pos:579,3005, =29
        //610x27    4,28, -Pos:608,2440, =29
        //620x27    4,28, -Pos:608,2480, =29
        //630x27    4,28, -Pos:608,2520, =29
        //640x27    3,28, -Pos:637,1920, =29
        //665x27    3,28, -Pos:637,1995, =29
        //690x27    2,28, -Pos:66X,1380, =29
        //695x27    1,28, -Pos:695,699, =29

        /// <summary>Number of PAK rows.</summary>
        //public const int pakHeightValue = 28;//500
        //24 23,20, -Pos:25,575, =24
        //25 24,25, -Pos:25,600, =29
        //50 24,28, -Pos:28,600, =29
        //26 24,28, -Pos:28,648, =29
        //27 24,28, -Pos:28,648, =29
        ///

        //25 24,25, -Pos:25,600, =29
        //25 24,25, -Pos:25,600, =29
        //25 24,27, -Pos:27,648, =29


        /// <summary>Memory length of extracted PAK file.</summary>
        const int pakBufferLengthValue = pakWidthValue * pakHeightValue;

        /// <summary>Abstracts PAK file to a managed disk or memory stream.</summary>
        private readonly FileProxy managedFile = new FileProxy();

        /// <summary>Extracted PAK file buffer.</summary>
        private Byte[] pakExtractedBuffer = new Byte[pakBufferLengthValue];

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets extracted PAK data.
        /// </summary>
        public Byte[] Buffer
        {
            get { return pakExtractedBuffer; }
            set { pakExtractedBuffer = value; }
        }

        /// <summary>
        /// Number of rows in PAK file (always 500).
        /// </summary>
        public int PakRowCount
        {
            get { return pakHeightValue; }
        }

        /// <summary>
        /// Number of bytes per PAK row (always 1001).
        /// </summary>
        public int PakRowLength
        {
            get { return pakWidthValue; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PakFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="filePath">Absolute path to PAK file.</param>
        public PakFile(string filePath)
        {
            Load(filePath);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load PAK file.
        /// </summary>
        /// <param name="filePath">Absolute path to PAK file.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string filePath)
        {
            // Validate filename
            if (!filePath.EndsWith("CLIMATE.PAK", StringComparison.InvariantCultureIgnoreCase) &&
                !filePath.EndsWith("POLITIC.PAK", StringComparison.InvariantCultureIgnoreCase))
                return false;

            // Load file
            if (!managedFile.Load(filePath, FileUsage.UseMemory, true))
                return false;

            // Expand each row of PAK file into buffer
            BinaryReader offsetReader = managedFile.GetReader(0);
            BinaryReader rowReader = managedFile.GetReader();
            for (int row = 0; row < pakHeightValue; row++)
            {
                // Get offsets
                UInt32 offset = offsetReader.ReadUInt32();
                int bufferPos = pakWidthValue * row;
                rowReader.BaseStream.Position = offset;

                // Unroll PAK row into buffer
                int rowPos = 0;
                while (rowPos < pakWidthValue)
                {
                    //UnityEngine.Debug.LogWarning(rowPos);
                    // Get PakRun data
                    UInt16 count = rowReader.ReadUInt16();
                    Byte value = rowReader.ReadByte();

                    // Do PakRun
                    for (int c = 0; c < count; c++)
                    {
                        //DAGUnity UnityEngine.Debug.LogWarning(row +","+ c + ", -Pos:" + rowPos +","+ bufferPos + ", =" + count);//724,0,29
                        pakExtractedBuffer[bufferPos + rowPos++] = value;
                    }
                }
            }

            // Managed file is no longer needed
            managedFile.Close();

            return true;
        }

        /// <summary>
        /// Get extracted PAK data as an indexed image.
        /// </summary>
        /// <returns>DFBitmap object.</returns>
        public DFBitmap GetDFBitmap()
        {
            DFBitmap DFBitmap = new DFBitmap();
            DFBitmap.Width = pakWidthValue;
            DFBitmap.Height = PakRowCount;
            DFBitmap.Data = pakExtractedBuffer;
            return DFBitmap;
        }

        /// <summary>
        /// Gets value for specified position in world map.
        /// </summary>
        /// <param name="x">X position in world map. 0 to PakRowLength-1.</param>
        /// <param name="y">Y position in world map. 0 to PakRowCount-1.</param>
        /// <returns>Value of pak data if valid, -1 if invalid.</returns>
        public int GetValue(int x, int y)
        {
            // Validate
            if (x < 0 || x >= PakRowLength) return -1;
            if (y < 0 || y >= PakRowCount) return -1;

            return Buffer[(y * PakRowLength) + x];
        }

        #endregion
    }
}
