using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Evacuation.Application.Imaging
{
    public class Plan
    {
        int width;
        int height;
        public int Width { get => width; }
        public int Height { get => height; }
        PixelKind[] plan;
        public Plan(int width, int height)
        {
            this.width = width;
            this.height = height;
            plan = new PixelKind[width * height];
        }

        public PixelKind this[int x, int y]
        {
            get => plan[y * width + x];
            set => plan[y * width + x] = value;
        }
    }
}