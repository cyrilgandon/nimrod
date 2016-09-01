﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Nimrod.Writers.Module
{
    public class ModelToModuleTypeScript : ModelToTypeScript
    {
        public ModelToModuleTypeScript(Type type) : base(type) { }

        protected override IEnumerable<string> GetHeader()
        {
            var genericArguments = this.Type.GetGenericArguments().ToHashSet();
            var propertyTypes = this.Type.GetProperties()
                                    .Select(p => p.PropertyType)
                                    .Where(p => !genericArguments.Contains(p));
            var imports = ModuleHelper.GetTypesToImport(propertyTypes)
                                .Where(t => !genericArguments.Contains(t))
                                .Select(t => ModuleHelper.GetImportLine(t));

            return imports.Concat(new[] {
                $"interface {TsName} {{"
            });
        }

        protected override IEnumerable<string> GetFooter()
        {
            var nonGenericTypescriptClass = this.Type.ToTypeScript(false, false);
            return new[] {
                $"}}",
                // no generic for export on require mode
                $"export default {nonGenericTypescriptClass};"};
        }
    }
}