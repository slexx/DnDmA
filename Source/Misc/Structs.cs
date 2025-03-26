using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Versioning;
using System.Security.Cryptography.Xml;


namespace DMAW_DND
{
    public struct ViewMatrix
    {
        private float[][] matrix;

        public float this[int row, int col]
        {
            get { return matrix[row][col]; }
            set { matrix[row][col] = value; }
        }

        public ViewMatrix(int rows, int cols)
        {
            matrix = new float[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new float[cols];
            }
        }
    };

    public struct Bone
    {
        public ulong Address;
        public FTransform Transform;
        public FVector3 Location;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FGameplayTagContainer
    {
        [FieldOffset(0x0)]
        public TArray GameplayTags;
        [FieldOffset(0x10)]
        public TArray ParentTags;
    }
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FRotator
    {
        [FieldOffset(0x0)]
        public double Pitch;
        [FieldOffset(0x8)]
        public double Yaw;
        [FieldOffset(0x10)]
        public double Roll;

        public FRotator (double _pitch, double _yaw, double _roll)
        {
            Pitch = _pitch;
            Yaw = _yaw;
            Roll = _roll;
        }
    }
    public struct FVector2
    {
        public double X;
        public double Y;
    }
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FVector3
    {
        [FieldOffset(0x0)]
        public double X; //Will be pitch on rotator
        [FieldOffset(0x8)]
        public double Y; //Will be yaw on rotator
        [FieldOffset(0x10)]
        public double Z; //Will be roll on rotator

        
        public FVector3(double _x, double _y, double _z)
        {
            X = _x;
            Y = _y;
            Z = _z;
        }
        public static FVector3 operator -(FVector3 a, FVector3 b)
        {
            return new FVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static FVector3 operator +(FVector3 a, FVector3 b)
        {
            return new FVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public double Dot(FVector3 b)
        {
            return X * b.X + Y * b.Y + Z * b.Z;
        }
    }

    public struct Matrix4x4
    {
        public double M11;
        public double M12;
        public double M13;
        public double M14;

        public double M21;
        public double M22;
        public double M23;
        public double M24;

        public double M31;
        public double M32;
        public double M33;
        public double M34;

        public double M41;
        public double M42;
        public double M43;
        public double M44;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BoneMemoryLayout
    {
        public float X;
        public float Y;
        public float Z;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct QAngle
    {
        public float X;
        public float Y;
        public float Z;
        // Add other fields if necessary
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct UtlVectorStruct
    {
        public ulong Size; // Using ulong for DWORD64
        public ulong Data; // Pointer to the array of elements
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct NetworkUtlVector
    {
        public uint size;
        public ulong data;
    }

    public struct TArray
    {
        public ulong Data;
        public int Count;
        public int Max;
    }
    public struct FString
    {
        public IntPtr Data;
        public int Count; // NumElements in UE, assuming 32-bit int for simplicity
        public int Max; // MaxElements in UE, assuming 32-bit int for simplicity
    }
    public unsafe struct FNickname
    {
        public FString OriginalNickName;
        public FString StreamingModeNickName; // Rogue#123123
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FName
    {
        [FieldOffset(0x0)]
        public int ComparisonIndex;
        [FieldOffset(0x4)]
        public int Number;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FDesignDataItem
    {
        [FieldOffset(0x90)]
        public FGameplayTag RarityType;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FGameplayTag
    {
        [FieldOffset(0x0)]
        public FName TagName;
    }
    public struct FQuat
    {
        public double X;
        public double Y;
        public double Z; // Top Down Rotation Angle
        public double W;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FTransform
    {
        [FieldOffset(0x0)]
        public FQuat Rotation;
        [FieldOffset(0x20)]
        public FVector3 Translation;
        [FieldOffset(0x40)]
        public FVector3 Scale3D;

        public Matrix4x4 ToMatrixWithScale()
        {
            Matrix4x4 m = new Matrix4x4();
            m.M41 = Translation.X;
            m.M42 = Translation.Y;
            m.M43 = Translation.Z;

            double x2 = Rotation.X + Rotation.X;
            double y2 = Rotation.Y + Rotation.Y;
            double z2 = Rotation.Z + Rotation.Z;

            double xx2 = Rotation.X * x2;
            double yy2 = Rotation.Y * y2;
            double zz2 = Rotation.Z * z2;
            m.M11 = (1.0f - (yy2 + zz2)) * Scale3D.X;
            m.M22 = (1.0f - (xx2 + zz2)) * Scale3D.Y;
            m.M33 = (1.0f - (xx2 + yy2)) * Scale3D.Z;

            double yz2 = Rotation.Y * z2;
            double wx2 = Rotation.Z * x2;
            m.M32 = (yz2 - wx2) * Scale3D.Z;
            m.M23 = (yz2 + wx2) * Scale3D.Y;

            double xy2 = Rotation.X * y2;
            double wz2 = Rotation.Z * z2;
            m.M21 = (xy2 - wz2) * Scale3D.Y;
            m.M12 = (xy2 + wz2) * Scale3D.X;

            double xz2 = Rotation.X * z2;
            double wy2 = Rotation.Z * y2;
            m.M31 = (xz2 + wy2) * Scale3D.Z;
            m.M13 = (xz2 - wy2) * Scale3D.X;

            m.M14 = 0;
            m.M24 = 0;
            m.M34 = 0;
            m.M44 = 1;

            return m;
        }


        public Vector3 GetVectorPos()
        {
            return new Vector3((float)Translation.X, (float)Translation.Y, (float)Translation.Z);
        }
        public static Matrix4x4 MatrixMultiplication(Matrix4x4 matrixOne, Matrix4x4 matrixTwo)
        {
            return new Matrix4x4
            {
                M11 = matrixOne.M11 * matrixTwo.M11 + matrixOne.M12 * matrixTwo.M21 + matrixOne.M13 * matrixTwo.M31 + matrixOne.M14 * matrixTwo.M41,
                M12 = matrixOne.M11 * matrixTwo.M12 + matrixOne.M12 * matrixTwo.M22 + matrixOne.M13 * matrixTwo.M32 + matrixOne.M14 * matrixTwo.M42,
                M13 = matrixOne.M11 * matrixTwo.M13 + matrixOne.M12 * matrixTwo.M23 + matrixOne.M13 * matrixTwo.M33 + matrixOne.M14 * matrixTwo.M43,
                M14 = matrixOne.M11 * matrixTwo.M14 + matrixOne.M12 * matrixTwo.M24 + matrixOne.M13 * matrixTwo.M34 + matrixOne.M14 * matrixTwo.M44,
                M21 = matrixOne.M21 * matrixTwo.M11 + matrixOne.M22 * matrixTwo.M21 + matrixOne.M23 * matrixTwo.M31 + matrixOne.M24 * matrixTwo.M41,
                M22 = matrixOne.M21 * matrixTwo.M12 + matrixOne.M22 * matrixTwo.M22 + matrixOne.M23 * matrixTwo.M32 + matrixOne.M24 * matrixTwo.M42,
                M23 = matrixOne.M21 * matrixTwo.M13 + matrixOne.M22 * matrixTwo.M23 + matrixOne.M23 * matrixTwo.M33 + matrixOne.M24 * matrixTwo.M43,
                M24 = matrixOne.M21 * matrixTwo.M14 + matrixOne.M22 * matrixTwo.M24 + matrixOne.M23 * matrixTwo.M34 + matrixOne.M24 * matrixTwo.M44,
                M31 = matrixOne.M31 * matrixTwo.M11 + matrixOne.M32 * matrixTwo.M21 + matrixOne.M33 * matrixTwo.M31 + matrixOne.M34 * matrixTwo.M41,
                M32 = matrixOne.M31 * matrixTwo.M12 + matrixOne.M32 * matrixTwo.M22 + matrixOne.M33 * matrixTwo.M32 + matrixOne.M34 * matrixTwo.M42,
                M33 = matrixOne.M31 * matrixTwo.M13 + matrixOne.M32 * matrixTwo.M23 + matrixOne.M33 * matrixTwo.M33 + matrixOne.M34 * matrixTwo.M43,
                M34 = matrixOne.M31 * matrixTwo.M14 + matrixOne.M32 * matrixTwo.M24 + matrixOne.M33 * matrixTwo.M34 + matrixOne.M34 * matrixTwo.M44,
                M41 = matrixOne.M41 * matrixTwo.M11 + matrixOne.M42 * matrixTwo.M21 + matrixOne.M43 * matrixTwo.M31 + matrixOne.M44 * matrixTwo.M41,
                M42 = matrixOne.M41 * matrixTwo.M12 + matrixOne.M42 * matrixTwo.M22 + matrixOne.M43 * matrixTwo.M32 + matrixOne.M44 * matrixTwo.M42,
                M43 = matrixOne.M41 * matrixTwo.M13 + matrixOne.M42 * matrixTwo.M23 + matrixOne.M43 * matrixTwo.M33 + matrixOne.M44 * matrixTwo.M43,
                M44 = matrixOne.M41 * matrixTwo.M14 + matrixOne.M42 * matrixTwo.M24 + matrixOne.M43 * matrixTwo.M34 + matrixOne.M44 * matrixTwo.M44
            };
        }
    }
    public struct BoneArray
    {
        public TArray Array1;
        public TArray Array2;
    } 
    public enum PlayerBone
    {
        Head = 29,
        Spine1 = 2,
        Spine2 = 3,
        Spine3 = 4,
        Neck01 = 28,

        LClavicle = 5,
        RClavicle = 32,

        LUpperArm = 6,
        LLowerArm = 7,
        LHand = 8,
        RUpperArm = 33,
        RLowerArm = 34,
        RHand = 37,

        Pelvis = 1,

        LThigh = 55,
        LCalf = 56,
        LFoot = 57,

        RThigh = 61,
        RCalf = 62,
        RFoot = 63,
    }

    public static class SkeletonHelper
    {
        public static readonly List<(PlayerBone, PlayerBone)> BoneConnections = new()
        {
            (PlayerBone.Head, PlayerBone.Neck01),
            (PlayerBone.Neck01, PlayerBone.Spine3),
            (PlayerBone.Spine3, PlayerBone.Spine2),
            (PlayerBone.Spine2, PlayerBone.Spine1),

            (PlayerBone.Neck01, PlayerBone.LUpperArm),
            (PlayerBone.LUpperArm, PlayerBone.LLowerArm),
            (PlayerBone.LLowerArm, PlayerBone.LHand),

            (PlayerBone.Neck01, PlayerBone.RUpperArm),
            (PlayerBone.RUpperArm, PlayerBone.RLowerArm),
            (PlayerBone.RLowerArm, PlayerBone.RHand),

            (PlayerBone.Spine1, PlayerBone.LThigh),
            (PlayerBone.LThigh, PlayerBone.LCalf),
            (PlayerBone.LCalf, PlayerBone.LFoot),

            (PlayerBone.Spine1, PlayerBone.RThigh),
            (PlayerBone.RThigh, PlayerBone.RCalf),
            (PlayerBone.RCalf, PlayerBone.RFoot),
        };

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FGameplayAttributeData
    {
        [FieldOffset(0x8)]
        public float BaseValue;
        [FieldOffset(0xC)]
        public float CurrentValue;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FGamePlayAttributeDataSet
    {
        [FieldOffset(0x30)]
        public FGameplayAttributeData Strength;
        [FieldOffset(0x60)]
        public FGameplayAttributeData Vigor;
        [FieldOffset(0x90)]
        public FGameplayAttributeData Agility;
        [FieldOffset(0xC0)]
        public FGameplayAttributeData Dexterity;
        [FieldOffset(0xF0)]
        public FGameplayAttributeData Will;
        [FieldOffset(0x120)]
        public FGameplayAttributeData Knowledge;
        [FieldOffset(0x150)]
        public FGameplayAttributeData Resourcefulness;
        [FieldOffset(0x180)]
        public FGameplayAttributeData PhysicalDamageWeaponPrimary;
        [FieldOffset(0x1A0)]
        public FGameplayAttributeData PhysicalDamageBase;
        [FieldOffset(0x1B0)]
        public FGameplayAttributeData PhysicalPower;
        [FieldOffset(0x230)]
        public FGameplayAttributeData ArmorRating;
        [FieldOffset(0x870)]
        public FGameplayAttributeData MemoryPayload;
        [FieldOffset(0x7A8)]
        public FGameplayAttributeData Health;
        [FieldOffset(0x7C8)]
        public FGameplayAttributeData MaxHealth;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FAccountDataReplication
    {
        [FieldOffset(0x0)]
        public FString AccountId;
        [FieldOffset(0x10)]
        public FNickname Nickname;
        [FieldOffset(0x58)]
        public FString PartyId;
        [FieldOffset(0x7C)]
        public int Level;
    }
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MinimalViewInfo
    {
        [FieldOffset(0x0)]
        public FVector3 Location;
        [FieldOffset(0x18)]
        public FRotator Rotation;
        [FieldOffset(0x30)]
        public float FOV;
    }
    public struct VectorsFromRot
    {
        public Vector3 AxisX, AxisY, AxisZ;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct  FPrimaryAssetId
    {
        [FieldOffset(0x0)]
        public FPrimaryAssetType PrimaryAssetType;
        [FieldOffset(0x8)]
        public FName PrimaryAssetName;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FPrimaryAssetType
    {
        [FieldOffset(0x0)]
        public FName Name;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FDCItemInfo
    {
        [FieldOffset(0xB0)]
        public FItemData ItemData;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FItemData
    {
        [FieldOffset(0x8)]
        public FPrimaryAssetId ItemId;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct FText
    {
        [FieldOffset(0x0)]
        public FString Text;
    }
    //hi
}
