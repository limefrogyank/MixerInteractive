﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.State
{
    public class GridPlacement
    {
        public string Size { get; set; }  //'large' | 'medium' | 'small'
        public double Width { get; set; }
        public double Height { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
