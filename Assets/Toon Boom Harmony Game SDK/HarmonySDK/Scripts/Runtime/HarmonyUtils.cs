using UnityEngine;
using System.IO;
using System.Text;

namespace ToonBoom.Harmony
{
    internal static class HarmonyUtils
    {
        internal static double[] GenerateIdentityMatrixCopy
        {
            get
            {
                return new double[] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
            }
        }

        internal static void ReadBinaryMatrix(BinaryReader reader, double[] matrix)
        {
            matrix[0] = reader.ReadDouble();
            matrix[1] = reader.ReadDouble();
            matrix[2] = reader.ReadDouble();
            matrix[3] = reader.ReadDouble();
            matrix[4] = reader.ReadDouble();
            matrix[5] = reader.ReadDouble();
            matrix[6] = reader.ReadDouble();
            matrix[7] = reader.ReadDouble();
            matrix[8] = reader.ReadDouble();
            matrix[9] = reader.ReadDouble();
            matrix[10] = reader.ReadDouble();
            matrix[11] = reader.ReadDouble();
            matrix[12] = reader.ReadDouble();
            matrix[13] = reader.ReadDouble();
            matrix[14] = reader.ReadDouble();
            matrix[15] = reader.ReadDouble();
        }

        internal static void WriteBinaryMatrix(BinaryWriter writer, double[] matrix)
        {
            writer.Write(matrix[0]);
            writer.Write(matrix[1]);
            writer.Write(matrix[2]);
            writer.Write(matrix[3]);
            writer.Write(matrix[4]);
            writer.Write(matrix[5]);
            writer.Write(matrix[6]);
            writer.Write(matrix[7]);
            writer.Write(matrix[8]);
            writer.Write(matrix[9]);
            writer.Write(matrix[10]);
            writer.Write(matrix[11]);
            writer.Write(matrix[12]);
            writer.Write(matrix[13]);
            writer.Write(matrix[14]);
            writer.Write(matrix[15]);
        }

        internal static void NativeArrayToMatrix(float[] array, int offset, out Matrix4x4 result)
        {
            result = Matrix4x4.identity;
            result.m00 = array[offset];
            result.m10 = array[offset + 1];
            result.m20 = array[offset + 2];
            result.m30 = array[offset + 3];
            result.m01 = array[offset + 4];
            result.m11 = array[offset + 5];
            result.m21 = array[offset + 6];
            result.m31 = array[offset + 7];
            result.m02 = array[offset + 8];
            result.m12 = array[offset + 9];
            result.m22 = array[offset + 10];
            result.m32 = array[offset + 11];
            result.m03 = array[offset + 12];
            result.m13 = array[offset + 13];
            result.m23 = array[offset + 14];
            result.m33 = array[offset + 15];
        }

        internal static void ReadHarmonyId(BinaryReader reader, out HarmonyId harmonyId)
        {
            string name = ReadHarmonyString(reader);
            int id = reader.ReadInt32();
            harmonyId = new HarmonyId(id, name);
        }

        internal static string ReadHarmonyString(BinaryReader reader)
        {
            int stringLength = reader.ReadInt32();
            byte[] stringBytes = reader.ReadBytes(stringLength);

            return Encoding.ASCII.GetString(stringBytes); //ASCII encoding will likely break things! Switch to UTF
        }
    }
}
