using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using DMAW_DND.Source.Classes;

namespace DMAW_DND
{
    internal class EspWindow : GameWindow
    {
        public unsafe EspWindow(int monitorIndex, int gameWidth, int gameHeight) : base(
            new GameWindowSettings
        {
            Win32SuspendTimerOnDrag = false,
            UpdateFrequency = 60.0,
        }, 
            new NativeWindowSettings
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(gameWidth, gameHeight),
            APIVersion = new Version(4, 4),
            WindowState = WindowState.Normal,
            Title = "ESP",
            //Flags = ContextFlags.Offscreen,
            Vsync = VSyncMode.Off
        })
        {
            WindowState = WindowState.Maximized;
            WindowBorder = WindowBorder.Hidden;
            GLFW.SetWindowAttrib(this.WindowPtr, WindowAttribute.Floating, true);
            Thread.Sleep(100);
            var monitor = GLFW.GetMonitors()[monitorIndex];
            GLFW.GetMonitorWorkarea(monitor, out int x, out int y, out int width, out int height);
            GLFW.SetWindowMonitor(this.WindowPtr, monitor, 0, 0, width, height, 60);

            //IntPtr hWnd = GLFW.GetWin32Window(this.WindowPtr);

            //WindowHelper.SetUnfocusedStyles(hWnd);
        }

        public static FVector2 WorldToScreen(FVector3 worldPos, MinimalViewInfo cameraCache, int screenWidth, int screenHeight)
        {
            Matrix4x4 tempMatrix = CreateMatrix(cameraCache.Rotation, new Vector3(0, 0, 0));

            FVector3 vAxisX, vAxisY, vAxisZ;

            vAxisX = new FVector3(tempMatrix.M11, tempMatrix.M12, tempMatrix.M13);
            vAxisY = new FVector3(tempMatrix.M21, tempMatrix.M22, tempMatrix.M23);
            vAxisZ = new FVector3(tempMatrix.M31, tempMatrix.M32, tempMatrix.M33);

            FVector3 vDelta = worldPos - cameraCache.Location;
            FVector3 vTransformed = new FVector3(vDelta.Dot(vAxisY), vDelta.Dot(vAxisZ), vDelta.Dot(vAxisX));
            if (vTransformed.Z < 1d)
            {
                vTransformed.Z = 1d;
            }

            float ScreenCenterX = screenWidth / 2.0f;
            float ScreenCenterY = screenHeight / 2.0f;

            FVector2 screenPos = new FVector2()
            {
                X = (uint)(ScreenCenterX + vTransformed.X * (ScreenCenterX / Math.Tan((cameraCache.FOV + 1f) * (float)Math.PI / 360f)) / vTransformed.Z),
                Y = (uint)(ScreenCenterY - vTransformed.Y * (ScreenCenterX / Math.Tan((cameraCache.FOV + 1f) * (float)Math.PI / 360f)) / vTransformed.Z)
            };
            return screenPos;
        }
        public static Matrix4x4 CreateMatrix(FRotator rot, Vector3 origin)
        {
            double radPitch = ((rot.Pitch * (float)Math.PI / 180.0f));
            double radYaw = ((rot.Yaw * (float)Math.PI / 180.0f));
            double radRoll = ((rot.Roll * (float)Math.PI / 180.0f));

            double SP = Math.Sin(radPitch);
            double CP = Math.Cos(radPitch);
            double SY = Math.Sin(radYaw);
            double CY = Math.Cos(radYaw);
            double SR = Math.Sin(radRoll);
            double CR = Math.Cos(radRoll);

            Matrix4x4 matrix = new Matrix4x4();

            matrix.M11 = (float)(CP * CY);
            matrix.M12 = (float)(CP * SY);
            matrix.M13 = (float)SP;
            matrix.M14 = 0.0f;

            matrix.M21 = (float)(SR * SP * CY - CR * SY);
            matrix.M22 = (float)(SR * SP * SY + CR * CY);
            matrix.M23 = (float)(-SR * CP);
            matrix.M24 = 0.0f;

            matrix.M31 = (float)(-(CR * SP * CY + SR * SY));
            matrix.M32 = (float)(CY * SR - CR * SP * SY);

            matrix.M33 = (float)(CR * CP);
            matrix.M34 = 0.0f;

            matrix.M41 = origin.X;
            matrix.M42 = origin.Y;
            matrix.M43 = origin.Z;
            matrix.M44 = 1.0f;

            return matrix;
        }
        private static VectorsFromRot ComputeVectorsFromRot(Vector3 rot)
        {
            float radPitch = rot.X * (float)Math.PI / 180.0f;
            float radYaw = rot.Y * (float)Math.PI / 180.0f;
            float radRoll = rot.Z * (float)Math.PI / 180.0f;

            float SP = (float)Math.Sin(radPitch);
            float CP = (float)Math.Cos(radPitch);
            float SY = (float)Math.Sin(radYaw);
            float CY = (float)Math.Cos(radYaw);
            float SR = (float)Math.Sin(radRoll);
            float CR = (float)Math.Cos(radRoll);

            VectorsFromRot result;
            result.vAxisX = new Vector3(CP * CY, CP * SY, SP);
            result.vAxisY = new Vector3(SR * SP * CY - CR * SY, SR * SP * SY + CR * CY, -SR * CP);
            result.vAxisZ = new Vector3(-(CR * SP * CY + SR * SY), CY * SR - CR * SP * SY, CR * CP);

            return result;
        }

        struct VectorsFromRot
        {
            public Vector3 vAxisX, vAxisY, vAxisZ;
        }



        
    }
}
