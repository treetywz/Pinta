//
// PlainBrush.cs
//
// Author:
//       Aaron Bockover <abockover@novell.com>
// 
// Copyright (c) 2010 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using Cairo;
using Pinta.Core;
namespace Pinta.Tools.Brushes;
internal sealed class PlainBrush : BasePaintBrush
{
private readonly WorkspaceManager workspace;

private readonly PercentOption hardness_option = new (
	"plain-brush-hardness", 100, Translations.GetString ("Hardness"));

public override List<ToolOption> Options { get; protected set; }

internal PlainBrush (WorkspaceManager workspace)
    {
this.workspace = workspace;
Options = [hardness_option];
    }
public override string Name => Translations.GetString ("Normal");
public override int Priority => -100;

private PointI? last_stamp_position;
private double leftover_distance = 0;

protected override RectangleI OnMouseMove (
Context g,
ImageSurface surface,
BrushStrokeArgs strokeArgs)
    {
PointI from = last_stamp_position ?? strokeArgs.LastPosition;
PointI to = strokeArgs.CurrentPosition;

double radius = g.LineWidth / 2.0;
if (radius < 0.5)
	radius = 0.5;

double hardness = hardness_option.Value / 100.0;

double dx = to.X - from.X;
double dy = to.Y - from.Y;
double segmentDistance = Math.Sqrt (dx * dx + dy * dy);

double spacing = Math.Max (radius * 0.25, 1.0);

int minX = Math.Min (from.X, to.X);
int minY = Math.Min (from.Y, to.Y);
int maxX = Math.Max (from.X, to.X);
int maxY = Math.Max (from.Y, to.Y);
int pad = (int) Math.Ceiling (radius) + 2;

RectangleI dirty = new (
	minX - pad,
	minY - pad,
	(maxX - minX) + pad * 2,
	(maxY - minY) + pad * 2);

if (segmentDistance > 0) {
	double traveled = leftover_distance;
	double t = 0;

	while (traveled + segmentDistance * (1 - t) >= spacing) {
		double remaining = spacing - traveled;
		t += remaining / segmentDistance;

		double px = from.X + dx * t;
		double py = from.Y + dy * t;

		StampDab (g, px, py, radius, hardness, strokeArgs.StrokeColor, strokeArgs.UseAntialiasing);

		traveled = 0;
	}

	leftover_distance = traveled + segmentDistance * (1 - t);
} else if (last_stamp_position is null) {
	// No movement yet this stroke -- treat as a single click, stamp once.
	StampDab (g, to.X, to.Y, radius, hardness, strokeArgs.StrokeColor, strokeArgs.UseAntialiasing);
}

last_stamp_position = to;

return dirty;
    }

private static void StampDab (Context g, double cx, double cy, double radius, double hardness, Color color, bool useAntialiasing)
    {
g.Save ();

if (!useAntialiasing) {
	g.Antialias = Antialias.None;
	g.SetSourceColor (color);
	g.Arc (cx, cy, radius, 0, 2 * Math.PI);
	g.Fill ();
} else {
	double hardnessCurved = Math.Pow (hardness, 32);
	double minFalloff = 1.0;

	double innerRadius = radius * hardnessCurved;
	if (innerRadius > radius - minFalloff)
		innerRadius = radius - minFalloff;
	if (innerRadius < 0)
		innerRadius = 0;

	RadialGradient gradient = new (
		cx, cy, innerRadius,
		cx, cy, radius);

	gradient.AddColorStop (0, color);
	gradient.AddColorStop (1, new Color (color.R, color.G, color.B, 0));

	g.SetSource (gradient);
	g.Arc (cx, cy, radius, 0, 2 * Math.PI);
	g.Fill ();

	gradient.Dispose ();
}

g.Restore ();
    }

protected override void OnMouseUp ()
    {
last_stamp_position = null;
leftover_distance = 0;
    }
}