﻿namespace Maestro
{
    internal class Option
    {
        public string Default { get; set; }
        public string Description { get; set; }
        public bool IsFlag { get; set; } = false;
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public bool Required { get; set; } = false;
        public string ValuePlaceholder { get; set; } = "";
    }
}