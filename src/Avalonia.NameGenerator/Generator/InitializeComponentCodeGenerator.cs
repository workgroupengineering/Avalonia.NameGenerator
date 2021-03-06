﻿using System.Collections.Generic;
using Avalonia.NameGenerator.Domain;
using XamlX.TypeSystem;

namespace Avalonia.NameGenerator.Generator
{
    internal class InitializeComponentCodeGenerator: ICodeGenerator
    {
        private readonly bool _attachDevTools;
        private const string AttachDevToolsCodeBlock = @"
#if DEBUG
            this.AttachDevTools();
#endif
";

        public InitializeComponentCodeGenerator(IXamlTypeSystem types)
        {
            _attachDevTools = types.FindAssembly("Avalonia.Diagnostics") != null;
        }

        public string GenerateCode(string className, string nameSpace, IEnumerable<ResolvedName> names)
        {
            var properties = new List<string>();
            var initializations = new List<string>();
            foreach (var (typeName, name, fieldModifier) in names)
            {
                properties.Add($"        {fieldModifier} global::{typeName} {name} {{ get; set; }}");
                initializations.Add($"            {name} = this.FindControl<global::{typeName}>(\"{name}\");");
            }

            var devToolsBlock = _attachDevTools ? AttachDevToolsCodeBlock : string.Empty;
            return $@"// <auto-generated />

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace {nameSpace}
{{
    partial class {className}
    {{
{string.Join("\n", properties)}

        public void InitializeComponent(bool loadXaml = true)
        {{
            if (loadXaml)
            {{
                AvaloniaXamlLoader.Load(this);
            }}
{devToolsBlock}
{string.Join("\n", initializations)}
        }}
    }}
}}
";
        }
    }
}