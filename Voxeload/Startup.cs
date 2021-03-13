using OpenTK.Windowing.Desktop;
using System;

namespace Voxeload
{
    class Startup
    {
        static void Main(string[] args)
        {
            GameWindowSettings gws = GameWindowSettings.Default;
            gws.IsMultiThreaded = true;
            gws.RenderFrequency = 60.0f;
            gws.UpdateFrequency = 60.0f;

            using Voxeload vox = new(gws, NativeWindowSettings.Default);

            vox.Run();
        }
    }
}
