﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Web.Mvc;
using Nimrod.Test.ModelExamples;

namespace Nimrod.Test
{
    public class TypeDiscoveryTests
    {
        public class HttpTestAttribute : Attribute
        {
        }
        public class NoTypescriptDetactableController : Controller
        {
            [HttpTest]
            public JsonNetResult<bool> TestAction(string stringValue)
            {
                throw new NotImplementedException();
            }

            public JsonNetResult<bool> TestAction2(string stringValue)
            {
                throw new NotImplementedException();
            }
        }


        public class GenericFoo<T>
        {
            public T Property { get; set; }
        }

        public class GenericReturnController : Controller
        {
            [HttpGet]
            public JsonNetResult<GenericFoo<int>> TestAction(string stringValue)
            {
                throw new NotImplementedException();
            }
        }

        public class WebApiController : System.Web.Http.ApiController
        {
            [System.Web.Http.HttpGet]
            public string TestAction()
            {
                throw new NotImplementedException();
            }
        }

        public class Cat { public Dog Dog { get; } }
        public class Dog { public Cat Cat { get; } }

        [Test]
        public void EnumerateTypesTest_LoopReference()
        {
            var result = TypeDiscovery.EnumerateTypes(typeof(Cat))
                         .OrderBy(t => t.Name)
                         .ToList();

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(typeof(Cat), result[0]);
            Assert.AreEqual(typeof(Dog), result[1]);
            Assert.AreEqual(typeof(object), result[2]);
        }


        [Test]
        public void EnumerateTypesTest_string()
        {
            var result = TypeDiscovery.EnumerateTypes(typeof(string))
                         .OrderBy(t => t.Name)
                         .ToList();

            Assert.AreEqual(4, result.Count);

            Assert.AreEqual(typeof(int), result[0]);
            Assert.AreEqual(typeof(object), result[1]);
            Assert.AreEqual(typeof(string), result[2]);
            Assert.AreEqual(typeof(ValueType), result[3]);
        }

        public class Nothing { }

        public class GenericContainer
        {
            public GenericFoo<Nothing> Property { get; set; }
        }
        [Test]
        public void EnumerateTypes_ShouldReturnGenericContainer()
        {
            var result = TypeDiscovery.EnumerateTypes(typeof(GenericContainer)).ToList();
            Assert.IsTrue(result.Contains(typeof(GenericFoo<Nothing>)));
        }
    }
}
