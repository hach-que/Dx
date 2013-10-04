using System;
using Xunit;

//
// Use the NuGet package manager to reference Xunit for this
// project.  You can do this from MonoDevelop by right-clicking
// the project and going "Manage NuGet Packages..." and
// searching for "Xunit".
//
// If you don't see the NuGet dropdown, you might not have the
// add-on installed.  See https://github.com/mrward/monodevelop-nuget-addin
// for instructions on how to install the NuGet plugin for
// MonoDevelop.
//

namespace Dx.Runtime.Tests
{
    public class ClassTests
    {
        [Fact]
        public void TestTrue()
        {
            Assert.Equal(true, true);
        }
    }
}