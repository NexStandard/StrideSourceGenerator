﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrideSourceGenerator.API;
interface ITemplateProvider
{
    public MemberDeclarationSyntax GetTemplate(string value);
}