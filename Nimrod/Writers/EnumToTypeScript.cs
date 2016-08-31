﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Nimrod
{
    public abstract class EnumToTypeScript : ToTypeScript
    {
        public string TsName => this.Type.ToTypeScript();

        protected abstract IEnumerable<string> GetHeader();
        protected abstract IEnumerable<string> GetFooter();
        protected abstract IEnumerable<string> GetHeaderDescription();
        protected abstract IEnumerable<string> GetFooterDescription();
        public EnumToTypeScript(Type type) : base(type)
        {
            if (!this.Type.IsEnum)
            {
                throw new ArgumentException($"{this.Type.Name} is not an System.Enum.", nameof(type));
            }
            var underlyingType = Enum.GetUnderlyingType(this.Type);
            if (underlyingType != typeof(int))
            {
                throw new NotSupportedException($"Unsupported underlying type for enums in typescript [{underlyingType}]. Only ints are supported.");
            }
        }

        public override IEnumerable<string> GetLines()
        {
            return new[] {
                this.GetHeader(),
                this.GetBody(),
                this.GetFooter(),
                this.GetHeaderDescription(),
                this.GetBodyDescription(),
                this.GetFooterDescription()
            }.SelectMany(line => line);
        }

        public IEnumerable<string> GetBodyDescription() => new[] {
            $@"static getDescription(item: {this.TsName}): string {{
                switch (item) {{{
                    this.Type.GetEnumValues()
                        .OfType<object>()
                        .Select(enumValue =>
                        {
                            var description = EnumExtensions.GetDescription(enumValue);
                            var enumName = this.Type.GetEnumName(enumValue);
                            return $"case {this.TsName}.{enumName}: return '{description}';";
                        }).JoinNewLine()}
                    default: return item.toString();
                }}
            }}"
        };

        public IEnumerable<string> GetBody() =>
            this.Type.GetEnumValues().OfType<object>()
                     .Select(enumValue =>
                     {
                         var enumName = this.Type.GetEnumName(enumValue);
                         return $"{enumName} = {(int)enumValue},";
                     });
    }
}
