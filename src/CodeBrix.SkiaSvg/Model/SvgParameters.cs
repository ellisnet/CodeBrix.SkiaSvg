// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.Model; //Was previously: namespace Svg.Model;

public readonly record struct SvgParameters(Dictionary<string, string> Entities, string Css);
