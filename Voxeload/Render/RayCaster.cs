using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public static class RayCaster
    {
        public static (Vector3i pos, Tile.Face face)? CastIntoWorld(Level level, int layer, Vector3 origin, float rotX, float rotY, float maxDistance, float step)
        {
            Vector4 point = new(origin, 1.0f);
            Vector4 offset = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotY)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotX)) * new Vector4(0, 0, -step, 1.0f);
            Tile.Face lastFaceCrossed = Tile.Face.North;

            float distance = 0;
            while (distance < maxDistance)
            {
                if (level.GetTileID(layer, (int)point.X, (int)point.Y, (int)point.Z) != 0)
                {
                    return (new Vector3i((int)point.X, (int)point.Y, (int)point.Z), lastFaceCrossed);
                }

                Vector4 newPoint = point + offset;

                if ((int)point.X != (int)newPoint.X)
                {
                    if (newPoint.X > point.X) lastFaceCrossed = Tile.Face.West;
                    else lastFaceCrossed = Tile.Face.East;
                }
                else if ((int)point.Y != (int)newPoint.Y)
                {
                    if (newPoint.Y > point.Y) lastFaceCrossed = Tile.Face.Bottom;
                    else lastFaceCrossed = Tile.Face.Top;
                }
                else if ((int)point.Z != (int)newPoint.Z)
                {
                    if (newPoint.Z > point.Z) lastFaceCrossed = Tile.Face.North;
                    else lastFaceCrossed = Tile.Face.South;
                }

                point = newPoint;
                distance += step;
            }

            return null;
        }
    }
}
