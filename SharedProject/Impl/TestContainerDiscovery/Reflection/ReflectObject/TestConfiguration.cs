﻿using System.Collections.Generic;
using ReflectObject;

namespace FineCodeCoverage.Impl
{
    public class TestConfiguration : ReflectObjectProperties
    {
        public TestConfiguration(object toReflect) : base(toReflect) { }
        public TestConfiguration() { } // tests
        public object UserRunSettings { get; set; }
        public IEnumerable<Container> Containers { get; set; }
        public string SolutionDirectory { get; set; }
    }
}
