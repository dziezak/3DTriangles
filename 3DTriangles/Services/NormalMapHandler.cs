using System;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace YourAppNamespace.Rendering
{
    public class NormalMapHandler
    {
        private WriteableBitmap? _normalMap;
        private byte[]? _normalMapBytes;
        private int _stride;
        private const int BytesPerPixel = 4;

        public bool IsEnabled { get; set; } = false;

        public void LoadNormalMap(WriteableBitmap normalMap)
        {
            _normalMap = normalMap;
            _stride = normalMap.BackBufferStride;
            _normalMapBytes = new byte[_stride * normalMap.PixelHeight];
            normalMap.CopyPixels(_normalMapBytes, _stride, 0);
        }

        public Vector3 ApplyNormalMap(Vector3 Npow, Vector3 Pu, Vector3 Pv, float u, float v)
        {
            if (!IsEnabled || _normalMap == null || _normalMapBytes == null)
                return Npow;

            int texX = (int)(u * (_normalMap.PixelWidth - 1));
            int texY = (int)((1 - v) * (_normalMap.PixelHeight - 1));
            texX = Math.Clamp(texX, 0, _normalMap.PixelWidth - 1);
            texY = Math.Clamp(texY, 0, _normalMap.PixelHeight - 1);

            int texIndex = texY * _stride + texX * BytesPerPixel;
            if (texIndex + 2 >= _normalMapBytes.Length)
                return Npow;

            byte r = _normalMapBytes[texIndex + 2];
            byte g = _normalMapBytes[texIndex + 1];
            byte b = _normalMapBytes[texIndex + 0];

            Vector3 Ntex = new Vector3(
                (r / 255f) * 2f - 1f,
                (g / 255f) * 2f - 1f,
                (b / 255f)
            );
            Ntex = Vector3.Normalize(Ntex);

            Matrix4x4 M = new Matrix4x4(
                Pu.X, Pv.X, Npow.X, 0,
                Pu.Y, Pv.Y, Npow.Y, 0,
                Pu.Z, Pv.Z, Npow.Z, 0,
                0, 0, 0, 1
            );

            Vector3 Nmod = Vector3.TransformNormal(Ntex, M);
            return Vector3.Normalize(Nmod);
        }
    }
}
