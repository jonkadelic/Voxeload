using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public abstract class BaseTileModels
    {
        public abstract TileModel GetModel(byte sides);

        public abstract byte[] GetColours(ITileAppearance appearance, byte sides);
    }
}
