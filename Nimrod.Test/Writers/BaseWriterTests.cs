﻿using Nimrod.Test.ModelExamples;
using Nimrod.Writers.Default;
using Nimrod.Writers.Require;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nimrod.Test
{
    [TestFixture]
    public class BaseWriterTests
    {
        public class GenericItem<T>
        {
            public T Item { get; set; }
        }
        public class GenericWrapper<T>
        {
            public GenericItem<T> Item
            {
                get; set;
            }
        }

        [Test]
        public void WriteModel_GetImports()
        {
            var dependencies = new[] { typeof(GenericWrapper<GenericItem<int>>) };
            var imports = RequireModuleHelper.GetTypesToImport(dependencies)
                .OrderBy(s => s.Name)
                .ToList();

            Assert.AreEqual(2, imports.Count);
            Assert.AreEqual(typeof(GenericItem<>), imports[0]);
            Assert.AreEqual(typeof(GenericWrapper<>), imports[1]);
        }

        [Test]
        public void WriteModel_WriteImports()
        {
            Assert.AreEqual("import IGenericItem = require('./Nimrod.Test.GenericItem');", RequireModuleHelper.GetImportLine(typeof(GenericItem<>)));
            Assert.AreEqual("import IGenericWrapper = require('./Nimrod.Test.GenericWrapper');", RequireModuleHelper.GetImportLine(typeof(GenericWrapper<>)));
        }

        [Test]
        public void GetTypescriptType_GenericListContainer()
        {
            var genericTypeDefinition = typeof(BarWrapper<int>).GetGenericTypeDefinition().ToTypeScript();
            var writer = new ModelToDefaultTypeScript(genericTypeDefinition);
            string ts = writer.GetLines().JoinNewLine();
            Assert.IsTrue(ts.Contains("Bars: (T | null)[] | null;"));
        }

        [Test]
        public void GetTypescriptType_GenericCustomContainer()
        {
            var genericTypeDefinition = typeof(Fuzz<int>).GetGenericTypeDefinition().ToTypeScript();
            var writer = new ModelToDefaultTypeScript(genericTypeDefinition);

            string ts = writer.GetLines().JoinNewLine();
            Assert.IsTrue(ts.Contains("Fuzzs: Nimrod.Test.IGenericFoo<T> | null;"));
        }

    }
}

