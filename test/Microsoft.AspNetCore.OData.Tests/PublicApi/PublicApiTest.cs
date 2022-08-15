//-----------------------------------------------------------------------------
// <copyright file="PublicApiTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.PublicApi
{
    public partial class PublicApiTest
    {
        private const string AssemblyName = "Microsoft.AspNetCore.OData.dll";
        private const string OutputFileName = "Microsoft.AspNetCore.OData.PublicApi.out";

#if NET5_0
        private const string BaseLineFileName = "Microsoft.AspNetCore.OData.PublicApi.Net5.bsl";
#elif NET6_0
        private const string BaseLineFileName = "Microsoft.AspNetCore.OData.PublicApi.Net6.bsl";
#else
        private const string BaseLineFileName = "Microsoft.AspNetCore.OData.PublicApi.NetCore31.bsl";
#endif

        private const string BaseLineFileFolder = @"test\Microsoft.AspNetCore.OData.Tests\PublicApi\";

        [Fact]
        public void TestPublicApi()
        {
            // Arrange
            string outputPath = Environment.CurrentDirectory;
            string outputFile = outputPath + Path.DirectorySeparatorChar + OutputFileName;

            // Act
            using (FileStream fs = new FileStream(outputFile, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string assemblyPath = outputPath + Path.DirectorySeparatorChar + AssemblyName;
                    Assert.True(File.Exists(assemblyPath), string.Format("{0} does not exist in current directory", assemblyPath));
                    PublicApiHelper.DumpPublicApi(sw, assemblyPath);
                }
            }
            string outputString = File.ReadAllText(outputFile);
            string baselineString = GetBaseLineString();

            // Assert
            Assert.True(String.Compare(baselineString, outputString, StringComparison.Ordinal) == 0,
                String.Format("Base line file '{1}' and output file '{2}' do not match, please check.'{0}'" +
                "To update the baseline, please run:{0}{0}" +
                "copy /y \"{2}\" \"{1}\"", Environment.NewLine,
                BaseLineFileFolder + BaseLineFileName,
                outputFile));
        }

        private string GetBaseLineString()
        {
            const string projectDefaultNamespace = "Microsoft.AspNetCore.OData.Tests";
            const string resourcesFolderName = "PublicApi";
            const string pathSeparator = ".";
            string path = projectDefaultNamespace + pathSeparator + resourcesFolderName + pathSeparator + BaseLineFileName;

            using (Stream stream = typeof(PublicApiTest).Assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    string message = Error.Format("The embedded resource '{0}' was not found.", path);
                    throw new FileNotFoundException(message, path);
                }

                using (TextReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
